using CoffeeShop.Data;
using CoffeeShop.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoffeeShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly CoffeeDbContext _context;

        public ProductController(CoffeeDbContext context)
        {
            _context = context;
        }

        // Danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();

            return View(products);
        }

        // Chi tiết sản phẩm
        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            var reviewsList = new List<Review>();

            // Đọc bảng Reviews bằng ADO.NET Connection để ép kiểu UserId an toàn
            using (var command = _context.Database.GetDbConnection().CreateCommand())
            {
                command.CommandText = "SELECT OrderId, ProductId, UserId, Rating, Comment, CreatedAt FROM Reviews WHERE ProductId = @productId";

                var param = command.CreateParameter();
                param.ParameterName = "@productId";
                param.Value = id;
                command.Parameters.Add(param);

                if (command.Connection.State != System.Data.ConnectionState.Open)
                {
                    await command.Connection.OpenAsync();
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var review = new Review();
                        review.OrderId = reader.IsDBNull(0) ? 0 : Convert.ToInt32(reader.GetValue(0));
                        review.ProductId = reader.IsDBNull(1) ? 0 : Convert.ToInt32(reader.GetValue(1));

                        // Parse UserId an toàn dù dữ liệu cũ trong DB là String hay Int
                        object rawUserId = reader.GetValue(2);
                        if (rawUserId != null && int.TryParse(rawUserId.ToString(), out int parsedUserId))
                        {
                            review.UserId = parsedUserId;
                        }

                        review.Rating = reader.IsDBNull(3) ? 0 : Convert.ToInt32(reader.GetValue(3));
                        review.Comment = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                        review.CreatedAt = reader.IsDBNull(5) ? DateTime.Now : Convert.ToDateTime(reader.GetValue(5));

                        reviewsList.Add(review);
                    }
                }
            }

            // Sắp xếp theo ngày tạo mới nhất
            reviewsList = reviewsList.OrderByDescending(r => r.CreatedAt).ToList();

            // Nạp thông tin User tương ứng
            foreach (var review in reviewsList)
            {
                if (review.UserId > 0)
                {
                    review.User = await _context.Users.FindAsync(review.UserId);
                }
            }

            product.Reviews = reviewsList;

            // Sản phẩm liên quan
            ViewBag.RelatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId &&
                            p.ProductId != product.ProductId)
                .Take(4)
                .ToListAsync();

            return View(product);
        }
    }
}