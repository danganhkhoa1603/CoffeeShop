namespace CoffeeShop.ViewModels
{
    public class RevenueReportViewModel
    {
        // Thống kê tổng quan
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int TotalAllOrders { get; set; }
        public int TotalCustomers { get; set; }
        public decimal AverageOrderValue { get; set; }

        // Bộ lọc thời gian (week, month, 6months, year)
        public string Period { get; set; } = "year";
        public string PeriodTitle { get; set; } = "Năm nay (12 tháng)";
        public string PeriodUnitLabel { get; set; } = "Tháng";

        // Bộ lọc năm (khi xem theo năm)
        public int SelectedYear { get; set; }
        public List<int> AvailableYears { get; set; } = new();

        // Dữ liệu Biểu đồ Cột động (Doanh thu & Số lượng đơn hàng)
        public List<string> ChartLabels { get; set; } = new();
        public List<decimal> RevenueData { get; set; } = new();
        public List<int> OrdersData { get; set; } = new();

        // Tương thích ngược
        public List<string> Months => ChartLabels;
        public List<decimal> RevenueByMonth => RevenueData;
        public List<int> OrdersByMonth => OrdersData;

        // Dữ liệu Biểu đồ Tròn 1: Phân bố trạng thái đơn hàng
        public List<string> OrderStatusLabels { get; set; } = new();
        public List<int> OrderStatusCounts { get; set; } = new();
        public List<double> OrderStatusPercentages { get; set; } = new();

        // Dữ liệu Biểu đồ Tròn 2: Cơ cấu doanh thu theo danh mục sản phẩm
        public List<string> CategoryNames { get; set; } = new();
        public List<decimal> CategoryRevenue { get; set; } = new();
        public List<double> CategoryPercentages { get; set; } = new();

        // Bảng chi tiết tổng hợp theo từng mốc thời gian (ngày/tháng)
        public List<ReportDetailItem> ReportDetails { get; set; } = new();
        public List<ReportDetailItem> MonthlyDetails => ReportDetails;
    }

    public class ReportDetailItem
    {
        public int Index { get; set; }
        public string Label { get; set; } = string.Empty;
        public string SubLabel { get; set; } = string.Empty;
        public int OrderCount { get; set; }
        public decimal Revenue { get; set; }
        public decimal AverageRevenuePerOrder { get; set; }
        public double PercentageOfTotal { get; set; }

        // Tương thích thuộc tính cũ
        public int Month => Index;
        public string MonthLabel => Label;
    }
}