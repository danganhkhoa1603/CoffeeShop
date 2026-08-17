using CoffeeShop.Data;
using CoffeeShop.Extensions;
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

        // Xem chi tiết đơn hàng
        public IActionResult Details(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = _context.Orders
                .FirstOrDefault(x => x.OrderId == id);

            if (order == null)
            {
                return NotFound();
            }

            // Bảo mật: chỉ chủ đơn hàng hoặc Admin/Employee được xem chi tiết
            if (order.UserId != userId.Value && role != "Admin" && role != "Employee")
            {
                return Forbid();
            }

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

        // In hóa đơn bán hàng cho khách hàng
        public IActionResult PrintInvoice(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = _context.Orders.FirstOrDefault(x => x.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.UserId != userId.Value && role != "Admin" && role != "Employee")
            {
                return Forbid();
            }

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

        // Xuất hóa đơn ra file Excel
        public IActionResult ExportExcel(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var role = HttpContext.Session.GetString("Role");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = _context.Orders.FirstOrDefault(x => x.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.UserId != userId.Value && role != "Admin" && role != "Employee")
            {
                return Forbid();
            }

            var details = _context.OrderDetails
                .Include(x => x.Product)
                .Where(x => x.OrderId == id)
                .ToList();

            var excelBytes = InvoiceExportHelper.GenerateInvoiceExcel(order, details);
            string fileName = $"HoaDon_CoffeeShop_DH{id}_{DateTime.Now:yyyyMMdd_HHmm}.xls";

            return File(excelBytes, "application/vnd.ms-excel", fileName);
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
                TempData["Success"] = $"Đã hủy đơn hàng #{id} thành công!";
            }

            return RedirectToAction(nameof(History));
        }
        // Xóa đơn hàng khỏi lịch sử
        [HttpPost]
        public IActionResult DeleteHistory(int id)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Chỉ cho phép xóa đơn hàng thuộc về chính người dùng đang đăng nhập
            var order = _context.Orders.FirstOrDefault(x =>
                x.OrderId == id &&
                x.UserId == userId.Value);

            if (order == null)
            {
                return NotFound();
            }

            // 1. Xóa các chi tiết đơn hàng trước (OrderDetails)
            var details = _context.OrderDetails.Where(x => x.OrderId == id).ToList();
            if (details.Any())
            {
                _context.OrderDetails.RemoveRange(details);
            }

            // 2. Xóa đơn hàng (Orders)
            _context.Orders.Remove(order);
            _context.SaveChanges();

            TempData["Success"] = $"Đã xóa đơn hàng #{id} khỏi lịch sử thành công!";

            return RedirectToAction(nameof(History));
        }
    }
}