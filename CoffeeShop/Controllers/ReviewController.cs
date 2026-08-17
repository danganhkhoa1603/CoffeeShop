using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CoffeeShop.Data;
using CoffeeShop.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeShop.Controllers
{
    public class ReviewController : Controller
    {
        private readonly CoffeeDbContext _context;

        public ReviewController(CoffeeDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(int OrderId, int Rating, string Comment)
        {
            var userName = HttpContext.Session.GetString("UserName");
            var userIdStr = HttpContext.Session.GetString("UserId");

            // Lấy thông tin đơn hàng cùng danh sách sản phẩm trong đơn
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == OrderId);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng!";
                return RedirectToAction("History", "Order");
            }

            // Lấy UserId: Ưu tiên lấy từ Đơn hàng, nếu không có mới lấy từ Session
            int userId = order.UserId;
            if (userId <= 0 && !string.IsNullOrEmpty(userIdStr))
            {
                int.TryParse(userIdStr, out userId);
            }

            // Kiểm tra xem đơn hàng này đã từng đánh giá chưa (tránh trùng lặp)
            bool isAlreadyReviewed = await _context.Reviews.AnyAsync(r => r.OrderId == OrderId);
            if (isAlreadyReviewed)
            {
                TempData["Success"] = "Đơn hàng này đã được đánh giá trước đó!";
                return RedirectToAction("History", "Order");
            }

            if (order.OrderDetails != null && order.OrderDetails.Any())
            {
                // Tạo đánh giá cho từng sản phẩm có trong đơn hàng
                foreach (var item in order.OrderDetails)
                {
                    var review = new Review
                    {
                        OrderId = OrderId,
                        ProductId = item.ProductId,
                        UserId = userId,
                        Rating = Rating,
                        Comment = Comment ?? string.Empty,
                        CreatedAt = DateTime.Now
                    };
                    _context.Reviews.Add(review);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Cảm ơn bạn đã gửi đánh giá!";
            }

            return RedirectToAction("History", "Order");
        }
    }
}