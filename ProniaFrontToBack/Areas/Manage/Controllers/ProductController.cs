using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaFrontToBack.Areas.Manage.ViewModels.Product;
using ProniaFrontToBack.DAL;
using ProniaFrontToBack.Helpers.Extensions;
using ProniaFrontToBack.Models;
using ProductImageVm = ProniaFrontToBack.Areas.Manage.ViewModels.Product.ProductImageVm;
namespace ProniaFrontToBack.Areas.Manage.Controllers;

[Area("Manage")]
[Authorize(Roles = "Admin")]
public class ProductController : Controller
{
    private readonly AppDbContext _appDbContext;
    private readonly IWebHostEnvironment _env;

    public ProductController(AppDbContext appDbContext, IWebHostEnvironment env)
    {
        _appDbContext = appDbContext;
        _env = env;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _appDbContext.Products
            .Include(c => c.Category)
            .Include(t => t.TagProducts)
            .ThenInclude(pt => pt.Tag)
            .Include(p => p.ProductImages)
            .ToListAsync();

        return View(products);
    }

    public IActionResult Create()
    {
        ViewBag.Categories = _appDbContext.Categories.ToList();
        ViewBag.Tags = _appDbContext.Tags.ToList();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProductVm vm)
    {
        ViewBag.Categories = _appDbContext.Categories.ToList();
        ViewBag.Tags = _appDbContext.Tags.ToList();

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = _appDbContext.Categories.ToList();
            return View();
        }

        if (vm.CategoryId != null)
        {
            if (!await _appDbContext.Categories.AnyAsync(c => c.Id == vm.CategoryId))
            {
                ModelState.AddModelError("CategoryId", $"Category with id {vm.CategoryId} does not exist");
                return View();
            }
        }

        Product product = new Product()
        {
            Name = vm.Name,
            Description = vm.Description,
            Price = vm.Price,
            SKU = vm.SKU,
            CategoryId = vm.CategoryId,
            ProductImages = new List<ProductImage>()
        };

        if (vm.TagIds != null)
        {
            foreach (var tagId in vm.TagIds)
            {
                if (!(await _appDbContext.Tags.AnyAsync(t => t.Id == tagId)))
                {
                    ModelState.AddModelError("TagId", $"Tag with id {tagId} does not exist");
                    return View();
                }

                TagProduct tagProduct = new TagProduct()
                {
                    TagId = tagId,
                    Product = product
                };

                _appDbContext.TagProducts.Add(tagProduct);
            }
        }

        List<string> error = new List<string>();
        if (!vm.MainPhoto.ContentType.Contains("image/"))
        {
            ModelState.AddModelError("MainPhoto", "Enter the correct image format");
            return View();
        }

        if (vm.MainPhoto.Length > 3000000)
        {
            ModelState.AddModelError("MainPhoto", "You can upload max 3mb images");
            return View();
        }

        product.ProductImages.Add(new()
        {
            Primary = true,
            ImgUrl = vm.MainPhoto.Upload(_env.WebRootPath, "Upload/Product")
        });

        foreach (var item in vm.Images)
        {
            if (!item.ContentType.Contains("image/"))
            {
                error.Add($"{item.Name} is not a valid image format!");
                continue;
            }

            if (item.Length > 3000000)
            {
                error.Add($"{item.Name} can upload max 3mb images");
                continue;
            }

            product.ProductImages.Add(new()
            {
                Primary = false,
                ImgUrl = item.Upload(_env.WebRootPath, "Upload/Product")
            });
        }

        TempData["error"] = error;

        await _appDbContext.Products.AddAsync(product);
        await _appDbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
    
    public async Task<IActionResult> Update(int? id)
    {
        if (id == null || !(_appDbContext.Products.Any(x => x.Id == id)))
        {
            return View("Error");
        }

        var product = await _appDbContext.Products
            .Include(c => c.Category)
            .Include(p => p.ProductImages)
            .Include(t => t.TagProducts)
            .Include(t => t.TagProducts)
            .ThenInclude(pt => pt.Tag)
            .FirstOrDefaultAsync(x => x.Id == id);

        ViewBag.Categories = _appDbContext.Categories.ToList();
        ViewBag.Tags = _appDbContext.Tags.ToList();

        UpdateProductVm updateProductVm = new UpdateProductVm()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            CategoryId = product.CategoryId,
            Price = product.Price,
            TagIds = new List<int>(),
            ProductImages = new List<ProductImageVm>()
        };

        foreach (var item in product.TagProducts)
        {
            updateProductVm.TagIds.Add(item.TagId);
        }

        if (product.ProductImages != null)
        {
            foreach (var item in product.ProductImages)
            {
                updateProductVm.ProductImages.Add(new()
                {
                    Primary = item.Primary,
                    ImgUrl = item.ImgUrl,
                });
            }
        }
        
        return View(updateProductVm);
    }

    [HttpPost]
    public async Task<IActionResult> Update(UpdateProductVm vm)
    {
        ViewBag.Categories = _appDbContext.Categories.ToList();
        if (vm.Id == null)
        {
            return View("Error");
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        Product oldProduct = _appDbContext.Products
            .Include(p => p.TagProducts)
            .Include(p => p.ProductImages)
            .FirstOrDefault(x => x.Id == vm.Id);

        if (oldProduct == null)
        {
            return View("Error");
        }

        if (vm.CategoryId != null)
        {
            if (!await _appDbContext.Categories.AnyAsync(c => c.Id == vm.CategoryId))
            {
                ModelState.AddModelError("CategoryId", $"Category with id {vm.CategoryId} does not exist");
                return View();
            }
        }

        _appDbContext.TagProducts.RemoveRange(oldProduct.TagProducts);

        if (vm.TagIds != null)
        {
            foreach (var item in vm.TagIds)
            {
                await _appDbContext.TagProducts.AddAsync(new TagProduct()
                {
                    ProductId = oldProduct.Id,
                    TagId = item
                });
            }
        }

        if (vm.MainPhoto != null)
        {
            if (!vm.MainPhoto.ContentType.Contains("image/"))
            {
                ModelState.AddModelError("MainPhoto", "Enter the correct image format");
                return View(vm);
            }

            if (vm.MainPhoto.Length > 3000000)
            {
                ModelState.AddModelError("MainPhoto", "You can upload max 3mb images");
                return View(vm);
            }

            FileExtension.Delete(_env.WebRootPath, "/Upload/Product/",
                oldProduct.ProductImages.FirstOrDefault(x => x.Primary).ImgUrl);
            _appDbContext.ProductImages.Remove(oldProduct.ProductImages.FirstOrDefault(x => x.Primary));

            oldProduct.ProductImages.Add(new()
            {
                Primary = true,
                ImgUrl = vm.MainPhoto.Upload(_env.WebRootPath, "Upload/Product")
            });
        }

        if (vm.ImagesUrls != null)
        {
            var removeImg = new List<ProductImage>();
            foreach (var item in oldProduct.ProductImages.Where(x => !x.Primary))
            {
                if (vm.ImagesUrls.All(x => x != item.ImgUrl))
                {
                    FileExtension.Delete(_env.WebRootPath, "Upload/Product", item.ImgUrl);
                    _appDbContext.ProductImages.Remove(item);
                }
            }
        }
        else
        {
            foreach (var item in oldProduct.ProductImages.Where(x => !x.Primary))
            {
                FileExtension.Delete(_env.WebRootPath, "/Upload/Product/", item.ImgUrl);
                _appDbContext.ProductImages.Remove(item);
            }
        }

        if (vm.Images != null)
        {
            foreach (var item in vm.Images)
            {
                if (!item.ContentType.Contains("image/"))
                {
                    continue;
                }

                if (item.Length > 3000000)
                {
                    continue;
                }

                oldProduct.ProductImages.Add(new()
                {
                    Primary = false,
                    ImgUrl = item.Upload(_env.WebRootPath, "Upload/Product")
                });
            }
        }

        oldProduct.Name = vm.Name;
        oldProduct.Description = vm.Description;
        oldProduct.Price = vm.Price;
        oldProduct.CategoryId = vm.CategoryId;

        await _appDbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}