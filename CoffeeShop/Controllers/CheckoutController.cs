using CoffeeShop.Data;
using CoffeeShop.Extensions;
using CoffeeShop.Models;
using Microsoft.AspNetCore.Mvc;

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

            int userId = HttpContext.Session.GetInt32("UserId").Value;

            var user = _context.Users.FirstOrDefault(x => x.UserId == userId);

            Order order = new Order();

            if (user != null)
            {
                order.CustomerName = user.FullName;
                order.Phone = user.Phone;
                order.Address = user.Address;
            }

            return View(order);
        }

        // Xử lý đặt hàng
        [HttpPost]
        public IActionResult Index(Order order)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart");

            if (cart == null || cart.Count == 0)
            {
                return RedirectToAction("Index", "Cart");
            }

            order.UserId = HttpContext.Session.GetInt32("UserId").Value;
            order.OrderDate = DateTime.Now;
            order.Status = "Chờ xác nhận";
            order.TotalMoney = cart.Sum(x => x.Total);

            _context.Orders.Add(order);
            _context.SaveChanges();

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

            TempData["Success"] = "🎉 Đặt hàng thành công!";

            return RedirectToAction("History", "Order");
        }
    }
}