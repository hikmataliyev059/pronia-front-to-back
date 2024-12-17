﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ProniaFrontToBack.Models;

namespace ProniaFrontToBack.Services.Managers;

public class CustomSignInManager : SignInManager<AppUser>
{
    public CustomSignInManager(UserManager<AppUser> userManager,
        IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<AppUser> claimsFactory,
        IOptions<IdentityOptions> optionsAccessor,
        ILogger<SignInManager<AppUser>> logger,
        IAuthenticationSchemeProvider schemes,
        IUserConfirmation<AppUser> confirmation)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
    }

    public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent,
        bool lockoutOnFailure)
    {
        var user = await UserManager.FindByNameAsync(userName);
        if (user != null && !user.IsVerified)
        {
            return SignInResult.NotAllowed;
        }

        return await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
    }
}