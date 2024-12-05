using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaFrontToBack.DAL;

namespace ProniaFrontToBack.Areas.Manage.Controllers
{
    [Area("Manage")]
    public class ProductController:Controller
    {
        AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(c=>c.Category)
                .Include(c => c.TagProducts)
                .ThenInclude(p=>p.Tag)
                .ToListAsync();
            return View();
        }
    }
}
