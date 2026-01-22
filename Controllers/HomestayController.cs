using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCoSo_Nhom2.Models;
using System.Security.Claims;
using DoAnCoSo_Nhom2.Data;
using Microsoft.AspNetCore.Authorization;
using DoAnCoSo_Nhom2.Service.TimeService;

namespace DoAnCoSo_Nhom2.Controllers
{
    public class HomestayController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TimeService _timeService;
        public HomestayController(ApplicationDbContext context, TimeService timeService)
        {
            _context = context;
            _timeService = timeService;
        }

        public IActionResult Details(int id)
        {
            var homestay = _context.Homestays.Where(h => h.IsApproved).Include(h => h.HomestayAmenities).ThenInclude(ha => ha.Amenity).Include(h => h.Reviews)
                .FirstOrDefault(h => h.Id == id);
            if (homestay == null)
            {
                return NotFound();
            }

            return View(homestay);
        }

        public IActionResult Search(string name, string city, decimal? maxPrice)
        {
            var homestays = _context.Homestays.Where(h => h.IsApproved).AsQueryable();

            if (!string.IsNullOrEmpty(name))
            {
                homestays = homestays.Where(h => h.Name.Contains(name));
            }

            if (!string.IsNullOrEmpty(city))
            {
                homestays = homestays.Where(h => h.City.Contains(city));
            }
            if (maxPrice.HasValue)
            {
                homestays = homestays.Where(h => h.PricePerNight <= maxPrice);
            }

            return View(homestays.ToList());
        }

        public IActionResult Sort(decimal? PricePerNight, string City, int? Rating)
        {
            var homestays = _context.Homestays.Where(h => h.IsApproved).Include(h => h.Reviews).AsQueryable();

            if (PricePerNight.HasValue)
            {
                homestays = homestays.Where(h => h.PricePerNight <= PricePerNight);
            }

            if (!string.IsNullOrEmpty(City))
            {
                homestays = homestays.Where(h => h.City.Contains(City));
            }

            if (Rating.HasValue)
            {
                homestays = homestays.Where(h => h.Reviews.Any() && h.Reviews.Average(r => r.Rating) >= Rating);
            }

            ViewBag.PricePerNight = PricePerNight;
            ViewBag.City = City;
            ViewBag.Rating = Rating;

            return View("Sort", homestays.ToList());
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Rate(int homestayId, int rating, string comment)
        {
            var homestay = _context.Homestays.Where(h => h.IsApproved).FirstOrDefault(h => h.Id == homestayId);
            if (homestay == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }
            if (rating < 1 || rating > 5)
            {
                ModelState.AddModelError("rating", "Đánh giá phải từ 1 đến 5 sao.");
            }

            if (_context.Reviews.Any(r => r.HomestayId == homestayId && r.UserId == userId))
            {
                ModelState.AddModelError("", "Bạn đã đánh giá Homestay này rồi.");
            }

            if (!ModelState.IsValid)
            {
                homestay.Reviews = _context.Reviews.Where(r => r.HomestayId == homestayId).ToList();
                return View("Details", homestay);
            }

            var review = new Review
            {
                HomestayId = homestayId,
                UserId = userId,
                Rating = rating,
                Comment = comment,
                CreatedAt = _timeService.Now()
            };

            try
            {
                _context.Reviews.Add(review);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi lưu đánh giá: {ex.Message}");
                homestay.Reviews = _context.Reviews.Where(r => r.HomestayId == homestayId).ToList();
                return View("Details", homestay);
            }

            return RedirectToAction("Details", new { id = homestayId });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReportReview(int reviewId, string reason)
        {
            var review = _context.Reviews
                .Include(r => r.Homestay)
                .FirstOrDefault(r => r.Id == reviewId);
            if (review == null || !review.Homestay.IsApproved)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            if (_context.Reports.Any(r => r.ReviewId == reviewId && r.UserId == userId))
            {
                TempData["HomestayError"] = "Bạn đã báo cáo đánh giá này rồi."; 
                return RedirectToAction("Details", new { id = review.HomestayId });
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["HomestayError"] = "Vui lòng cung cấp lý do báo cáo."; 
                return RedirectToAction("Details", new { id = review.HomestayId });
            }

            var report = new Report
            {
                ReviewId = reviewId,
                UserId = userId,
                Reason = reason,
                ReportedAt = DateTime.Now,
                Status = "Pending"
            };

            try
            {
                _context.Reports.Add(report);
                _context.SaveChanges();
                TempData["HomestaySuccess"] = "Báo cáo của bạn đã được gửi đi và đang chờ xử lý."; 
            }
            catch (Exception ex)
            {
                TempData["HomestayError"] = $"Lỗi khi gửi báo cáo: {ex.Message}";
            }

            return RedirectToAction("Details", new { id = review.HomestayId });
        }
    }
}