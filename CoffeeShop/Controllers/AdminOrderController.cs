using CoffeeShop.Data;
using CoffeeShop.Extensions;
using CoffeeShop.Models;
using CoffeeShop.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Controllers
{
    public class AdminOrderController : Controller
    {
        private readonly CoffeeDbContext _context;

        public AdminOrderController(CoffeeDbContext context)
        {
            _context = context;
        }

        private bool CheckAdminAccess()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin" || role == "Employee";
        }

        // GET: /AdminOrder/
        public async Task<IActionResult> Index(string? status, string? search)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                query = query.Where(o => o.Status == status);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o => o.CustomerName.Contains(search) || o.Phone.Contains(search) || o.OrderId.ToString().Contains(search));
            }

            // 1. Lấy danh sách đơn hàng
            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

            // 2. Lấy danh sách OrderId từ các đơn hàng hiện tại
            var orderIds = orders.Select(o => o.OrderId).ToList();

            // 3. Query riêng bảng Reviews an toàn
            if (orderIds.Any())
            {
                try
                {
                    var allReviews = await _context.Reviews.AsNoTracking().ToListAsync();
                    var matchingReviews = allReviews.Where(r => orderIds.Contains(r.OrderId)).ToList();

                    // 4. Gán danh sách Review tương ứng vào từng Order
                    foreach (var order in orders)
                    {
                        order.Reviews = matchingReviews.Where(r => r.OrderId == order.OrderId).ToList();
                    }
                }
                catch
                {
                    // Nếu query Reviews gặp lỗi dữ liệu, khởi tạo danh sách rỗng để không bị crash trang
                    foreach (var order in orders)
                    {
                        order.Reviews = new List<Review>();
                    }
                }
            }

            // Analytics / Metrics
            ViewBag.TotalOrders = await _context.Orders.CountAsync();
            ViewBag.PendingOrders = await _context.Orders.CountAsync(o => o.Status == "Chờ xác nhận");
            ViewBag.CompletedOrders = await _context.Orders.CountAsync(o => o.Status == "Đã hoàn thành");
            ViewBag.TotalRevenue = await _context.Orders.Where(o => o.Status == "Đã hoàn thành").SumAsync(o => (decimal?)o.TotalMoney) ?? 0;
            ViewBag.CurrentStatus = status ?? "Tất cả";
            ViewBag.CurrentSearch = search;

            return View(orders);
        }

        // GET: /AdminOrder/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders.FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            var orderDetails = await _context.OrderDetails
                .Include(d => d.Product)
                .Where(d => d.OrderId == id)
                .ToListAsync();

            ViewBag.OrderDetails = orderDetails;

            return View(order);
        }

        // In hóa đơn từ trang Admin
        public async Task<IActionResult> PrintInvoice(int id)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders.FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            var orderDetails = await _context.OrderDetails
                .Include(d => d.Product)
                .Where(d => d.OrderId == id)
                .ToListAsync();

            var vm = new OrderDetailsViewModel
            {
                Order = order,
                OrderDetails = orderDetails
            };

            return View("~/Views/Order/PrintInvoice.cshtml", vm);
        }

        // Xuất 1 hóa đơn chi tiết ra file Excel từ Admin
        public async Task<IActionResult> ExportExcel(int id)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders.FirstOrDefaultAsync(m => m.OrderId == id);
            if (order == null)
            {
                return NotFound();
            }

            var orderDetails = await _context.OrderDetails
                .Include(d => d.Product)
                .Where(d => d.OrderId == id)
                .ToListAsync();

            var excelBytes = InvoiceExportHelper.GenerateInvoiceExcel(order, orderDetails);
            string fileName = $"HoaDon_Admin_DH{id}_{DateTime.Now:yyyyMMdd_HHmm}.xls";

            return File(excelBytes, "application/vnd.ms-excel", fileName);
        }

        // Xuất toàn bộ danh sách đơn hàng đã lọc ra file Excel
        public async Task<IActionResult> ExportAllExcel(string? status, string? search)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(status) && status != "Tất cả")
            {
                query = query.Where(o => o.Status == status);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(o => o.CustomerName.Contains(search) || o.Phone.Contains(search) || o.OrderId.ToString().Contains(search));
            }

            var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();
            string filterTitle = $"Trạng thái: {(string.IsNullOrEmpty(status) ? "Tất cả" : status)}";
            if (!string.IsNullOrEmpty(search))
            {
                filterTitle += $" | Từ khóa: {search}";
            }

            var excelBytes = InvoiceExportHelper.GenerateOrdersListExcel(orders, filterTitle);
            string fileName = $"DanhSach_DonHang_CoffeeShop_{DateTime.Now:yyyyMMdd_HHmm}.xls";

            return File(excelBytes, "application/vnd.ms-excel", fileName);
        }

        // POST: /AdminOrder/UpdateStatus
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int orderId, string status)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng #{orderId} thành '{status}'!";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /AdminOrder/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                var details = _context.OrderDetails.Where(d => d.OrderId == id);
                _context.OrderDetails.RemoveRange(details);
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa đơn hàng #{id} thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}