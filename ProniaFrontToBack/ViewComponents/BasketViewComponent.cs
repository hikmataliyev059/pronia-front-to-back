using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProniaFrontToBack.DAL;
using ProniaFrontToBack.ViewModels.Basket;

namespace ProniaFrontToBack.ViewComponents;

public class BasketViewComponent : ViewComponent
{
    private readonly AppDbContext _context;
    private const string BasketCookieKey = "basket";
    
    public BasketViewComponent(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IViewComponentResult> InvokeAsync()
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
                deleteItems.ForEach(d =>
                {
                    cookies.Remove(d);
                });
                HttpContext.Response.Cookies.Append(BasketCookieKey, JsonConvert.SerializeObject(cookies));
            }
        }

        return View(cart);
    }
}