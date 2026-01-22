using System.Security.Claims;
using DoAnCoSo_Nhom2.Data;
using DoAnCoSo_Nhom2.Models;
using DoAnCoSo_Nhom2.Services.ActivityLog;
using DoAnCoSo_Nhom2.Service.TimeService;
using DoAnCoSo_Nhom2.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DoAnCoSo_Nhom2.Service.VnPay;

namespace DoAnCoSo_Nhom2.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ActivityLogService _logService;
        private readonly TimeService _timeService;
        private readonly IVnPayService _vnPayService;
        private readonly IConfiguration _configuration;

        public CartController(ApplicationDbContext context, ActivityLogService logService, TimeService timeService, IVnPayService vnPayService, IConfiguration configuration)
        {
            _context = context;
            _logService = logService;
            _timeService = timeService;
            _vnPayService = vnPayService;
            _configuration = configuration;
        }

        public IActionResult AddToCart(int homeStayId)
        {
            ViewBag.HomeStayId = homeStayId;
            return View(new BookingCreateViewModel { HomestayId = homeStayId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(BookingCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.HomeStayId = model.HomestayId;
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var homestay = await _context.Homestays.FindAsync(model.HomestayId);
            if (homestay == null)
            {
                ModelState.AddModelError("", "Homestay không tồn tại.");
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

            var overlappingBooking = await _context.Bookings.FirstOrDefaultAsync(b =>
                b.HomestayId == model.HomestayId &&
                (b.Status == "Pending" || b.Status == "Confirmed") &&
                model.CheckInDate < b.CheckOutDate &&
                model.CheckOutDate > b.CheckInDate);

            if (overlappingBooking != null)
            {
                ModelState.AddModelError("", "Ngày bạn chọn đã có người đặt. Vui lòng chọn khoảng thời gian khác.");
                ViewBag.HomeStayId = model.HomestayId;
                return View(model);
            }

            var cart = new Cart
            {
                HomestayId = model.HomestayId,
                CheckInDate = model.CheckInDate,
                CheckOutDate = model.CheckOutDate,
                NumberOfGuests = model.NumberOfGuests,
                TotalPrice = homestay.PricePerNight * nights,
                UserId = userId,
                CreatedAt = _timeService.Now(),
            };

            try
            {
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();

                await _logService.LogAsync(userId, $"Đã thêm Homestay {homestay.Name} vào giỏ hàng");
                TempData["CartMessage"] = "Đã thêm Homestay vào giỏ hàng thành công!"; 
            }
            catch (System.Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi khi thêm vào giỏ hàng: {ex.Message}");
                ViewBag.HomeStayId = model.HomestayId;
                return View(model);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Remove(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var cartItem = await _context.Carts
                .Include(c => c.Homestay)
                .FirstOrDefaultAsync(c => c.Id == id && c.UserId == userId);

            if (cartItem != null)
            {
                _context.Carts.Remove(cartItem);
                await _context.SaveChangesAsync();

                await _logService.LogAsync(userId,
                    $"Đã xóa Homestay {cartItem.Homestay?.Name} khỏi giỏ hàng (ID: {cartItem.HomestayId}), " +
                    $"CheckIn: {cartItem.CheckInDate:dd/MM/yyyy}, CheckOut: {cartItem.CheckOutDate:dd/MM/yyyy}, " +
                    $"Số khách: {cartItem.NumberOfGuests}, Tổng tiền: {cartItem.TotalPrice:N0} VND");
                TempData["CartMessage"] = "Đã xóa mục khỏi giỏ hàng thành công!"; 
            }
            else
            {
                TempData["CartError"] = "Không tìm thấy mục trong giỏ hàng."; 
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var carts = await _context.Carts
                .Include(c => c.Homestay)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (!carts.Any())
            {
                TempData["CartMessage"] = "Giỏ hàng của bạn đang trống.";
            }

            return View(carts);
        }

        [HttpPost]
        public IActionResult Pay(int id)
        {
            var cartItem = _context.Carts.FirstOrDefault(c => c.Id == id && c.UserId == User.FindFirst(ClaimTypes.NameIdentifier).Value);
            if (cartItem == null)
            {
                TempData["CartError"] = "Không tìm thấy mục trong giỏ hàng.";
                return RedirectToAction("Index");
            }

            var txnRef = $"{cartItem.Id}_{DateTime.Now.Ticks}";
            string returnUrl = _configuration["Vnpay:ReturnUrlCart"];

            var paymentInfo = new PaymentInformationModel
            {
                Name = "Thanh toán đặt phòng Homestay",
                OrderDescription = $"Đặt phòng Homestay #{cartItem.HomestayId}",
                Amount = (double)cartItem.TotalPrice,
                OrderType = "cart",
                OrderId = txnRef,
                ReturnUrl = returnUrl
            };

            Console.WriteLine($"Creating payment URL with OrderId: {txnRef}");

            var paymentUrl = _vnPayService.CreatePaymentUrl(paymentInfo, HttpContext);

            if (string.IsNullOrEmpty(paymentUrl))
            {
                TempData["CartError"] = "Không thể tạo URL thanh toán."; 
                return RedirectToAction("Index");
            }

            Console.WriteLine($"Redirecting to payment URL: {paymentUrl}");
            return Redirect(paymentUrl);
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult PaymentCallbackVnpay()
        {
            Console.WriteLine("==== VNPay Callback Received ====");

            foreach (var item in Request.Query)
            {
                Console.WriteLine($"{item.Key} = {item.Value}");
            }

            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response == null)
            {
                TempData["PaymentError"] = "Không nhận được phản hồi từ VNPay."; 
                return RedirectToAction("PaymentResult");
            }

            Console.WriteLine($"Response: Success={response.Success}, VnPayResponseCode={response.VnPayResponseCode}, OrderId={response.OrderId}");

            if (response.Success)
            {
                var orderIdParts = response.OrderId.Split('_');
                if (orderIdParts.Length > 0 && int.TryParse(orderIdParts[0], out int cartItemId))
                {
                    var cartItem = _context.Carts.FirstOrDefault(c => c.Id == cartItemId);
                    if (cartItem != null)
                    {
                        var booking = new Booking
                        {
                            HomestayId = cartItem.HomestayId,
                            UserId = cartItem.UserId,
                            CheckInDate = cartItem.CheckInDate,
                            CheckOutDate = cartItem.CheckOutDate,
                            NumberOfGuests = cartItem.NumberOfGuests,
                            TotalPrice = cartItem.TotalPrice,
                            Status = "Pending",
                            UpdatedAt = _timeService.Now(),
                        };

                        _context.Bookings.Add(booking);
                        _context.Carts.Remove(cartItem);
                        _context.SaveChanges();

                        TempData["PaymentMessage"] = "Thanh toán thành công!";
                    }
                    else
                    {
                        TempData["PaymentError"] = "Không tìm thấy mục trong giỏ hàng."; 
                    }
                }
                else
                {
                    TempData["PaymentError"] = "Mã giao dịch không hợp lệ."; 
                }
            }
            else
            {
                string error = "Thanh toán thất bại.";
                if (!string.IsNullOrEmpty(response.VnPayResponseCode))
                {
                    error += $" Mã lỗi: {response.VnPayResponseCode}.";
                }
                if (!string.IsNullOrEmpty(response.OrderDescription))
                {
                    error += $" Thông tin: {response.OrderDescription}";
                }
                TempData["PaymentError"] = error; 
            }

            return RedirectToAction("PaymentResult");
        }

        [AllowAnonymous]
        public IActionResult PaymentResult()
        {
            ViewBag.Message = TempData["PaymentMessage"]; 
            ViewBag.Error = TempData["PaymentError"]; 

            return View();
        }
    }
}