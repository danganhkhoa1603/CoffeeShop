using CoffeeShop.Data;
using CoffeeShop.Models;
using CoffeeShop.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CoffeeShop.Controllers
{
    public class ReportController : Controller
    {
        private readonly CoffeeDbContext _context;

        public ReportController(CoffeeDbContext context)
        {
            _context = context;
        }

        private bool CheckAdminAccess()
        {
            var role = HttpContext.Session.GetString("Role");
            return role == "Admin";
        }

        private static string GetVietnameseDayOfWeek(DayOfWeek dayOfWeek)
        {
            return dayOfWeek switch
            {
                DayOfWeek.Monday => "Thứ 2",
                DayOfWeek.Tuesday => "Thứ 3",
                DayOfWeek.Wednesday => "Thứ 4",
                DayOfWeek.Thursday => "Thứ 5",
                DayOfWeek.Friday => "Thứ 6",
                DayOfWeek.Saturday => "Thứ 7",
                DayOfWeek.Sunday => "Chủ Nhật",
                _ => ""
            };
        }

        public async Task<IActionResult> Index(string? period, int? year)
        {
            if (!CheckAdminAccess())
            {
                return RedirectToAction("Login", "Account");
            }

            var model = new RevenueReportViewModel();
            period = string.IsNullOrWhiteSpace(period) ? "year" : period.ToLower();
            model.Period = period;

            // Danh sách các năm có đơn hàng
            var availableYears = await _context.Orders
                .Select(o => o.OrderDate.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            if (!availableYears.Any())
            {
                availableYears.Add(DateTime.Now.Year);
            }

            int selectedYear = year ?? (availableYears.Contains(DateTime.Now.Year) ? DateTime.Now.Year : availableYears.First());
            model.SelectedYear = selectedYear;
            model.AvailableYears = availableYears;
            model.TotalCustomers = await _context.Users.CountAsync(x => x.Role == "Customer");

            DateTime today = DateTime.Today;
            List<Order> ordersInPeriod;

            if (period == "week")
            {
                model.PeriodTitle = "Báo cáo theo tuần (7 ngày gần nhất)";
                model.PeriodUnitLabel = "Ngày";

                var startDate = today.AddDays(-6);
                var endDate = today.AddDays(1).AddTicks(-1);

                ordersInPeriod = await _context.Orders
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                    .ToListAsync();

                var completedOrders = ordersInPeriod
                    .Where(o => o.Status == "Đã hoàn thành" || o.Status == "Hoàn thành")
                    .ToList();

                model.TotalRevenue = completedOrders.Sum(x => x.TotalMoney);
                model.TotalOrders = completedOrders.Count;
                model.TotalAllOrders = ordersInPeriod.Count;
                model.AverageOrderValue = model.TotalOrders > 0 ? (model.TotalRevenue / model.TotalOrders) : 0;

                for (int i = 6; i >= 0; i--)
                {
                    var date = today.AddDays(-i);
                    var dayOrders = completedOrders.Where(o => o.OrderDate.Date == date.Date).ToList();
                    var dayRevenue = dayOrders.Sum(o => o.TotalMoney);
                    var dayCount = dayOrders.Count;

                    string dow = GetVietnameseDayOfWeek(date.DayOfWeek);
                    string chartLabel = $"{date:dd/MM} ({dow})";

                    model.ChartLabels.Add(chartLabel);
                    model.RevenueData.Add(dayRevenue);
                    model.OrdersData.Add(dayCount);

                    decimal avg = dayCount > 0 ? (dayRevenue / dayCount) : 0;
                    double pct = model.TotalRevenue > 0 ? (double)(dayRevenue / model.TotalRevenue * 100) : 0;

                    model.ReportDetails.Add(new ReportDetailItem
                    {
                        Index = 7 - i,
                        Label = date.ToString("dd/MM/yyyy"),
                        SubLabel = dow,
                        OrderCount = dayCount,
                        Revenue = dayRevenue,
                        AverageRevenuePerOrder = avg,
                        PercentageOfTotal = Math.Round(pct, 1)
                    });
                }
            }
            else if (period == "month")
            {
                model.PeriodTitle = "Báo cáo 1 tháng (30 ngày gần nhất)";
                model.PeriodUnitLabel = "Ngày";

                var startDate = today.AddDays(-29);
                var endDate = today.AddDays(1).AddTicks(-1);

                ordersInPeriod = await _context.Orders
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                    .ToListAsync();

                var completedOrders = ordersInPeriod
                    .Where(o => o.Status == "Đã hoàn thành" || o.Status == "Hoàn thành")
                    .ToList();

                model.TotalRevenue = completedOrders.Sum(x => x.TotalMoney);
                model.TotalOrders = completedOrders.Count;
                model.TotalAllOrders = ordersInPeriod.Count;
                model.AverageOrderValue = model.TotalOrders > 0 ? (model.TotalRevenue / model.TotalOrders) : 0;

                for (int i = 29; i >= 0; i--)
                {
                    var date = today.AddDays(-i);
                    var dayOrders = completedOrders.Where(o => o.OrderDate.Date == date.Date).ToList();
                    var dayRevenue = dayOrders.Sum(o => o.TotalMoney);
                    var dayCount = dayOrders.Count;

                    string chartLabel = date.ToString("dd/MM");

                    model.ChartLabels.Add(chartLabel);
                    model.RevenueData.Add(dayRevenue);
                    model.OrdersData.Add(dayCount);

                    decimal avg = dayCount > 0 ? (dayRevenue / dayCount) : 0;
                    double pct = model.TotalRevenue > 0 ? (double)(dayRevenue / model.TotalRevenue * 100) : 0;

                    model.ReportDetails.Add(new ReportDetailItem
                    {
                        Index = 30 - i,
                        Label = date.ToString("dd/MM/yyyy"),
                        SubLabel = GetVietnameseDayOfWeek(date.DayOfWeek),
                        OrderCount = dayCount,
                        Revenue = dayRevenue,
                        AverageRevenuePerOrder = avg,
                        PercentageOfTotal = Math.Round(pct, 1)
                    });
                }
            }
            else if (period == "6months")
            {
                model.PeriodTitle = "Báo cáo 6 tháng gần nhất";
                model.PeriodUnitLabel = "Tháng";

                var firstMonth = today.AddMonths(-5);
                var startDate = new DateTime(firstMonth.Year, firstMonth.Month, 1);
                var endDate = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month), 23, 59, 59);

                ordersInPeriod = await _context.Orders
                    .Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate)
                    .ToListAsync();

                var completedOrders = ordersInPeriod
                    .Where(o => o.Status == "Đã hoàn thành" || o.Status == "Hoàn thành")
                    .ToList();

                model.TotalRevenue = completedOrders.Sum(x => x.TotalMoney);
                model.TotalOrders = completedOrders.Count;
                model.TotalAllOrders = ordersInPeriod.Count;
                model.AverageOrderValue = model.TotalOrders > 0 ? (model.TotalRevenue / model.TotalOrders) : 0;

                for (int i = 5; i >= 0; i--)
                {
                    var targetDate = today.AddMonths(-i);
                    var mOrders = completedOrders.Where(o => o.OrderDate.Year == targetDate.Year && o.OrderDate.Month == targetDate.Month).ToList();
                    var mRevenue = mOrders.Sum(o => o.TotalMoney);
                    var mCount = mOrders.Count;

                    string chartLabel = $"T{targetDate.Month:00}/{targetDate.Year}";

                    model.ChartLabels.Add(chartLabel);
                    model.RevenueData.Add(mRevenue);
                    model.OrdersData.Add(mCount);

                    decimal avg = mCount > 0 ? (mRevenue / mCount) : 0;
                    double pct = model.TotalRevenue > 0 ? (double)(mRevenue / model.TotalRevenue * 100) : 0;

                    model.ReportDetails.Add(new ReportDetailItem
                    {
                        Index = 6 - i,
                        Label = $"Tháng {targetDate.Month:00}/{targetDate.Year}",
                        SubLabel = $"Năm {targetDate.Year}",
                        OrderCount = mCount,
                        Revenue = mRevenue,
                        AverageRevenuePerOrder = avg,
                        PercentageOfTotal = Math.Round(pct, 1)
                    });
                }
            }
            else // "year" (1 năm)
            {
                model.Period = "year";
                model.PeriodTitle = $"Báo cáo 1 năm (12 tháng - Năm {selectedYear})";
                model.PeriodUnitLabel = "Tháng";

                ordersInPeriod = await _context.Orders
                    .Where(o => o.OrderDate.Year == selectedYear)
                    .ToListAsync();

                var completedOrders = ordersInPeriod
                    .Where(o => o.Status == "Đã hoàn thành" || o.Status == "Hoàn thành")
                    .ToList();

                model.TotalRevenue = completedOrders.Sum(x => x.TotalMoney);
                model.TotalOrders = completedOrders.Count;
                model.TotalAllOrders = ordersInPeriod.Count;
                model.AverageOrderValue = model.TotalOrders > 0 ? (model.TotalRevenue / model.TotalOrders) : 0;

                for (int m = 1; m <= 12; m++)
                {
                    var mOrders = completedOrders.Where(o => o.OrderDate.Month == m).ToList();
                    var mRevenue = mOrders.Sum(o => o.TotalMoney);
                    var mCount = mOrders.Count;

                    string chartLabel = $"Tháng {m}";

                    model.ChartLabels.Add(chartLabel);
                    model.RevenueData.Add(mRevenue);
                    model.OrdersData.Add(mCount);

                    decimal avg = mCount > 0 ? (mRevenue / mCount) : 0;
                    double pct = model.TotalRevenue > 0 ? (double)(mRevenue / model.TotalRevenue * 100) : 0;

                    model.ReportDetails.Add(new ReportDetailItem
                    {
                        Index = m,
                        Label = $"Tháng {m:00}/{selectedYear}",
                        SubLabel = $"Năm {selectedYear}",
                        OrderCount = mCount,
                        Revenue = mRevenue,
                        AverageRevenuePerOrder = avg,
                        PercentageOfTotal = Math.Round(pct, 1)
                    });
                }
            }

            // ==========================================
            // Biểu đồ tròn 1: Phân bố trạng thái đơn hàng
            // ==========================================
            var statusOrderCounts = ordersInPeriod
                .GroupBy(o => string.IsNullOrWhiteSpace(o.Status) ? "Chưa xác định" : o.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            foreach (var item in statusOrderCounts)
            {
                model.OrderStatusLabels.Add(item.Status);
                model.OrderStatusCounts.Add(item.Count);
                double pct = ordersInPeriod.Count > 0 ? ((double)item.Count / ordersInPeriod.Count * 100) : 0;
                model.OrderStatusPercentages.Add(Math.Round(pct, 1));
            }

            // ==========================================
            // Biểu đồ tròn 2: Cơ cấu doanh thu theo danh mục
            // ==========================================
            var periodCompletedOrderIds = ordersInPeriod
                .Where(o => o.Status == "Đã hoàn thành" || o.Status == "Hoàn thành")
                .Select(o => o.OrderId)
                .ToList();

            var categoryStats = await _context.OrderDetails
                .Where(od => periodCompletedOrderIds.Contains(od.OrderId))
                .Include(od => od.Product)
                    .ThenInclude(p => p!.Category)
                .GroupBy(od => od.Product != null && od.Product.Category != null 
                    ? od.Product.Category.CategoryName 
                    : (od.Product != null ? od.Product.ProductName : "Khác"))
                .Select(g => new
                {
                    CategoryName = g.Key,
                    Revenue = g.Sum(od => od.Price * od.Quantity)
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            decimal totalCategoryRevenue = categoryStats.Sum(x => x.Revenue);
            foreach (var cat in categoryStats)
            {
                model.CategoryNames.Add(cat.CategoryName);
                model.CategoryRevenue.Add(cat.Revenue);
                double pct = totalCategoryRevenue > 0 ? (double)(cat.Revenue / totalCategoryRevenue * 100) : 0;
                model.CategoryPercentages.Add(Math.Round(pct, 1));
            }

            return View(model);
        }
    }
}