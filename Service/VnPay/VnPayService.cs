using System.Net;
using DoAnCoSo_Nhom2.Libraries;
using DoAnCoSo_Nhom2.Models;

namespace DoAnCoSo_Nhom2.Service.VnPay
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var pay = new VnPayLibrary();
            var urlCallBack = model.ReturnUrl ?? _configuration["Vnpay:ReturnUrl"];
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);


            pay.AddRequestData("vnp_Version", "2.1.0");
            pay.AddRequestData("vnp_Command", "pay");
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            var amountInVnd = (long)(decimal.Parse(model.Amount.ToString()) * 30);
            pay.AddRequestData("vnp_Amount", amountInVnd.ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);
            var orderInfo = WebUtility.UrlEncode($"{model.Name} {model.OrderDescription} {model.Amount}");
            pay.AddRequestData("vnp_OrderInfo", orderInfo);
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_TxnRef", model.OrderId);
            var paymentUrl = pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);
            Console.WriteLine($"Generated Payment URL: {paymentUrl}");
            return paymentUrl;
        }

        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            if (!collections.Any())
            {
                Console.WriteLine("Không nhận được dữ liệu từ VNPay.");
                return null;
            }

            var pay = new VnPayLibrary();
            foreach (var (key, value) in collections)
            {
                if (!string.IsNullOrEmpty(key) && key.StartsWith("vnp_"))
                {
                    pay.AddResponseData(key, value);
                }
            }
            Console.WriteLine($"Response Data from VNPay: {string.Join("&", collections.Select(kv => $"{kv.Key}={kv.Value}"))}");
            var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);

            if (response == null)
            {
                Console.WriteLine("Lỗi khi xử lý phản hồi từ VNPay.");
                return null;
            }
            response.VnPayResponseCode = collections["vnp_ResponseCode"];
            response.OrderDescription = collections["vnp_OrderInfo"];
            response.OrderId = collections["vnp_TxnRef"];
            response.OrderType = collections["vnp_OrderType"];
            response.OrderDescription = System.Web.HttpUtility.UrlDecode(response.OrderDescription);
            response.Success = response.VnPayResponseCode == "00";

            return response;
        }
    }
}
