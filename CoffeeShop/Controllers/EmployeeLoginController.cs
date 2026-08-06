using CoffeeShop.Data;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Controllers
{
    public class EmployeeLoginController : Controller
    {
        private readonly CoffeeDbContext _context;

        public EmployeeLoginController(CoffeeDbContext context)
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
            var employee = _context.Users.FirstOrDefault(x =>
                x.Phone == phone &&
                x.Password == password &&
                x.Role == "Employee");

            if (employee == null)
            {
                ViewBag.Error = "Sai tài khoản hoặc mật khẩu";
                return View();
            }

            HttpContext.Session.SetInt32("UserId", employee.UserId);
            HttpContext.Session.SetString("UserName", employee.FullName);
            HttpContext.Session.SetString("Role", employee.Role);

            return RedirectToAction("Index", "Employee");
        }
    }
}