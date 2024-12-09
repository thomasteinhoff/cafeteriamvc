using Microsoft.AspNetCore.Mvc.Rendering;
namespace Cafeteria.Models;
public class OrderCreateViewModel
{ 
    public List<Product> Products { get; set; } = new List<Product>(); // Get products from database
    public IEnumerable<SelectListItem>? ProductsSelectList { get; set; } = new List<SelectListItem>(); // Lists selectable items
    public int SelectedProductId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string? Message { get; set; }
}