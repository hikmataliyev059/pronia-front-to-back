using System.ComponentModel.DataAnnotations;
using ProniaFrontToBack.Models.Base;

namespace ProniaFrontToBack.Models;

public class Category : BaseEntity
{
    [Required, StringLength(10, ErrorMessage = "You can enter a maximum of 10 characters"),
     MinLength(3, ErrorMessage = "The name must be at least 3 characters long")]
    public string Name { get; set; }

    public List<Product>? Products { get; set; }
}