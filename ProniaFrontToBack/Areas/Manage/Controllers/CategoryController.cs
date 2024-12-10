using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaFrontToBack.DAL;
using ProniaFrontToBack.Models;

namespace ProniaFrontToBack.Areas.Manage.Controllers;

[Area("Manage")]
[Authorize(Roles = "Admin")]
public class CategoryController : Controller
{
    private readonly AppDbContext _appDbContext;

    public CategoryController(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _appDbContext.Categories
            .Include(p => p.Products).ToListAsync();
        return View(categories);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Category category)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        _appDbContext.Categories.Add(category);
        _appDbContext.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = _appDbContext.Categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
        {
            return NotFound();
        }

        _appDbContext.Categories.Remove(category);
        _appDbContext.SaveChanges();
        return RedirectToAction("Index");
    }

    public IActionResult Update(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category = _appDbContext.Categories.FirstOrDefault(c => c.Id == id);
        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    [HttpPost]
    public IActionResult Update(Category newCategory)
    {
        if (!ModelState.IsValid)
        {
            return View(newCategory);
        } 

        var oldCategory = _appDbContext.Categories.FirstOrDefault(c => c.Id == newCategory.Id);
        if (oldCategory == null)
        {
            return NotFound();
        }

        oldCategory.Name = newCategory.Name;
        _appDbContext.SaveChanges();

        return RedirectToAction("Index");
    }
}