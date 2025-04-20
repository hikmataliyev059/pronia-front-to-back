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

    public AccountController(UserManager<AppUser> userManager,
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

        var verificationCode = new Random().Next(100000, 999999).ToString();
        user.VerificationCode = verificationCode;
        user.VerificationCodeExpireTime = DateTime.UtcNow.AddMinutes(1);

        await _userManager.UpdateAsync(user);

        var mailRequest = new MailRequest
        {
            ToEmail = user.Email,
            Subject = "Email Verification Code",
            Body = $"Your verification code is: {verificationCode}"
        };
        await _mailService.SendEmailAsync(mailRequest);

        await _userManager.AddToRoleAsync(user, UserRoles.Member.ToString());

        // await _signInManager.SignInAsync(user, true);


        TempData["UserId"] = user.Id;
        return RedirectToAction("VerifyEmail");
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

        var result1 = await _signInManager.PasswordSignInAsync
            (vm.EmailOrUsername, vm.Password, vm.RememberMe, false);
        if (result1.IsNotAllowed)
        {
            ModelState.AddModelError("", "Your email is not verified.");
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


    public IActionResult VerifyEmail()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> VerifyEmail(string verificationCode)
    {
        var userId = TempData["UserId"]?.ToString();
        if (userId == null)
        {
            return RedirectToAction("Register");
        }

        TempData.Keep("UserId");

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            ModelState.AddModelError("", "User not found.");
            return View();
        }

        if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
        {
            var lockoutEnd = user.LockoutEnd?.ToLocalTime();
            ModelState.AddModelError("", $"You are temporarily blocked. Try again at {lockoutEnd}.");
            return View();
        }

        if (user.VerificationCodeExpireTime != null && DateTime.UtcNow < user.VerificationCodeExpireTime)
        {
            var remainingTime = user.VerificationCodeExpireTime.Value - DateTime.UtcNow;
            ViewData["TimeRemaining"] =
                $"You have {remainingTime.Minutes} minute(s) and " +
                $"{remainingTime.Seconds} second(s) left to enter the code.";
        }
        else
        {
            ModelState.AddModelError
                ("", "The verification code has expired. Please request a new code.");
            return View();
        }

        if (user.VerificationCode != verificationCode)
        {
            user.AccessFailedCount++;
            if (user.AccessFailedCount >= 5)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(1);
                user.AccessFailedCount = 0;
                await _userManager.UpdateAsync(user);
                ModelState.AddModelError
                    ("", "Too many failed attempts. You are temporarily blocked for 1 minute.");
                return View();
            }

            await _userManager.UpdateAsync(user);
            ModelState.AddModelError("",
                $"Invalid verification code. You have {5 - user.AccessFailedCount} attempt(s) remaining.");
            return View();
        }

        user.EmailConfirmed = true;
        user.VerificationCode = null;
        user.IsVerified = true;
        user.AccessFailedCount = 0;
        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            ModelState.AddModelError("", "Failed to update user.");
            return View();
        }

        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }


    [HttpPost]
    public async Task<IActionResult> ResendVerificationCode()
    {
        var userId = TempData["UserId"]?.ToString();
        if (userId == null)
        {
            return RedirectToAction("Register");
        }

        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            ModelState.AddModelError("", "User not found.");
            return RedirectToAction("Register");
        }
        
        if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
        {
            ModelState.AddModelError("",
                $"You are temporarily blocked. Try again at {user.LockoutEnd?.ToLocalTime()}.");
            TempData.Keep("UserId");
            return RedirectToAction("VerifyEmail");
        }

        var verificationCodeLimit = 3;
        var verificationTimeLimit = TimeSpan.FromMinutes(5);

        if (user.VerificationCodeGeneratedAt.HasValue &&
            DateTime.UtcNow - user.VerificationCodeGeneratedAt.Value < verificationTimeLimit)
        {
            ModelState.AddModelError
            ("", $"Please wait {verificationTimeLimit.Minutes} " +
                 $"minute(s) before requesting another verification code.");
            TempData.Keep("UserId");
            return RedirectToAction("VerifyEmail");
        }

        var newCode = new Random().Next(100000, 999999).ToString();
        user.VerificationCode = newCode;
        user.VerificationCodeExpireTime =
            DateTime.UtcNow.AddMinutes(1);
        user.VerificationCodeGeneratedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        var mailRequest = new MailRequest
        {
            ToEmail = user.Email,
            Subject = "New Verification Code",
            Body = $"Your new verification code is: {newCode}"
        };
        await _mailService.SendEmailAsync(mailRequest);

        TempData.Keep("UserId");
        TempData["ResendSuccess"] = "A new verification code has been sent to your email.";
        return RedirectToAction("VerifyEmail");
    }
}