using DoAnCoSo_Nhom2.Models;

namespace DoAnCoSo_Nhom2.Service.VnPay
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(PaymentInformationModel model, HttpContext context);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);

    }
}
