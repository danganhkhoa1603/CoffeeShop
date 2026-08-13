using CoffeeShop.Data;
using CoffeeShop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Controllers
{
    public class AdminCustomerController : Controller
    {
        private readonly CoffeeDbContext _context;

        public AdminCustomerController(CoffeeDbContext context)
        {
            _context = context;
        }

        private bool CheckAdminAccess()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        // GET: /AdminCustomer/
        public async Task<IActionResult> Index(string? search)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var query = _context.Users.Where(u => u.Role == "Customer");

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.FullName.Contains(search) || u.Phone.Contains(search) || (u.Address != null && u.Address.Contains(search)));
            }

            var customers = await query.OrderByDescending(u => u.CreatedDate).ToListAsync();

            // Calculate order counts per customer
            var customerIds = customers.Select(c => c.UserId).ToList();
            var orderCounts = await _context.Orders
                .Where(o => customerIds.Contains(o.UserId))
                .GroupBy(o => o.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count(), TotalSpent = g.Where(x => x.Status == "Đã hoàn thành").Sum(x => x.TotalMoney) })
                .ToDictionaryAsync(x => x.UserId, x => new { x.Count, x.TotalSpent });

            ViewBag.OrderCounts = orderCounts;
            ViewBag.CurrentSearch = search;
            ViewBag.TotalCustomers = customers.Count;

            return View(customers);
        }

        // POST: /AdminCustomer/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var customer = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.Role == "Customer");
            if (customer != null)
            {
                _context.Users.Remove(customer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa tài khoản khách hàng {customer.FullName}!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
