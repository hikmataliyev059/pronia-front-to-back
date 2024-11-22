using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaFrontToBack.DAL;
using ProniaFrontToBack.Models;
using ProniaFrontToBack.ViewModels;

namespace ProniaFrontToBack.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext appDbContext)
    {
        _context = appDbContext;
    }

    public IActionResult Index()
    {
        List<Slider> sliders = new List<Slider>();
        Slider slider1 = new Slider()
        {
            Title = "65% OFF",
            SubTitle = "New Plant1",
            Description = "Pronia1, With 100% Natural, Organic & Plant Shop"
        };
        Slider slider2 = new Slider()
        {
            Title = "55% OFF",
            SubTitle = "New Plant2",
            Description = "Pronia2, With 100% Natural, Organic & Plant Shop"
        };
        sliders.Add(slider1);
        sliders.Add(slider2);


        List<Product> products = _context.Products.Include(p => p.ProductImages).ToList();
        HomeVm vm = new HomeVm()
        {
            Sliders = sliders,
            Products = products
        };
        return View(vm);
    }

    public async Task<IActionResult> Detail(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.ProductImages)
            .Include(p => p.TagProducts)
            .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(p => p.Id == id);
        
        return View(product);
    }
}