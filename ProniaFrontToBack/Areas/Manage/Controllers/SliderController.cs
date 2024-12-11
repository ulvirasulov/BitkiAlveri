using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaFrontToBack.DAL;
using ProniaFrontToBack.Helpers.Extensions;
using ProniaFrontToBack.Models;

namespace ProniaFrontToBack.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Authorize(Roles = "Admin")]
    public class SliderController : Controller
    {
        AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public SliderController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            List<Slider> sliders = await _context.Sliders.ToListAsync();
            return View(sliders);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Slider slider)
        {
            if (!slider.File.ContentType.Contains("image"))
            {
                ModelState.AddModelError("File", "Duzdun sekil formati secin");
                return View();
            }
            if (slider.File.Length > 2097152)
            {
                ModelState.AddModelError("File", "Sekil max 2 Mb ola biler");
                return View();
            }

            slider.ImgUrl = slider.File.Upload(_env.WebRootPath, "Upload/Slider");

            if (!ModelState.IsValid)
            {
                return View();
            }
            _context.Sliders.Add(slider);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        public ActionResult Delete(int? id)
        {
            var slider = _context.Sliders.FirstOrDefault(c => c.Id == id);
            if (slider == null)
            {
                return NotFound();
            }
            FileExtension.DeleteFile(_env.WebRootPath, "Upload/Slider", slider.ImgUrl);
            _context.Sliders.Remove(slider);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }


    }
}
