using CoffeeShop.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly CoffeeDbContext _context;

        public HomeController(CoffeeDbContext context)
        {
            _context = context;
        }

        // Trang chủ
        public async Task<IActionResult> Index()
        {
            var products = _context.Products
                       .Where(p => p.CategoryId == 1 || p.CategoryId == 2)
                       .ToList();

            return View(products);
        }

        // Trang giới thiệu
        public IActionResult About()
        {
            return View();
        }

        // Trang cửa hàng
        public IActionResult Store()
        {
            var products = _context.Products
                                   .Where(p => p.CategoryId == 3)
                                   .ToList();

            return View(products);
        }

        // Trang liên hệ
        public IActionResult Contact()
        {
            return View();
        }
    }
}