using System.ComponentModel.DataAnnotations;

namespace BooksShoppingProjectMVC.Models.DTOs
{
    public class CheckoutModel
    {
        [Required]
        [MaxLength(40)]
        public string? Name { get; set; }
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string? Mobile { get; set; }
        [Required]
        [MaxLength(200)]
        public string? Address { get; set; }
        [Required]
        [MaxLength(40)]
        public string? PaymentMethod { get; set; }
    }
}
