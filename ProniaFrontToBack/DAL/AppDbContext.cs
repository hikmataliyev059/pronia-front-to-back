using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProniaFrontToBack.Models;

namespace ProniaFrontToBack.DAL;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Slider> Sliders { get; set; }

    public DbSet<Category> Categories { get; set; }

    public DbSet<Product> Products { get; set; }

    public DbSet<Tag> Tags { get; set; }

    public DbSet<TagProduct> TagProducts { get; set; }

    public DbSet<ProductImage> ProductImages { get; set; }

    public DbSet<Setting> Settings { get; set; }
}