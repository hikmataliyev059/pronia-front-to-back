using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaFrontToBack.DAL;
using ProniaFrontToBack.Models;
using ProniaFrontToBack.ViewModels.Shop;

namespace ProniaFrontToBack.Controllers;

public class ShopController : Controller
{
    private readonly AppDbContext _context;

    public ShopController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, int? order)
    {
        IQueryable<Product> query = _context.Products
            .Include(p => p.ProductImages)
            .AsQueryable();

        switch (order)
        {
            case 1:
                query = query.OrderBy(p => p.Name);
                break;
            case 2:
                query = query.OrderBy(p => p.Price);
                break;
            case 3:
                query = query.OrderByDescending(p => p.Id);
                break;
        }

        if (!String.IsNullOrEmpty(search))
        {
            query = query.Where(p => p.Name.ToLower().Contains(search.ToLower()));
        }

        ShopVm shopVm = new ShopVm()
        {
            Categories = await _context.Categories.Include(c => c.Products).ToListAsync(),
            Products = await query.ToListAsync(),
        };
        return View(shopVm);
    }
}