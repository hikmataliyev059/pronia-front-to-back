using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaFrontToBack.DAL;
using ProniaFrontToBack.Helpers.Extensions;
using ProniaFrontToBack.Models;

namespace ProniaFrontToBack.Areas.Manage.Controllers;

[Area("Manage")]
[Authorize(Roles = "Admin")]
public class SliderController : Controller
{
    private readonly AppDbContext _appDbContext;
    private readonly IWebHostEnvironment _env;

    public SliderController(AppDbContext appDbContext, IWebHostEnvironment env)
    {
        _appDbContext = appDbContext;
        _env = env;
    }   

    public async Task<IActionResult> Index()
    {
        List<Slider> sliders = await _appDbContext.Sliders.ToListAsync();
        return View(sliders);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public IActionResult Create(Slider slider)
    {
        if (!slider.File.ContentType.Contains("image"))
        {
            ModelState.AddModelError("File", "Enter the correct file type");
            return View();
        }

        if (slider.File.Length > 2097152)
        {
            ModelState.AddModelError("File", "File is too long");
        }


        slider.ImgUrl = slider.File.Upload(_env.WebRootPath, "Upload/Slider/");


        if (!ModelState.IsValid)
        {
            return View();
        }

        _appDbContext.Sliders.Add(slider);
        _appDbContext.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int? id)
    {
        var slider = _appDbContext.Sliders.FirstOrDefault(s => s.Id == id);
        if (slider == null)
        {
            return NotFound();
        }

        FileExtension.Delete(_env.WebRootPath, "Upload/Slider/", slider.ImgUrl);

        _appDbContext.Sliders.Remove(slider);
        _appDbContext.SaveChanges();
        return RedirectToAction(nameof(Index));
    }
}