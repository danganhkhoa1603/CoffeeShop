using CoffeeShop.Models;

namespace CoffeeShop.ViewModels
{
    public class PaymentQRViewModel
    {
        public Order Order { get; set; } = null!;
        public List<OrderDetail> OrderDetails { get; set; } = new();

        // Thông tin tài khoản ngân hàng thụ hưởng
        public string BankName { get; set; } = "ACB (Ngân hàng TMCP Á Châu)";
        public string BankCode { get; set; } = "ACB";
        public string AccountNumber { get; set; } = "16010991";
        public string AccountHolder { get; set; } = "DANG ANH KHOA";

        // Số tiền và nội dung chuyển khoản
        public decimal Amount { get; set; }
        public string TransferContent { get; set; } = string.Empty;

        // URL ảnh mã QR VietQR
        public string QrImageUrl { get; set; } = string.Empty;
    }
}
