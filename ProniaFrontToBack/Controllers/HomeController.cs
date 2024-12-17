using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaFrontToBack.DAL;
using ProniaFrontToBack.Models;

namespace ProniaFrontToBack.Controllers
{
    public class HomeController : Controller
    {
        AppDbContext _dbContext;

        public HomeController(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IActionResult Index()
        {

            Response.Cookies.Append("Dost", "Salamlarimi catdiriram Ayxana", new CookieOptions()
            {
                MaxAge = TimeSpan.FromHours(3)
            });


            HttpContext.Session.SetString("ab107", "ey tepegoz");


            List<Product> products = _dbContext.Products.Include(x => x.ProductImages).ToList();
            return View(products);
        }

        public async Task<IActionResult> Detail(int? id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var product = await _dbContext.Products.Include(x => x.Category)
                .Include(x => x.ProductImages)
                .Include(x => x.TagProducts)
                .ThenInclude(tp => tp.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);

            ViewBag.ReProduct = await _dbContext.Products
                .Include(x => x.ProductImages)
                .Where(x => x.CategoryId == product.CategoryId && x.Id != product.Id)
                .ToListAsync();

            return View(product);
        }

    }
}
