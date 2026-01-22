using DoAnCoSo_Nhom2.Models;
using DoAnCoSo_Nhom2.Service.VnPay;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAnCoSo_Nhom2.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;

        public PaymentController(IVnPayService vnPayService)
        {
            _vnPayService = vnPayService;
        }

        [HttpGet]
        [ValidateAntiForgeryToken]
        public IActionResult CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return BadRequest(string.Join("; ", errors));
            }

            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);

            if (string.IsNullOrEmpty(url))
            {
                TempData["PaymentError"] = "Không thể tạo URL thanh toán."; 
                return RedirectToAction("Index", "Home");
            }

            return Redirect(url);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);

            if (response == null)
            {
                TempData["PaymentError"] = "Không nhận được phản hồi từ VNPay."; 
                return RedirectToAction("PaymentResult");
            }

            if (response.Success)
            {
                var orderIdParts = response.OrderId.Split('_');
                if (orderIdParts.Length < 1)
                {
                    TempData["PaymentError"] = "Mã giao dịch không hợp lệ.";
                    return RedirectToAction("PaymentResult");
                }

                var db = HttpContext.RequestServices.GetService(typeof(DoAnCoSo_Nhom2.Data.ApplicationDbContext)) as DoAnCoSo_Nhom2.Data.ApplicationDbContext;
                if (db == null)
                {
                    TempData["PaymentError"] = "Lỗi hệ thống."; 
                    return RedirectToAction("PaymentResult");
                }

                int id;
                if (!int.TryParse(orderIdParts[0], out id))
                {
                    TempData["PaymentError"] = "Mã giao dịch không hợp lệ."; 
                    return RedirectToAction("PaymentResult");
                }

                if (response.OrderType == "booking")
                {
                    var tempBooking = db.TempBookings.FirstOrDefault(t => t.Id == id);
                    if (tempBooking == null)
                    {
                        TempData["PaymentError"] = "Không tìm thấy thông tin đặt phòng tạm.";
                        return RedirectToAction("PaymentResult");
                    }

                    var booking = new Booking
                    {
                        HomestayId = tempBooking.HomestayId,
                        UserId = tempBooking.UserId,
                        CheckInDate = tempBooking.CheckInDate,
                        CheckOutDate = tempBooking.CheckOutDate,
                        NumberOfGuests = tempBooking.NumberOfGuests,
                        TotalPrice = tempBooking.TotalPrice,
                        Status = "Confirmed",
                        UpdatedAt = DateTime.Now
                    };

                    db.Bookings.Add(booking);
                    db.TempBookings.Remove(tempBooking);
                    db.SaveChanges();

                    TempData["PaymentSuccess"] = "Đặt phòng và thanh toán thành công."; 
                }
                else if (response.OrderType == "cart")
                {
                    var cartItem = db.Carts.FirstOrDefault(c => c.Id == id);
                    if (cartItem == null)
                    {
                        TempData["PaymentError"] = "Không tìm thấy mục trong giỏ hàng.";
                        return RedirectToAction("PaymentResult");
                    }

                    var booking = new Booking
                    {
                        HomestayId = cartItem.HomestayId,
                        UserId = cartItem.UserId,
                        CheckInDate = cartItem.CheckInDate,
                        CheckOutDate = cartItem.CheckOutDate,
                        NumberOfGuests = cartItem.NumberOfGuests,
                        TotalPrice = cartItem.TotalPrice,
                        Status = "Confirmed",
                        UpdatedAt = DateTime.Now
                    };

                    db.Bookings.Add(booking);
                    db.Carts.Remove(cartItem);
                    db.SaveChanges();

                    TempData["PaymentSuccess"] = "Thanh toán giỏ hàng thành công.";
                }
                else
                {
                    TempData["PaymentError"] = "Loại đơn hàng không xác định."; 
                }
            }
            else
            {
                TempData["PaymentError"] = $"Thanh toán thất bại. Mã lỗi: {response.VnPayResponseCode}"; 
            }

            return RedirectToAction("PaymentResult");
        }

        [AllowAnonymous]
        public IActionResult PaymentResult()
        {
            ViewBag.Message = TempData["PaymentSuccess"];
            ViewBag.Error = TempData["PaymentError"]; 
            return View();
        }
    }
}