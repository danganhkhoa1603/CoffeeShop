using Coffee.Models;
using CoffeeShop.Data;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly CoffeeDbContext _context;

        public HomeController(CoffeeDbContext context)
        {
            _context = context;
        }

        // =========================
        // Trang chủ
        // Chỉ hiển thị sản phẩm Category 1 và 2
        // =========================
        public IActionResult Index(string? keyword, int? price)
        {
            var products = _context.Products
                                   .Where(x => x.CategoryId == 1 || x.CategoryId == 2)
                                   .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                products = products.Where(x => x.ProductName.Contains(keyword));
            }

            // Lọc theo giá
            if (price.HasValue)
            {
                switch (price)
                {
                    case 1:
                        products = products.Where(x => x.Price < 50000);
                        break;

                    case 2:
                        products = products.Where(x => x.Price >= 50000 &&
                                                      x.Price <= 100000);
                        break;

                    case 3:
                        products = products.Where(x => x.Price > 100000);
                        break;
                }
            }

            return View(products.ToList());
        }

        // =========================
        // Giới thiệu
        // =========================
        public IActionResult About()
        {
            return View();
        }

        // =========================
        // Cửa hàng (Nguyên liệu)
        // =========================
        public IActionResult Store()
        {
            var products = _context.Products
                                   .Where(x => x.CategoryId == 3)
                                   .ToList();

            return View(products);
        }

        // =========================
        // Liên hệ
        // =========================
        public IActionResult Contact()
        {
            return View(new Contact());
        }
    }
}