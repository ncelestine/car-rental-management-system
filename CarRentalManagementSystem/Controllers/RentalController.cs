
using System.Security.Claims;
using CarRentalManagementSystem.Data;
using CarRentalManagementSystem.Models;
using CarRentalManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarRentalManagementSystem.Controllers
{
    public class RentalsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RentalsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Browse available cars
        [AllowAnonymous]
        public async Task<IActionResult> Browse(
            string? search,
            string? category,
            bool availableOnly = true,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var carsQuery = _context.Cars.AsQueryable();

            // Inventory-level availability
            carsQuery = carsQuery.Where(c => c.IsAvailable);

            if (!string.IsNullOrWhiteSpace(search))
            {
                carsQuery = carsQuery.Where(c =>
                    c.Make.Contains(search) || c.Model.Contains(search));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                carsQuery = carsQuery.Where(c => c.Category == category);
            }

            DateTime rangeStart = (startDate ?? DateTime.Today).Date;
            DateTime rangeEnd = (endDate ?? rangeStart).Date;

            if (availableOnly)
            {
                carsQuery = carsQuery.Where(c =>
                    !_context.Rentals.Any(r =>
                        r.CarId == c.CarId &&
                        r.Status == "Approved" &&
                        r.StartDate <= rangeEnd &&
                        r.EndDate >= rangeStart
                    )
                );
            }

            ViewBag.Categories = await _context.Cars
                .Select(c => c.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            ViewBag.Search = search ?? "";
            ViewBag.Category = category ?? "";
            ViewBag.AvailableOnly = availableOnly;
            ViewBag.StartDate = rangeStart.ToString("yyyy-MM-dd");
            ViewBag.EndDate = rangeEnd.ToString("yyyy-MM-dd");

            var cars = await carsQuery
                .OrderBy(c => c.Make)
                .ThenBy(c => c.Model)
                .ToListAsync();

            return View(cars);
        }

        // GET: Rentals/Request
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Request(int carId)
        {
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.CarId == carId);
            if (car == null) return NotFound();

            if (!car.IsAvailable)
            {
                TempData["Error"] = "Sorry, that car is not available.";
                return RedirectToAction(nameof(Browse));
            }

            return View(new RentalRequestViewModel
            {
                CarId = car.CarId,
                CarDisplay = $"{car.Make} {car.Model} ({car.Year}) - ${car.DailyRate:0.00}/day"
            });
        }

        // POST: Rentals/Request
        [Authorize(Roles = "Customer")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Request(RentalRequestViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var car = await _context.Cars.FirstOrDefaultAsync(c => c.CarId == vm.CarId);
            if (car == null) return NotFound();

            bool overlapsApproved = await _context.Rentals.AnyAsync(r =>
                r.CarId == car.CarId &&
                r.Status == "Approved" &&
                r.StartDate <= vm.EndDate.Date &&
                r.EndDate >= vm.StartDate.Date
            );

            if (overlapsApproved)
            {
                ModelState.AddModelError("", "Car is already booked for those dates.");
                vm.CarDisplay = $"{car.Make} {car.Model} ({car.Year}) - ${car.DailyRate:0.00}/day";
                return View(vm);
            }

            string identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.IdentityUserId == identityUserId);

            if (customer == null)
            {
                customer = new Customer
                {
                    IdentityUserId = identityUserId,
                    Email = User.Identity?.Name ?? "unknown@example.com",
                    FullName = User.Identity?.Name ?? "Customer",
                    CreatedAt = DateTime.Now
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            int days = (vm.EndDate.Date - vm.StartDate.Date).Days + 1;
            if (days < 1) days = 1;

            var rental = new Rental
            {
                CustomerId = customer.CustomerId,
                CarId = car.CarId,
                StartDate = vm.StartDate.Date,
                EndDate = vm.EndDate.Date,
                RequestedAt = DateTime.Now,
                Status = "Pending",
                TotalCost = days * car.DailyRate,
                IdentityUserId = identityUserId
            };

            _context.Rentals.Add(rental);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyRentals));
        }

        // Customer rentals
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> MyRentals()
        {
            string identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

            var rentals = await _context.Rentals
                .Include(r => r.Car)
                .Where(r => r.IdentityUserId == identityUserId)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            return View(rentals);
        }

        // Admin manage rentals
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Manage(string? status)
        {
            var query = _context.Rentals
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.Status == status);

            return View(await query
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync());
        }

        // Approve rental
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id)
        {
            var rental = await _context.Rentals.FirstOrDefaultAsync(r => r.RentalId == id);
            if (rental == null || rental.Status != "Pending")
                return RedirectToAction(nameof(Manage));

            bool overlapsApproved = await _context.Rentals.AnyAsync(r =>
                r.RentalId != rental.RentalId &&
                r.CarId == rental.CarId &&
                r.Status == "Approved" &&
                r.StartDate <= rental.EndDate &&
                r.EndDate >= rental.StartDate
            );

            if (overlapsApproved)
            {
                TempData["Error"] = "Cannot approve due to overlapping booking.";
                return RedirectToAction(nameof(Manage));
            }

            rental.Status = "Approved";
            rental.DecisionAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage));
        }

        // Reject rental
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var rental = await _context.Rentals.FirstOrDefaultAsync(r => r.RentalId == id);
            if (rental == null) return RedirectToAction(nameof(Manage));

            rental.Status = "Rejected";
            rental.DecisionAt = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Manage));
        }

        // Complete rental
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int id)
        {
            var rental = await _context.Rentals.FirstOrDefaultAsync(r => r.RentalId == id);
            if (rental == null) return RedirectToAction(nameof(Manage));

            rental.Status = "Completed";
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Manage));
        }
    }
}

