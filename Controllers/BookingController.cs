using System.Security.Claims;
using DoAnCoSo_Nhom2.Data;
using DoAnCoSo_Nhom2.Models;
using DoAnCoSo_Nhom2.Service.VnPay;
using DoAnCoSo_Nhom2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAnCoSo_Nhom2.Services.ActivityLog;
using DoAnCoSo_Nhom2.Service.TimeService;

namespace DoAnCoSo_Nhom2.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IVnPayService _vnPayService;
        private readonly IConfiguration _configuration;
        private readonly ActivityLogService _logService;
        private readonly TimeService _timeService;

        public BookingController(
            ApplicationDbContext context,
            IVnPayService vnPayService,
            IConfiguration configuration,
            ActivityLogService logService,
            TimeService timeService
        )
        {
            _context = context;
            _vnPayService = vnPayService;
            _configuration = configuration;
            _logService = logService;
            _timeService = timeService;
        }

        [HttpGet]
        public IActionResult Create(int homeStayId)
        {
            var homestay = _context.Homestays.FirstOrDefault(h => h.Id == homeStayId && h.IsApproved);
            if (homestay == null)
            {
                TempData["BookingError"] = "Homestay không tồn tại hoặc chưa được phê duyệt."; 
                return RedirectToAction("Search", "Homestay");
            }

            ViewBag.HomeStayId = homeStayId;
            return View(new BookingCreateViewModel { HomestayId = homeStayId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(BookingCreateViewModel model)
        {
            var homestay = _context.Homestays.FirstOrDefault(h => h.Id == model.HomestayId && h.IsApproved);
            if (homestay == null)
            {
                ModelState.AddModelError("", "Homestay không tồn tại hoặc chưa được phê duyệt.");
                ViewBag.HomeStayId = model.HomestayId;
                return View(model);
            }

            var nights = (model.CheckOutDate - model.CheckInDate).Days;
            if (nights <= 0)
            {
                ModelState.AddModelError("", "Ngày trả phòng phải sau ngày nhận phòng.");
                ViewBag.HomeStayId = model.HomestayId;
                return View(model);
            }

            var overlappingBooking = _context.Bookings.Any(b =>
                b.HomestayId == model.HomestayId &&
                (b.Status == "Pending" || b.Status == "Confirmed") &&
                model.CheckInDate < b.CheckOutDate &&
                model.CheckOutDate > b.CheckInDate);

            if (overlappingBooking)
            {
                ModelState.AddModelError("", "Ngày bạn chọn đã có người đặt.");
                ViewBag.HomeStayId = model.HomestayId;
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var tempBooking = new TempBooking
            {
                HomestayId = model.HomestayId,
                UserId = userId,
                CheckInDate = model.CheckInDate,
                CheckOutDate = model.CheckOutDate,
                NumberOfGuests = model.NumberOfGuests,
                TotalPrice = homestay.PricePerNight * nights
            };

            _context.TempBookings.Add(tempBooking);
            _context.SaveChanges();

            var txnRef = $"{tempBooking.Id}_{_timeService.Now().Ticks}";

            var paymentInfo = new PaymentInformationModel
            {
                Name = "Thanh toán đặt phòng",
                OrderDescription = $"Đặt phòng Homestay #{tempBooking.HomestayId}",
                Amount = (double)tempBooking.TotalPrice,
                OrderType = "booking",
                OrderId = txnRef,
                ReturnUrl = _configuration["Vnpay:ReturnUrlBooking"]
            };

            var paymentUrl = _vnPayService.CreatePaymentUrl(paymentInfo, HttpContext);
            return Redirect(paymentUrl);
        }

        public IActionResult Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var bookings = _context.Bookings
                .Include(b => b.Homestay)
                .ThenInclude(h => h.Staff)
                .Where(bk => bk.UserId == userId)
                .ToList();
            return View(bookings);
        }

        public async Task<IActionResult> Cancel(int id)
        {
            var booking = _context.Bookings
                .Include(b => b.Homestay)
                .FirstOrDefault(b => b.Id == id);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (booking != null && booking.UserId == userId)
            {
                booking.Status = "Cancelled";
                booking.UpdatedAt = _timeService.Now();
                _context.SaveChanges();

                await _logService.LogAsync(userId, $"Hủy đặt phòng tại Homestay {booking.Homestay?.Name} (Check-in: {booking.CheckInDate:dd/MM/YYYY})");

                TempData["BookingMessage"] = "Đơn đặt phòng đã được hủy.";
            }
            else
            {
                TempData["BookingError"] = "Không thể hủy đơn đặt phòng này."; 
            }

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response?.Success == true)
            {
                var orderIdParts = response.OrderId.Split('_');
                if (orderIdParts.Length > 0 && int.TryParse(orderIdParts[0], out int tempBookingId))
                {
                    var temp = _context.TempBookings.FirstOrDefault(t => t.Id == tempBookingId);
                    if (temp != null)
                    {
                        var booking = new Booking
                        {
                            HomestayId = temp.HomestayId,
                            UserId = temp.UserId,
                            CheckInDate = temp.CheckInDate,
                            CheckOutDate = temp.CheckOutDate,
                            NumberOfGuests = temp.NumberOfGuests,
                            TotalPrice = temp.TotalPrice,
                            Status = "Pending",
                            UpdatedAt = _timeService.Now()
                        };

                        _context.Bookings.Add(booking);
                        _context.TempBookings.Remove(temp);
                        _context.SaveChanges();

                        TempData["BookingMessage"] = "Đặt phòng và thanh toán thành công."; 
                        return RedirectToAction("Index");
                    }
                }

                TempData["BookingError"] = "Không tìm thấy thông tin đặt phòng tạm."; 
            }
            else
            {
                TempData["BookingError"] = "Thanh toán thất bại.";
            }

            return RedirectToAction("Index");
        }

        [AllowAnonymous]
        public IActionResult PaymentResult()
        {
            ViewBag.Message = TempData["BookingMessage"]; 
            ViewBag.Error = TempData["BookingError"]; 
            return View();
        }
    }
}