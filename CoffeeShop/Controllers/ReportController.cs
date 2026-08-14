using CoffeeShop.Data;
using CoffeeShop.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Controllers
{
    public class ReportController : Controller
    {
        private readonly CoffeeDbContext _context;

        public ReportController(CoffeeDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var model = new RevenueReportViewModel();

            // Tổng doanh thu
            model.TotalRevenue = _context.Orders
                .Where(x => x.Status == "Hoàn thành")
                .Sum(x => (decimal?)x.TotalMoney) ?? 0;

            // Tổng đơn hàng
            model.TotalOrders = _context.Orders
                .Count(x => x.Status == "Hoàn thành");

            // Tổng khách hàng
            model.TotalCustomers = _context.Users
                .Count(x => x.Role == "Customer");

            // ======================
            // Biểu đồ doanh thu theo tháng
            // ======================
            var monthly = _context.Orders
                .Where(x => x.Status == "Hoàn thành")
                .GroupBy(x => x.OrderDate.Month)
                .Select(g => new
                {
                    Month = g.Key,
                    Revenue = g.Sum(x => x.TotalMoney)
                })
                .OrderBy(x => x.Month)
                .ToList();

            model.Months = monthly
                .Select(x => "Tháng " + x.Month)
                .ToList();

            model.RevenueByMonth = monthly
                .Select(x => x.Revenue)
                .ToList();

            return View(model);
        }
    }
}