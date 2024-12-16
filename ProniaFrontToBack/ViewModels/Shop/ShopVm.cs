using ProniaFrontToBack.Models;

namespace ProniaFrontToBack.ViewModels.Shop;

public class ShopVm
{
    public int? Order { get; set; }
    public int? CategoryId { get; set; }
    public string? Search { get; set; }
    public List<Product> Products { get; set; }
    public List<Category> Categories { get; set; }
}