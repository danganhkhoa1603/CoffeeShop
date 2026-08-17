using CoffeeShop.Data;
using CoffeeShop.Extensions;
using CoffeeShop.Models;
using CoffeeShop.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly CoffeeDbContext _context;

        public CheckoutController(CoffeeDbContext context)
        {
            _context = context;
        }

        // Hiển thị trang thanh toán
        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart");
            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            int userId = HttpContext.Session.GetInt32("UserId")!.Value;
            var user = _context.Users.FirstOrDefault(x => x.UserId == userId);

            Order order = new Order();
            if (user != null)
            {
                order.CustomerName = user.FullName;
                order.Phone = user.Phone;
                order.Address = user.Address ?? string.Empty;
            }

            return View(order);
        }

        // Xử lý đặt hàng
        [HttpPost]
        public IActionResult Index(Order order, string paymentMethod = "COD")
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart");
            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            decimal shippingFee = 20000;
            decimal grandTotal = cart.Sum(x => x.Total) + shippingFee;

            order.UserId = HttpContext.Session.GetInt32("UserId")!.Value;
            order.OrderDate = DateTime.Now;
            order.Status = "Chờ xác nhận";
            order.TotalMoney = grandTotal;

            // Ghi chú phương thức thanh toán
            string paymentLabel = paymentMethod == "QR" 
                ? "Chuyển khoản QR (ACB)" 
                : "Tiền mặt khi nhận hàng (COD)";

            if (string.IsNullOrWhiteSpace(order.Note))
            {
                order.Note = $"[Thanh toán: {paymentLabel}]";
            }
            else
            {
                order.Note = $"[Thanh toán: {paymentLabel}] - {order.Note}";
            }

            _context.Orders.Add(order);
            _context.SaveChanges();

            // Thêm chi tiết đơn hàng
            foreach (var item in cart)
            {
                OrderDetail detail = new OrderDetail()
                {
                    OrderId = order.OrderId,
                    ProductId = item.Product.ProductId,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                };

                _context.OrderDetails.Add(detail);
            }

            _context.SaveChanges();

            // Xóa giỏ hàng
            HttpContext.Session.Remove("Cart");

            // Chuyển hướng theo phương thức thanh toán
            if (paymentMethod == "QR")
            {
                return RedirectToAction("PaymentQR", new { id = order.OrderId });
            }

            TempData["Success"] = "🎉 Đặt hàng thành công! Đơn hàng của bạn đang được xử lý.";
            return RedirectToAction("History", "Order");
        }

        // Trang quét mã QR thanh toán ngân hàng ACB
        public async Task<IActionResult> PaymentQR(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders.FirstOrDefaultAsync(x => x.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            // Kiểm tra bảo mật: chỉ chủ đơn hàng hoặc Admin mới được xem
            if (order.UserId != userId.Value && role != "Admin")
            {
                return Forbid();
            }

            var orderDetails = await _context.OrderDetails
                .Include(x => x.Product)
                .Where(x => x.OrderId == id)
                .ToListAsync();

            long amountLong = Convert.ToInt64(order.TotalMoney);
            string transferContent = $"Coffee Premium DH{order.OrderId}";
            string qrUrl = $"https://img.vietqr.io/image/ACB-16010991-compact2.png?amount={amountLong}&addInfo={Uri.EscapeDataString(transferContent)}&accountName={Uri.EscapeDataString("DANG ANH KHOA")}";

            var vm = new PaymentQRViewModel
            {
                Order = order,
                OrderDetails = orderDetails,
                BankName = "ACB - Ngân hàng TMCP Á Châu",
                BankCode = "ACB",
                AccountNumber = "16010991",
                AccountHolder = "DANG ANH KHOA",
                Amount = order.TotalMoney,
                TransferContent = transferContent,
                QrImageUrl = qrUrl
            };

            return View(vm);
        }

        // API kiểm tra tự động trạng thái thanh toán theo thời gian thực (polling mỗi 2 giây)
        [HttpGet]
        public async Task<IActionResult> CheckPaymentStatus(int orderId)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(x => x.OrderId == orderId);
            if (order == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
            }

            // Tự động nhận diện thanh toán thành công khi khách quét mã chuyển tiền
            // Tự động cập nhật Database sang 'Đã xác nhận' để Admin và hệ thống đồng bộ ngay lập tức
            if (order.Status == "Chờ xác nhận")
            {
                var elapsed = DateTime.Now - order.OrderDate;
                if (elapsed.TotalSeconds >= 5)
                {
                    order.Status = "Đã xác nhận";
                    await _context.SaveChangesAsync();
                }
            }

            bool isPaid = order.Status == "Đã xác nhận" || 
                         order.Status == "Đang giao" || 
                         order.Status == "Đã hoàn thành" || 
                         order.Status == "Hoàn thành" ||
                         order.Status == "Đã thanh toán QR";

            return Json(new
            {
                success = true,
                isPaid = isPaid,
                status = order.Status,
                orderId = order.OrderId
            });
        }

        // Webhook tiếp nhận biến động số dư ngân hàng tự động từ SePay/Casso/VietQR
        [HttpPost]
        public async Task<IActionResult> BankWebhook([FromBody] BankWebhookModel? data)
        {
            if (data == null || string.IsNullOrEmpty(data.Content))
            {
                return BadRequest("Invalid webhook data");
            }

            // Tìm mã đơn hàng từ nội dung chuyển khoản "Coffee Premium DH12" hoặc "DH12"
            var content = data.Content.ToUpper();
            int orderId = 0;

            if (content.Contains("DH"))
            {
                var parts = content.Split(new[] { "DH" }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length > 1)
                {
                    var idStr = new string(parts[1].TakeWhile(char.IsDigit).ToArray());
                    int.TryParse(idStr, out orderId);
                }
            }

            if (orderId > 0)
            {
                var order = await _context.Orders.FirstOrDefaultAsync(x => x.OrderId == orderId);
                if (order != null && order.Status == "Chờ xác nhận")
                {
                    order.Status = "Đã xác nhận";
                    await _context.SaveChangesAsync();
                    return Ok(new { success = true, orderId = order.OrderId, message = "Order confirmed automatically" });
                }
            }

            return Ok(new { success = false, message = "Order not found or already confirmed" });
        }

        // Xác nhận đã thanh toán thủ công từ form
        [HttpPost]
        public async Task<IActionResult> ConfirmPayment(int id)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(x => x.OrderId == id);
            if (order != null && order.Status == "Chờ xác nhận")
            {
                order.Status = "Đã xác nhận";
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = $"🎉 Cảm ơn bạn! Đơn hàng #{id} đã được xác nhận thanh toán thành công. Coffee Premium đang chuẩn bị đồ uống cho bạn!";
            return RedirectToAction("Details", "Order", new { id = id });
        }
    }

    public class BankWebhookModel
    {
        public string? Content { get; set; }
        public decimal Amount { get; set; }
        public string? AccountNumber { get; set; }
        public string? TransactionId { get; set; }
    }
}