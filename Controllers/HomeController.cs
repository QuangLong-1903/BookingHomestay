using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCoSo_Nhom2.Data;
using DoAnCoSo_Nhom2.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Linq;
using DoAnCoSo_Nhom2.Service.TimeService;

namespace DoAnCoSo_Nhom2.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeService _timeService; 

        public HomeController(ApplicationDbContext context, TimeService timeService) 
        {
            _context = context;
            _timeService = timeService;
        }

        public IActionResult Index()
        {
            var homestays = _context.Homestays
                .Where(h => h.IsApproved)
                .Include(h => h.HomestayAmenities).ThenInclude(ha => ha.Amenity)
                .Include(h => h.Reviews)
                .ToList();

            return View(homestays);
        }

        public IActionResult Search(string name, string city, decimal? maxPrice)
        {
            var homestays = _context.Homestays
                .Where(h => h.IsApproved)
                .AsQueryable();

            if (!string.IsNullOrEmpty(name))
                homestays = homestays.Where(h => h.Name.Contains(name));

            if (!string.IsNullOrEmpty(city))
                homestays = homestays.Where(h => h.City.Contains(city));

            if (maxPrice.HasValue)
                homestays = homestays.Where(h => h.PricePerNight <= maxPrice);

            return View(homestays.ToList());
        }

        public IActionResult Sort(decimal? pricePerNight, string city, int? rating)
        {
            var homestays = _context.Homestays
                .Where(h => h.IsApproved)
                .Include(h => h.Reviews)
                .AsQueryable();

            if (pricePerNight.HasValue)
                homestays = homestays.Where(h => h.PricePerNight <= pricePerNight);

            if (!string.IsNullOrEmpty(city))
                homestays = homestays.Where(h => h.City.Contains(city));

            if (rating.HasValue)
                homestays = homestays.Where(h => h.Reviews.Any() && h.Reviews.Average(r => r.Rating) >= rating);

            ViewBag.PricePerNight = pricePerNight;
            ViewBag.City = city;
            ViewBag.Rating = rating;

            return View(homestays.ToList());
        }

        public IActionResult Details(int id)
        {
            var homestay = _context.Homestays
                .Where(h => h.IsApproved)
                .Include(h => h.HomestayAmenities).ThenInclude(ha => ha.Amenity)
                .Include(h => h.Reviews)
                .FirstOrDefault(h => h.Id == id);

            if (homestay == null)
                return NotFound();

            return View(homestay);
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Contact()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }

        [Authorize]
        public IActionResult OrderHistory()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var orders = _context.Bookings
                .Include(b => b.Homestay)
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.UpdatedAt)
                .ToList();

            return View(orders);
        }
    }
}
