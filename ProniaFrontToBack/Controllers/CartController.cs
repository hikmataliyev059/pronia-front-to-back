using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProniaFrontToBack.DAL;
using ProniaFrontToBack.ViewModels.Basket;

namespace ProniaFrontToBack.Controllers;

public class CartController : Controller
{
    private readonly AppDbContext _context;

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> AddBasket(int itemId)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == itemId);
        if (product == null)
        {
            return NotFound();
        }

        List<CookieItemVm> cookieList;

        var basket = Request.Cookies["basket"];
        if (basket != null)
        {
            cookieList = JsonConvert.DeserializeObject<List<CookieItemVm>>(basket);
            var existproduct = cookieList.FirstOrDefault(p => p.Id == product.Id);
            if (existproduct != null)
            {
                existproduct.Count += 1;
            }
            else
            {
                cookieList.Add(new CookieItemVm()
                {
                    Id = itemId,
                    Count = 1
                });
            }
        }
        else
        {
            cookieList = new List<CookieItemVm>();
            cookieList.Add(new CookieItemVm()
            {
                Id = itemId,
                Count = 1
            });
        }

        Response.Cookies.Append("basket", JsonConvert.SerializeObject(cookieList));

        return RedirectToAction("Index", "Home");
    }

    public IActionResult GetBasket()
    {
        return Content(Request.Cookies["basket"]);
    }
}