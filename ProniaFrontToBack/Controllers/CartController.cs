using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProniaFrontToBack.DAL;
using ProniaFrontToBack.ViewModels.Basket;

namespace ProniaFrontToBack.Controllers;

public class CartController : Controller
{
    private readonly AppDbContext _context;
    private const string BasketCookieKey = "basket";

    public CartController(AppDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var json = Request.Cookies[BasketCookieKey];
        List<CookieItemVm>? cookies = new List<CookieItemVm>();
        List<CookieItemVm> deleteItems = new List<CookieItemVm>();
        List<CartVm> cart = new List<CartVm>();

        if (json != null)
        {
            cookies = JsonConvert.DeserializeObject<List<CookieItemVm>>(json);
        }

        if (cookies != null && cookies.Count > 0)
        {
            cookies.ForEach(c =>
            {
                var product = _context.Products
                    .Include(p => p.ProductImages)
                    .FirstOrDefault(x => x.Id == c.Id);

                if (product == null)
                {
                    deleteItems.Add(c);
                }
                else
                {
                    cart.Add(new CartVm()
                    {
                        Id = c.Id,
                        Name = product.Name,
                        Price = product.Price,
                        ImgUrl = product.ProductImages.FirstOrDefault(p => p.Primary)?.ImgUrl,
                        Count = c.Count
                    });
                }
            });

            if (deleteItems.Count > 0)
            {
                deleteItems.ForEach(d => { cookies.Remove(d); });
                Response.Cookies.Append(BasketCookieKey, JsonConvert.SerializeObject(cookies));
            }
        }

        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> AddBasket([FromBody]int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest();
        }

        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
        {
            return NotFound();
        }

        List<CookieItemVm>? cookieList;

        var basket = Request.Cookies[BasketCookieKey];
        if (basket != null)
        {
            cookieList = JsonConvert.DeserializeObject<List<CookieItemVm>>(basket);
            if (cookieList != null)
            {
                var existproduct = cookieList.FirstOrDefault(p => p.Id == product.Id);
                if (existproduct != null)
                {
                    existproduct.Count += 1;
                }
                else
                {
                    cookieList.Add(new CookieItemVm()
                    {
                        Id = id,
                        Count = 1
                    });
                }
            }
        }
        else
        {
            cookieList = new List<CookieItemVm>();
            cookieList.Add(new CookieItemVm()
            {
                Id = id,
                Count = 1
            });
        }

        Response.Cookies.Append(BasketCookieKey, JsonConvert.SerializeObject(cookieList));

        return Ok();
    }

    public IActionResult GetBasket()
    {
        return Content(Request.Cookies[BasketCookieKey] ?? "Basket is empty");
    }

    public IActionResult Refresh()
    {
        return ViewComponent("Basket");
    }
    
}