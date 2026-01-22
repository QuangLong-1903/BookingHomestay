using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCoSo_Nhom2.Models;
using DoAnCoSo_Nhom2.Data;
using DoAnCoSo_Nhom2.Extensions;
using Ganss.Xss;

namespace DoAnCoSo_Nhom2.Controllers
{
    [Authorize(Roles = "Staff")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly HtmlSanitizer _sanitizer;

        public StaffController(ApplicationDbContext context)
        {
            _context = context;
            _sanitizer = new HtmlSanitizer();
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var homestays = _context.Homestays.Where(h => h.StaffId == userId).ToList();
            var bookings = _context.Bookings.Where(b => homestays.Select(h => h.Id).Contains(b.HomestayId)).ToList();
            ViewBag.Notification = bookings.Count(b => b.Status == "Pending");
            return View(homestays);
        }

        public IActionResult CreateHomestay()
        {
            ViewBag.Amenities = _context.Amenities.ToList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateHomestay(Homestay homestay, List<int> amenityIds, IFormFile[] images)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Amenities = _context.Amenities.ToList();
                return View(homestay);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            homestay.StaffId = userId;
            homestay.Images = SaveImages(images);
            homestay.IsApproved = false;
            homestay.Description = _sanitizer.Sanitize(homestay.Description);
            homestay.HomestayAmenities = amenityIds.Select(id => new HomestayAmenity { AmenityId = id }).ToList();

            _context.Homestays.Add(homestay);
            _context.SaveChanges();
            TempData["StaffMessage"] = "Homestay đã được thêm và đang chờ Admin phê duyệt."; // Sử dụng khóa riêng
            return RedirectToAction("Index");
        }

        public IActionResult EditHomestay(int id)
        {
            var homestay = GetHomestayById(id);
            if (homestay == null) return NotFound();

            ViewBag.Amenities = _context.Amenities.ToList();
            return View(homestay);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditHomestay(Homestay homestay, List<int> amenityIds, IFormFile[] images)
        {
            var existingHomestay = GetHomestayById(homestay.Id);
            if (existingHomestay == null) return NotFound();

            homestay.Description = _sanitizer.Sanitize(homestay.Description);
            existingHomestay.UpdateDetails(homestay, SaveImages(images), amenityIds);
            _context.SaveChanges();

            TempData["StaffMessage"] = "Homestay đã được cập nhật.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult DeleteHomestay(int id)
        {
            var homestay = GetHomestayById(id);
            if (homestay == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy Homestay để xóa.";
                return RedirectToAction("Index");
            }

            var hasBookings = _context.Bookings.Any(b => b.HomestayId == id);
            if (hasBookings)
            {
                TempData["ErrorMessage"] = "Không thể xóa Homestay này vì đang có đơn đặt phòng.";
                return RedirectToAction("Index");
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var notification = new DeletedHomestayNotification
                {
                    HomestayName = homestay.Name,
                    StaffId = userId,
                    DeletedAt = DateTime.Now,
                    HomestayId = homestay.Id,
                    Address = homestay.Address,
                    Reason = "Xóa bởi admin, hãy liên lạc chúng tôi"
                };

                _context.DeletedHomestayNotifications.Add(notification);
                _context.Homestays.Remove(homestay);
                _context.SaveChanges();

                TempData["SuccessMessage"] = $"Đã xóa Homestay \"{homestay.Name}\" thành công.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xóa Homestay: " + ex.Message; 
            }

            return RedirectToAction("Index");
        }

        public IActionResult ManageBookings()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bookings = _context.Bookings
                .Include(b => b.Homestay)
                .Include(b => b.User)
                .Where(b => b.Homestay.StaffId == userId)
                .ToList();
            return View(bookings);
        }

        [HttpPost]
        public IActionResult UpdateBookingStatus(int bookingId, string status)
        {
            var booking = _context.Bookings
                .Include(b => b.Homestay)
                .FirstOrDefault(b => b.Id == bookingId && b.Homestay.StaffId == User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (booking == null) return NotFound();

            booking.Status = status;
            booking.UpdatedAt = DateTime.Now;
            _context.SaveChanges();

            TempData["StaffMessage"] = $"Đơn đặt phòng đã được {status.ToLower()}.";
            return RedirectToAction("ManageBookings");
        }

        public IActionResult Notifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var homestays = _context.Homestays
                                    .Where(h => h.StaffId == userId)
                                    .ToList();

            var homestayIds = homestays.Select(h => h.Id).ToList();

            ViewBag.Bookings = _context.Bookings
                                       .Include(b => b.Homestay)
                                       .Include(b => b.User)
                                       .Where(b => homestayIds.Contains(b.HomestayId) && b.Status == "Pending")
                                       .ToList();

            ViewBag.Reviews = _context.Reviews
                                      .Include(r => r.Homestay)
                                      .Include(r => r.User)
                                      .Where(r => homestayIds.Contains(r.HomestayId))
                                      .OrderByDescending(r => r.CreatedAt)
                                      .ToList();

            ViewBag.Homestays = homestays;
            ViewBag.DeletedHomestays = _context.DeletedHomestayNotifications
                                               .Where(d => d.StaffId == userId)
                                               .ToList();

            return View();
        }

        private List<string> SaveImages(IFormFile[] images)
        {
            var imagePaths = new List<string>();
            if (images == null || images.Length == 0) return imagePaths;

            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/homestays");
            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            foreach (var image in images)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var filePath = Path.Combine(directoryPath, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    image.CopyTo(stream);
                }
                imagePaths.Add("/images/homestays/" + fileName);
            }
            return imagePaths;
        }

        private Homestay GetHomestayById(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return _context.Homestays.Include(h => h.HomestayAmenities).FirstOrDefault(h => h.Id == id && h.StaffId == userId);
        }
    }
}