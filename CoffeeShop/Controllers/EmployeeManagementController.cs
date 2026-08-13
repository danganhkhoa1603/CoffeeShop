using CoffeeShop.Data;
using CoffeeShop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Controllers
{
    public class EmployeeManagementController : Controller
    {
        private readonly CoffeeDbContext _context;

        public EmployeeManagementController(CoffeeDbContext context)
        {
            _context = context;
        }

        private bool CheckAdminAccess()
        {
            return HttpContext.Session.GetString("Role") == "Admin";
        }

        public IActionResult Index()
        {
            if (!CheckAdminAccess())
                return RedirectToAction("Login", "Account");

            var employees = _context.Users
                .Where(x => x.Role == "Employee")
                .OrderByDescending(x => x.UserId)
                .ToList();

            return View(employees);
        }

        //==================THÊM==================

        public IActionResult Create()
        {
            if (!CheckAdminAccess())
                return RedirectToAction("Login", "Account");

            return View();
        }

        [HttpPost]
        public IActionResult Create(User employee)
        {
            if (!CheckAdminAccess())
                return RedirectToAction("Login", "Account");

            if (_context.Users.Any(u => u.Phone == employee.Phone))
            {
                ViewBag.Error = "Số điện thoại này đã được sử dụng!";
                return View(employee);
            }

            employee.Role = "Employee";
            employee.CreatedDate = DateTime.Now;

            _context.Users.Add(employee);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Đã thêm nhân viên {employee.FullName} thành công!";
            return RedirectToAction(nameof(Index));
        }

        //==================SỬA==================

        public IActionResult Edit(int id)
        {
            if (!CheckAdminAccess())
                return RedirectToAction("Login", "Account");

            var employee = _context.Users.Find(id);
            if (employee == null || employee.Role != "Employee")
            {
                return NotFound();
            }

            return View(employee);
        }

        [HttpPost]
        public IActionResult Edit(User employee)
        {
            if (!CheckAdminAccess())
                return RedirectToAction("Login", "Account");

            var existing = _context.Users.FirstOrDefault(u => u.UserId == employee.UserId && u.Role == "Employee");
            if (existing == null)
            {
                return NotFound();
            }

            existing.FullName = employee.FullName;
            existing.Phone = employee.Phone;
            existing.Address = employee.Address;
            if (!string.IsNullOrWhiteSpace(employee.Password))
            {
                existing.Password = employee.Password;
            }

            _context.Users.Update(existing);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Đã cập nhật nhân viên {existing.FullName} thành công!";
            return RedirectToAction(nameof(Index));
        }

        //==================XÓA==================

        public IActionResult Delete(int id)
        {
            if (!CheckAdminAccess())
                return RedirectToAction("Login", "Account");

            var employee = _context.Users.FirstOrDefault(u => u.UserId == id && u.Role == "Employee");
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            if (!CheckAdminAccess())
                return RedirectToAction("Login", "Account");

            var employee = _context.Users.FirstOrDefault(u => u.UserId == id && u.Role == "Employee");
            if (employee != null)
            {
                _context.Users.Remove(employee);
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Đã xóa nhân viên thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}