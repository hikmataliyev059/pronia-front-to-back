using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ProniaFrontToBack.DAL;
using ProniaFrontToBack.Helpers.Email;
using ProniaFrontToBack.Models;
using IMailService = ProniaFrontToBack.Services.Abstractions.IMailService;
using MailService = ProniaFrontToBack.Services.Impl.MailService;

namespace ProniaFrontToBack;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllersWithViews();

        builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
        builder.Services.AddTransient<IMailService, MailService>();

        builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
        {
            opt.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789._";
            opt.User.RequireUniqueEmail = true;
            // opt.SignIn.RequireConfirmedEmail = true;
            opt.Password.RequiredLength = 8;
            opt.Lockout.AllowedForNewUsers = true;
            opt.Lockout.MaxFailedAccessAttempts = 3;
            opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(2);
        }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

        builder.Services.AddDbContext<AppDbContext>(opt =>
            opt.UseSqlServer(builder.Configuration.GetConnectionString("DB_URL"))
        );

        var app = builder.Build();

        //deyesen bunlari yazmasanda arxada goturur bunu 34-35 deyirem.
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"
        );

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}"
        );

        app.UseStaticFiles();

        app.Run();
    }
}