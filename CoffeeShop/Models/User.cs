using System.ComponentModel.DataAnnotations;

namespace CoffeeShop.Models
{
    public class User
    {
        public int UserId { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string Password { get; set; }

        public string? Address { get; set; }

        public string Role { get; set; } = "Customer";

        public DateTime CreatedDate { get; set; }
    }
}