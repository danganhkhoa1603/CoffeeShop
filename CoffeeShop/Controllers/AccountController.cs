using CoffeeShop.Data;
using CoffeeShop.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            // Kiểm tra số điện thoại rỗng
            if (string.IsNullOrWhiteSpace(user.Phone))
            {
                ViewBag.Error = "Vui lòng nhập số điện thoại!";
                return View(user);
            }

            // Kiểm tra định dạng số điện thoại Việt Nam
            string phonePattern = @"^(0|\+84)(3|5|7|8|9)[0-9]{8}$";

            if (!System.Text.RegularExpressions.Regex.IsMatch(
                user.Phone.Trim(),
                phonePattern))
            {
                ViewBag.Error = "Số điện thoại không hợp lệ! Ví dụ: 0938282070";
                return View(user);
            }

            // Kiểm tra số điện thoại đã tồn tại
            if (_context.Users.Any(x => x.Phone == user.Phone.Trim()))
            {
                ViewBag.Error = "Số điện thoại đã được đăng ký!";
                return View(user);
            }

            // Chuẩn hóa dữ liệu
            user.Phone = user.Phone.Trim();

            // Mặc định khách hàng
            user.Role = "Customer";
            user.CreatedDate = DateTime.Now;

            _context.Users.Add(user);
            _context.SaveChanges();

            TempData["Success"] = "Đăng ký thành công! Vui lòng đăng nhập.";

            return RedirectToAction("Login");
        }

        // ================= ĐĂNG NHẬP KHÁCH HÀNG =================

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
            HttpContext.Session.SetString("Phone", user.Phone);
            HttpContext.Session.SetString("Address", user.Address ?? "");
            HttpContext.Session.SetString("Role", user.Role);

            if (user.Role == "Admin" || user.Role == "Employee")
            {
                return RedirectToAction("Index", "AdminProduct");
            }

            return RedirectToAction("Index", "Home");
        }

        // ================= ĐĂNG NHẬP NHÂN VIÊN =================

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
            HttpContext.Session.SetString("Phone", employee.Phone);
            HttpContext.Session.SetString("Address", employee.Address);
            HttpContext.Session.SetString("Role", employee.Role);

            return RedirectToAction("Index", "Employee");
        }

        // ================= ĐĂNG XUẤT =================

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        // ================= CHỌN VAI TRÒ =================

        public IActionResult ChooseRole()
        {
            return View();
        }
    }
}