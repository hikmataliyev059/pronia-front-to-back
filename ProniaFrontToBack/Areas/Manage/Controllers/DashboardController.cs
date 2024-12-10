﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace ProniaFrontToBack.Areas.Manage.Controllers;

[Area("Manage")]
[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}