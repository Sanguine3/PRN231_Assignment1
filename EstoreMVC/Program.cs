using EstoreMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace EstoreMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add configuration settings
            builder.Configuration.AddJsonFile("appsettings.json");

            var connectionString = builder.Configuration.GetConnectionString("DbConnection");

            // Add services
            builder.Services.AddDbContext<EStoreContext>(options =>
                options.UseSqlServer(connectionString));

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<EStoreContext>();

            builder.Services.AddSession();
            builder.Services.AddHttpContextAccessor();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}