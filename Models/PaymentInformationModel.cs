namespace DoAnCoSo_Nhom2.Models
{
    public class PaymentInformationModel
    {
        public string OrderType { get; set; }
        public double Amount { get; set; }
        public string OrderDescription { get; set; }
        public string Name { get; set; }
        public string OrderId { get; set; }
        public string ReturnUrl { get; set; }

    }
}
