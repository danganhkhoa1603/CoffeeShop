using CoffeeShop.Data;
using CoffeeShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly CoffeeDbContext _context;

        public ProductController(CoffeeDbContext context)
        {
            _context = context;
        }

        //Danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();

            return View(products);
        }

        //Chi tiết sản phẩm
        public IActionResult Details(int id)
        {
            var product = _context.Products
                                  .FirstOrDefault(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            ViewBag.RelatedProducts = _context.Products
                .Where(p => p.CategoryId == product.CategoryId &&
                            p.ProductId != product.ProductId)
                .Take(4)
                .ToList();

            return View(product);
        }
    }
}