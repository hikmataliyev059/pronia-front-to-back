using Microsoft.EntityFrameworkCore;
using ProniaFrontToBack.DAL;

namespace ProniaFrontToBack;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllersWithViews();
        builder.Services.AddDbContext<AppDbContext>(opt => 
            opt.UseSqlServer(builder.Configuration.GetConnectionString("DB_URL"))
            );
        
        var app = builder.Build();

        app.MapControllerRoute(
            name:"default",
            pattern:"{controller=Home}/{action=Index}/{id?}"
            );
        
        app.UseStaticFiles();
        
        app.Run();
    }
}