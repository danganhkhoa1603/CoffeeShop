using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoffeeShop.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required]
        public string ProductName { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public string? Image { get; set; }

        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public int Stock { get; set; }

        public int CategoryId { get; set; }

        public virtual Category? Category { get; set; }

        // Navigation property liên kết bảng Review
        public virtual ICollection<Review>? Reviews { get; set; }
    }
}