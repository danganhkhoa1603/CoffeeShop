using System.ComponentModel.DataAnnotations;

namespace CoffeeShop.Models
{
    public class Order
    {
        public int OrderId { get; set; }

        // Người đặt hàng
        public int UserId { get; set; }

        [Required]
        public string CustomerName { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Address { get; set; }

        public string? Note { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal TotalMoney { get; set; }

        public string? Status { get; set; }
        public ICollection<OrderDetail>? OrderDetails { get; set; }
    }
}