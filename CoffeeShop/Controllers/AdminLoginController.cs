using CoffeeShop.Data;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Controllers
{
    public class AdminLoginController : Controller
    {
        private readonly CoffeeDbContext _context;

        public AdminLoginController(CoffeeDbContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(string phone, string password)
        {
            var admin = _context.Users.FirstOrDefault(x =>
                x.Phone == phone &&
                x.Password == password &&
                x.Role == "Admin");

            if (admin == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", admin.UserId);
            HttpContext.Session.SetString("UserName", admin.FullName);
            HttpContext.Session.SetString("Role", admin.Role);

            return RedirectToAction("Index", "AdminProduct");
        }
    }
}