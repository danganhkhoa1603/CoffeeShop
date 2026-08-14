using CoffeeShop.Models;

namespace CoffeeShop.ViewModels
{
    public class OrderDetailsViewModel
    {
        public Order Order { get; set; }

        public List<OrderDetail> OrderDetails { get; set; }
    }
}