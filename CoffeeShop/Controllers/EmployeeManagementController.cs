using CoffeeShop.Data;
using CoffeeShop.Models;
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

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
                return RedirectToAction("Login", "Account");

            var employees = _context.Users
                .Where(x => x.Role == "Employee")
                .ToList();

            return View(employees);
        }

        //==================THÊM==================

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(User employee)
        {
            employee.Role = "Employee";
            employee.CreatedDate = DateTime.Now;

            _context.Users.Add(employee);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        //==================SỬA==================

        public IActionResult Edit(int id)
        {
            var employee = _context.Users.Find(id);

            return View(employee);
        }

        [HttpPost]
        public IActionResult Edit(User employee)
        {
            employee.Role = "Employee";

            _context.Users.Update(employee);

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        //==================XÓA==================

        public IActionResult Delete(int id)
        {
            var employee = _context.Users.Find(id);

            if (employee != null)
            {
                _context.Users.Remove(employee);

                _context.SaveChanges();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}