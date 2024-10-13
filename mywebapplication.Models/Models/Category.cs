using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace mywebapplication.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [DisplayName("Category Name")]
        [MaxLength(50, ErrorMessage = "Name must be less than 50")]
        public string Name { get; set; }
        [DisplayName("Category Order")]
        [Range(1,100,ErrorMessage = "Display order must be between 1-100")]
        public int CategoryOrder { get; set; }
    }
}
