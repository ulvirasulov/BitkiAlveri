using Microsoft.EntityFrameworkCore;
using ProniaFrontToBack.DAL;

namespace ProniaFrontToBack
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<AppDbContext>(opt =>
            {
                opt.UseSqlServer(builder.Configuration.GetConnectionString("MsSql"));
            });

            var app = builder.Build();

            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=dashboard}/{action=index}/{id?}"
             );

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=home}/{action=index}/{id?}"
             );
            app.UseStaticFiles();
            app.Run();
        }
    }
}
