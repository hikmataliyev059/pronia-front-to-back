using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaFrontToBack.Areas.Manage.ViewModels.Product;
using ProniaFrontToBack.DAL;
using ProniaFrontToBack.Models;

namespace ProniaFrontToBack.Areas.Manage.Controllers;

[Area("Manage")]
public class ProductController : Controller
{
    private readonly AppDbContext _appDbContext;

    public ProductController(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<IActionResult> Index()
    {
        var products = await _appDbContext.Products
            .Include(c => c.Category)
            .Include(t => t.TagProducts)
            .ThenInclude(pt => pt.Tag)
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
            CategoryId = vm.CategoryId
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
            .Include(t => t.TagProducts)
            .ThenInclude(pt => pt.Tag).FirstOrDefaultAsync(x => x.Id == id);
        ViewBag.Categories = _appDbContext.Categories.ToList();

        UpdateProductVm updateProductVm = new UpdateProductVm()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            CategoryId = product.CategoryId,
            Price = product.Price
        };
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

        Product oldProduct = _appDbContext.Products.FirstOrDefault(x => x.Id == vm.Id);
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

        oldProduct.Name = vm.Name;
        oldProduct.Description = vm.Description;
        oldProduct.Price = vm.Price;
        oldProduct.CategoryId = vm.CategoryId;
        await _appDbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}