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

        private bool CheckAdminAccess()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin" || role == "Employee";
        }

        // Danh sách sản phẩm
        public async Task<IActionResult> Index(string? search, int? categoryId)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var query = _context.Products.Include(x => x.Category).AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.ProductName.Contains(search) || (p.Description != null && p.Description.Contains(search)));
            }

            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query.OrderByDescending(p => p.ProductId).ToListAsync();

            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentCategory = categoryId;
            ViewBag.TotalProducts = await _context.Products.CountAsync();

            return View(products);
        }

        // Hiển thị form thêm sản phẩm
        public IActionResult Create()
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            ViewBag.CategoryId = new SelectList(_context.Categories, "CategoryId", "CategoryName");
            return View();
        }

        // Lưu sản phẩm
        [HttpPost]
        public async Task<IActionResult> Create(Product product)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            if (product.ImageFile != null)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                string path = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(stream);
                }

                product.Image = fileName;
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã thêm sản phẩm {product.ProductName} thành công!";

            return RedirectToAction(nameof(Index));
        }

        // Hiển thị form sửa
        public async Task<IActionResult> Edit(int id)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

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
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

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
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(product.ImageFile.FileName);
                string path = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await product.ImageFile.CopyToAsync(stream);
                }

                // Xóa ảnh cũ nếu có
                if (!string.IsNullOrEmpty(oldProduct.Image))
                {
                    var oldPath = Path.Combine(uploadsFolder, oldProduct.Image);
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                oldProduct.Image = fileName;
            }

            _context.Update(oldProduct);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã cập nhật sản phẩm {oldProduct.ProductName} thành công!";

            return RedirectToAction(nameof(Index));
        }

        // Hiển thị xác nhận xóa
        public async Task<IActionResult> Delete(int id)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

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
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                if (!string.IsNullOrEmpty(product.Image))
                {
                    var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", product.Image);
                    if (System.IO.File.Exists(imagePath))
                    {
                        System.IO.File.Delete(imagePath);
                    }
                }

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã xóa sản phẩm thành công!";
            }

            return RedirectToAction(nameof(Index));
        }
        // Danh sách đánh giá từ khách hàng
        public async Task<IActionResult> Reviews()
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var reviews = await _context.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }
    }
}