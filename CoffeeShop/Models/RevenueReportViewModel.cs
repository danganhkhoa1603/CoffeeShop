namespace CoffeeShop.ViewModels
{
    public class RevenueReportViewModel
    {
        public decimal TotalRevenue { get; set; }

        public int TotalOrders { get; set; }

        public int TotalCustomers { get; set; }

        public List<string> Months { get; set; } = new();

        public List<decimal> RevenueByMonth { get; set; } = new();

        public List<string> CategoryNames { get; set; } = new();

        public List<decimal> CategoryRevenue { get; set; } = new();
    }
}