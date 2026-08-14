using CoffeeShop.Data;
using CoffeeShop.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Controllers
{
    public class OrderController : Controller
    {
        private readonly CoffeeDbContext _context;

        public OrderController(CoffeeDbContext context)
        {
            _context = context;
        }

        // Lịch sử đơn hàng
        public IActionResult History()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Hiển thị thông báo sau khi đặt hàng
            ViewBag.Success = TempData["Success"];

            var orders = _context.Orders
                .Where(x => x.UserId == userId.Value)
                .OrderByDescending(x => x.OrderDate)
                .ToList();

            return View(orders);
        }
        public IActionResult Details(int id)
        {
            var order = _context.Orders
                .FirstOrDefault(x => x.OrderId == id);

            if (order == null)
                return NotFound();

            var details = _context.OrderDetails
                .Include(x => x.Product)
                .Where(x => x.OrderId == id)
                .ToList();

            var vm = new OrderDetailsViewModel
            {
                Order = order,
                OrderDetails = details
            };

            return View(vm);
        }
        // Hủy đơn hàng
        public IActionResult Cancel(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = _context.Orders.FirstOrDefault(x =>
                x.OrderId == id &&
                x.UserId == userId.Value);

            if (order == null)
            {
                return NotFound();
            }

            if (order.Status == "Chờ xác nhận")
            {
                order.Status = "Đã hủy";
                _context.SaveChanges();
            }

            return RedirectToAction(nameof(History));
        }
    }
}