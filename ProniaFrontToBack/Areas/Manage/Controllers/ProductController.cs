using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProniaFrontToBack.Areas.Manage.ViewModels.Product;
using ProniaFrontToBack.DAL;
using ProniaFrontToBack.Helpers.Extensions;
using ProniaFrontToBack.Models;

namespace ProniaFrontToBack.Areas.Manage.Controllers
{
    [Area("Manage")]
    [Authorize(Roles = "Admin")]
    public class ProductController : Controller
    {
        AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(c => c.Category)
                .Include(c => c.TagProducts)
                .ThenInclude(p => p.Tag)
                .Include(p => p.ProductImages)
                .ToListAsync();
            return View(products);
        }

        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Tags = _context.Tags.ToList();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateProductVm vm)
        {
            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Tags = _context.Tags.ToList();
            if (!ModelState.IsValid)
            {
                return View();
            }
            if (vm.CategoryId != null)
            {
                if (!await _context.Categories.AnyAsync(c => c.Id == vm.CategoryId))
                {
                    ModelState.AddModelError("CategoryId", $"{vm.CategoryId} id-li Category yoxdur");
                    return View();
                }
            }

            Product product = new Product()
            {
                Name = vm.Name,
                Price = vm.Price,
                Description = vm.Description,
                CategoryId = vm.CategoryId,
                SKU = vm.SKU,
                ProductImages = new List<ProductImage>()
            };

            if (vm.TagIds != null)
            {
                foreach (var tagId in vm.TagIds)
                {
                    if (!(await _context.Tags.AnyAsync(t => t.Id == tagId)))
                    {
                        ModelState.AddModelError("TagIds", $"{tagId} id-li tag yoxdur");
                        return View();
                    }

                    TagProduct tagProduct = new TagProduct()
                    {
                        TagId = tagId,
                        Product = product
                    };
                    _context.TagProducts.Add(tagProduct);
                }
            }

            List<string> error = new List<string>();
            if (vm.MainPhoto.ContentType.Contains("/image"))
            {
                ModelState.AddModelError("MainPhoto", "Duzgun sekil formati daxil edin");
                return View(vm);
            }
            if (vm.MainPhoto.Length > 3000000)
            {
                ModelState.AddModelError("MainPhoto", "Maksimum 3 mb sekil daxil ede bilersen");
                return View(vm);
            }
            product.ProductImages.Add(new()
            {
                Primary = true,
                ImgUrl = vm.MainPhoto.Upload(_env.WebRootPath, "Upload/Product")
            });
            foreach (var item in vm.Images)
            {
                if (item.ContentType.Contains("/image"))
                {
                    error.Add($"{item.Name} image formatinda deyil");
                    continue;
                }
                if (item.Length > 3000000)
                {
                    error.Add($"Sekilin olcusu max 3 mb ola biler");
                    continue;
                }
                product.ProductImages.Add(new()
                {
                    Primary = false,
                    ImgUrl = item.Upload(_env.WebRootPath, "Upload/Product")
                });

            }

            TempData["error"] = error;

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || !(_context.Products.Any(c => c.Id == id)))
            {
                return View("Error");
            }

            var product = await _context.Products
                .Include(p => p.ProductImages)
                .Include(c => c.Category)
                .Include(c => c.TagProducts)
                .ThenInclude(c => c.Tag)
                .FirstOrDefaultAsync(p => p.Id == id);

            ViewBag.Categories = _context.Categories.ToList();
            ViewBag.Tags = _context.Tags.ToList();

            UpdateProductVm updateProductVm = new UpdateProductVm()
            {
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                CategoryId = product.CategoryId,
                TagIds = new List<int>(),
                productImages = new List<ProductImageVm>()
            };

            foreach (var item in product.TagProducts)
            {
                updateProductVm.TagIds.Add(item.TagId);
            }
            foreach (var item in product.ProductImages)
            {
                updateProductVm.productImages.Add(new()
                {
                    Primary = item.Primary,
                    ImgUrl = item.ImgUrl
                });
            }



            return View(updateProductVm);
        }
        [HttpPost]

        public async Task<IActionResult> Update(UpdateProductVm vm)
        {
            ViewBag.Categories = _context.Categories.ToList();
            if (vm.Id == null || !(_context.Products.Any(c => c.Id == vm.Id)))
            {
                return View("Error");
            }
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            Product oldProduct = _context.Products.Include(p => p.TagProducts).Include(p => p.ProductImages).FirstOrDefault(x => x.Id == vm.Id);
            if (oldProduct == null)
            {
                return View("Error");
            }
            if (vm.CategoryId != null)
            {
                if (!await _context.Categories.AnyAsync(c => c.Id == vm.CategoryId))
                {
                    ModelState.AddModelError("CategoryId", $"{vm.CategoryId} id-li Category yoxdur");
                    return View();
                }
            }

            _context.TagProducts.RemoveRange(oldProduct.TagProducts);

            if (vm.TagIds.Count != null)
            {
                foreach (var item in vm.TagIds)
                {
                    await _context.TagProducts.AddAsync(new TagProduct()
                    {
                        ProductId = oldProduct.Id,
                        TagId = item
                    });
                }
            }

            if (vm.MainPhoto != null)
            {
                if (vm.MainPhoto.ContentType.Contains("/image"))
                {
                    ModelState.AddModelError("MainPhoto", "Duzgun sekil formati daxil edin");
                    return View(vm);
                }
                if (vm.MainPhoto.Length > 3000000)
                {
                    ModelState.AddModelError("MainPhoto", "Maksimum 3 mb sekil daxil ede bilersen");
                    return View(vm);
                }

                FileExtension.DeleteFile(_env.WebRootPath, "Upload/Product", oldProduct.ProductImages.FirstOrDefault(x => x.Primary).ImgUrl);
                _context.ProductImages.Remove(oldProduct.ProductImages.FirstOrDefault(x => x.Primary));

                oldProduct.ProductImages.Add(new()
                {
                    Primary = true,
                    ImgUrl = vm.MainPhoto.Upload(_env.WebRootPath, "Upload/Product")
                });
            }

            if (vm.ImagesUrls != null)
            {
                var removeImage = new List<ProductImage>();

                foreach (var item in oldProduct.ProductImages.Where(x => !x.Primary))
                {
                    if (!vm.ImagesUrls.Any(x => x == item.ImgUrl))
                    {
                        FileExtension.DeleteFile(_env.WebRootPath, "Upload/Product", item.ImgUrl);
                        _context.ProductImages.Remove(item);
                    }
                }



            }
            else
            {
                foreach (var item in oldProduct.ProductImages.Where(x => !x.Primary))
                {
                    FileExtension.DeleteFile(_env.WebRootPath, "Upload/Product", item.ImgUrl);
                    _context.ProductImages.Remove(item);
                }
            }

            if (vm.Images != null)
            {
                foreach (var item in vm.Images)
                {
                    if (item.ContentType.Contains("/image"))
                    {
                        continue;
                    }
                    if (item.Length > 3000000)
                    {
                        continue;
                    }
                    oldProduct.ProductImages.Add(new()
                    {
                        Primary = false,
                        ImgUrl = item.Upload(_env.WebRootPath, "Upload/Product")
                    });

                }
            }


            oldProduct.Name = vm.Name;
            oldProduct.Price = vm.Price;
            oldProduct.Description = vm.Description;
            oldProduct.CategoryId = vm.CategoryId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            if (id == null) return BadRequest();
            var product = _context.Products.FirstOrDefault(x => x.Id == id);
            if (product == null) return NotFound();
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
