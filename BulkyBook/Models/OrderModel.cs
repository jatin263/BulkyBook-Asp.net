using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BulkyBook.Models
{
    public class OrderModel
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("BookModel")]
        public virtual BookModel Book { get; set; }
        [ForeignKey("UserModel")]
        public virtual UserModel User { get; set; }

        [Required]
        public int Quantity { get; set; }
        [Required]
        public int Price { get; set; }
        [Required]
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public int OrderStatus { get; set; }

    }
}
