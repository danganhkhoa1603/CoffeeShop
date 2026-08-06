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
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();

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
            return View();
        }

        // Trang liên hệ
        public IActionResult Contact()
        {
            return View();
        }
    }
}