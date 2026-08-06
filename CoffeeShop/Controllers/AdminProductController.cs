using Microsoft.AspNetCore.Mvc.Rendering;
using CoffeeShop.Data;
using CoffeeShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Controllers
{
    public class AdminProductController : Controller
    {
        private readonly CoffeeDbContext _context;

        public AdminProductController(CoffeeDbContext context)
        {
            _context = context;
        }

        // Danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(x => x.Category)
                .ToListAsync();

            return View(products);
        }
        // Hiển thị form thêm sản phẩm
        public IActionResult Create()
        {
            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }
        // Lưu sản phẩm
        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (product.ImageFile != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);

                string path = Path.Combine(Directory.GetCurrentDirectory(),
                    "wwwroot/uploads",
                    fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(stream);
                }

                product.Image = fileName;
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // Hiển thị form sửa
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            ViewBag.CategoryId = new SelectList(_context.Categories,
                "CategoryId",
                "CategoryName",
                product.CategoryId);

            return View(product);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            var oldProduct = await _context.Products.FindAsync(id);

            if (oldProduct == null)
                return NotFound();

            oldProduct.ProductName = product.ProductName;
            oldProduct.Description = product.Description;
            oldProduct.Price = product.Price;
            oldProduct.Stock = product.Stock;
            oldProduct.CategoryId = product.CategoryId;

            if (product.ImageFile != null)
            {
                string fileName = Guid.NewGuid().ToString() +
                                  Path.GetExtension(product.ImageFile.FileName);

                string path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot/uploads",
                    fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(stream);
                }

                oldProduct.Image = fileName;
            }

            _context.Update(oldProduct);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        // Hiển thị xác nhận xóa
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                // Xóa ảnh khỏi wwwroot/uploads
                if (!string.IsNullOrEmpty(product.Image))
                {
                    var imagePath = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        "wwwroot",
                        "uploads",
                        product.Image);

                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}