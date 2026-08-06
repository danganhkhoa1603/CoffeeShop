using CoffeeShop.Data;
using CoffeeShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
namespace CoffeeShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly CoffeeDbContext _context;

        public AccountController(CoffeeDbContext context)
        {
            _context = context;
        }

        // ================= ĐĂNG KÝ =================

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User user)
        {
            if (_context.Users.Any(x => x.Phone == user.Phone))
            {
                ViewBag.Error = "Số điện thoại đã tồn tại!";
                return View(user);
            }

            user.Role = "Customer";
            user.CreatedDate = DateTime.Now;

            _context.Users.Add(user);
            _context.SaveChanges();

            return RedirectToAction("Login");
        }

        // ================= ĐĂNG NHẬP =================

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string phone, string password)
        {
            var user = _context.Users.FirstOrDefault(x =>
                x.Phone == phone &&
                x.Password == password);

            if (user == null)
            {
                ViewBag.Error = "Sai số điện thoại hoặc mật khẩu!";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("UserName", user.FullName);
            HttpContext.Session.SetString("Role", user.Role);

            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "AdminProduct");
            }

            if (user.Role == "Employee")
            {
                return RedirectToAction("Index", "Employee");
            }

            return RedirectToAction("Index", "Home");

            return RedirectToAction("Index", "Home");
        }

        // ================= ĐĂNG XUẤT =================
        public IActionResult EmployeeLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult EmployeeLogin(string phone, string password)
        {
            var employee = _context.Users.FirstOrDefault(x =>
                x.Phone == phone &&
                x.Password == password &&
                x.Role == "Employee");

            if (employee == null)
            {
                ViewBag.Error = "Tài khoản nhân viên không đúng!";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", employee.UserId);
            HttpContext.Session.SetString("UserName", employee.FullName);
            HttpContext.Session.SetString("Role", employee.Role);

            return RedirectToAction("Index", "Employee");
        }
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }
        public IActionResult ChooseRole()
        {
            return View();
        }
    }
}