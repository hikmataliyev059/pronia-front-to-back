using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProniaFrontToBack.DAL;
using ProniaFrontToBack.Helpers.Email;
using ProniaFrontToBack.Helpers.Enums;
using ProniaFrontToBack.Models;
using ProniaFrontToBack.ViewModels.Account;
using IMailService = ProniaFrontToBack.Services.Abstractions.IMailService;

namespace ProniaFrontToBack.Controllers;

public class AccountController : Controller

{
    private readonly AppDbContext _appDbContext;
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IMailService _mailService;

    public AccountController(AppDbContext appDbContext, UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager, RoleManager<IdentityRole> roleManager, IMailService mailService)
    {
        _appDbContext = appDbContext;
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _mailService = mailService;
    }

    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        AppUser user = new AppUser();
        user.Email = vm.Email;
        user.Name = vm.Name;
        user.SurName = vm.Surname;
        user.UserName = vm.Username;

        var result = await _userManager.CreateAsync(user, vm.Password);
        if (!result.Succeeded)
        {
            foreach (var item in result.Errors)
            {
                ModelState.AddModelError("", item.Description);
            }

            return View();
        }

        // await _userManager.AddToRoleAsync(user, UserRoles.Admin.ToString());
        // bu ilk defe yarananda yazilir ki, ilk qeydiyyatdan kecen admin olsun qalani member
        // sonra silirik Admin yerine Member yazaciyiq

        await _userManager.AddToRoleAsync(user, UserRoles.Member.ToString());

        // await _signInManager.SignInAsync(user, true);

        return RedirectToAction("Login");
    }

    public async Task<IActionResult> LogOut()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginVm vm, string? returnUrl)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        var user = await _userManager.FindByEmailAsync(vm.EmailOrUsername) ??
                   await _userManager.FindByNameAsync(vm.EmailOrUsername);

        if (user == null)
        {
            ModelState.AddModelError("", "Invalid username or password.");
            return View();
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, vm.Password, true);

        if (result.IsLockedOut)
        {
            ModelState.AddModelError("", "Please try again shortly");
            return View();
        }

        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Invalid username or password.");
            return View();
        }

        await _signInManager.SignInAsync(user, vm.RememberMe);

        if (returnUrl != null)
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> CreateRole()
    {
        foreach (var item in Enum.GetValues(typeof(UserRoles)))
        {
            await _roleManager.CreateAsync(new IdentityRole(item.ToString()));
        }

        return RedirectToAction("Index", "Home");
    }

    public IActionResult ForgetPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ForgetPassword(ForgetPasswordVm vm)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        var appUser = await _userManager.FindByEmailAsync(vm.Email);
        if (appUser == null)
        {
            return NotFound();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(appUser);
        // object userStat = new
        // {
        //     userId = appUser.Id,
        //     token = token,
        // };    bunu mlm yazib uzun yoldu 155 ci setr ucun yazmisdi amma men yazdigim daha optimaldi

        var link = Url.Action("ResetPassword", "Account",
            new { userId = appUser.Id, Token = token }, protocol: HttpContext.Request.Scheme);

        var placeholders = new Dictionary<string, string>
        {
            { "UserName", appUser.UserName ?? vm.Email.Split('@')[0] },
            { "ResetLink", link }
        };
        var templatePath = Path.Combine(Directory.GetCurrentDirectory(),
            "wwwroot", "assets", "html", "ResetPasswordTemplate.html");
        var emailBody = EmailTemplateHelper.GetTemplate(templatePath, placeholders);

        MailRequest mailRequest = new MailRequest()
        {
            ToEmail = vm.Email,
            Subject = "Reset Your Password - Pronia Support",
            Body = emailBody
        };
        // normalda Body hissesine a href yazib sade gondermek olardi ozum template falan verdim code uzandi 163-170

        await _mailService.SendEmailAsync(mailRequest);

        return Ok("Reset password email sent.");
    }

    public IActionResult ResetPassword(string userId, string token)
    {
        if (userId == null)
        {
            return BadRequest();
        }

        ResetPasswordVm resetPasswordVm = new ResetPasswordVm()
        {
            token = token,
            userId = userId
        };
        return View(resetPasswordVm);
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordVm vm)
    {
        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var user = await _userManager.FindByIdAsync(vm.userId);
        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.ResetPasswordAsync(user, vm.token, vm.NewPassword);
        if (!result.Succeeded)
        {
            foreach (var item in result.Errors)
            {
                ModelState.AddModelError("", item.Description);
            }

            return View(vm);
        }

        return RedirectToAction("Login");
    }

    public async Task<IActionResult> ConfirmEmail(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return NotFound("User Not Found");
        }

        user.EmailConfirmed = true;
        await _appDbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Login));
    }
}