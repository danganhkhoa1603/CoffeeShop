using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Controllers
{
    public class EmployeeLoginController : Controller
    {
        public IActionResult Login()
        {
            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public IActionResult Login(string phone, string password)
        {
            return RedirectToAction("Login", "Account");
        }
    }
}