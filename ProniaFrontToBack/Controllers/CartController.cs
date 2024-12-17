using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using ProniaFrontToBack.DAL;
using ProniaFrontToBack.ViewModels.Basket;

namespace ProniaFrontToBack.Controllers
{
    public class CartController : Controller
    {
        AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var json = Request.Cookies["basket"];
            List<CookieItemVm> cookies = new List<CookieItemVm>();

            if (json != null)
            {
                cookies = JsonConvert.DeserializeObject<List<CookieItemVm>>(json);
            }

            List<CartVm> cart = new List<CartVm>();
            List<CookieItemVm> deleteItem = new List<CookieItemVm>();

            if (cookies.Count > 0)
            {
                cookies.ForEach(c =>
                {
                    var product = _context.Products.Include(x => x.ProductImages).FirstOrDefault(p => p.Id == c.Id);

                    if (product == null)
                    {
                        deleteItem.Add(c);
                    }
                    else
                    {
                        cart.Add(new CartVm()
                        {
                            Id = c.Id,
                            Price = product.Price,
                            Name = product.Name,
                            ImgUrl = product.ProductImages.FirstOrDefault(p => p.Primary).ImgUrl
                        });
                    }
                });
                if (deleteItem.Count>0)
                {
                    deleteItem.ForEach(d =>
                    {
                        cookies.Remove(d);
                    });
                    Response.Cookies.Append("basket", JsonConvert.SerializeObject(cookies));
                }                
            }

            return View(cart);
        }


        public async Task<IActionResult> AddBasket(int itemId)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == itemId);
            if (product == null) return NotFound();

            List<CookieItemVm> cookiesList;

            var basket = Request.Cookies["basket"];

            if (basket != null)
            {
                cookiesList = JsonConvert.DeserializeObject<List<CookieItemVm>>(basket);
                var existProduct = cookiesList.FirstOrDefault(x => x.Id == itemId);

                if (existProduct != null)
                {
                    existProduct.Count++;
                }
                else
                {
                    cookiesList.Add(new CookieItemVm()
                    {
                        Id = itemId,
                        Count = 1
                    });
                }

            }
            else
            {

                cookiesList = new List<CookieItemVm>();
                cookiesList.Add(new CookieItemVm()
                {
                    Id = itemId,
                    Count = 1
                });

            }

            Response.Cookies.Append("basket", JsonConvert.SerializeObject(cookiesList));

            return RedirectToAction("Index", "Home");
        }
        public IActionResult GetBasket()
        {
            var json = JsonConvert.DeserializeObject<CookieItemVm>(Request.Cookies["basket"]);
            return View();
        }



    }
}
