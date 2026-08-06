using CoffeeShop.Data;
using CoffeeShop.Extensions;
using CoffeeShop.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Controllers
{
    public class CartController : Controller
    {
        private readonly CoffeeDbContext _context;

        public CartController(CoffeeDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart")
                       ?? new List<CartItem>();

            return View(cart);
        }
        public IActionResult AddToCart(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.ProductId == id);

            if (product == null)
                return NotFound();

            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart")
                       ?? new List<CartItem>();

            var item = cart.FirstOrDefault(c => c.Product.ProductId == id);

            if (item == null)
            {
                cart.Add(new CartItem
                {
                    Product = product,
                    Quantity = 1
                });
            }
            else
            {
                item.Quantity++;
            }

            HttpContext.Session.SetObject("Cart", cart);

            return RedirectToAction("Index");
        }
        public IActionResult Increase(int id)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart")
                       ?? new List<CartItem>();

            var item = cart.FirstOrDefault(x => x.Product.ProductId == id);

            if (item != null)
            {
                item.Quantity++;
            }

            HttpContext.Session.SetObject("Cart", cart);

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Decrease(int id)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart")
                       ?? new List<CartItem>();

            var item = cart.FirstOrDefault(x => x.Product.ProductId == id);

            if (item != null)
            {
                item.Quantity--;

                if (item.Quantity <= 0)
                {
                    cart.Remove(item);
                }
            }

            HttpContext.Session.SetObject("Cart", cart);

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Remove(int id)
        {
            var cart = HttpContext.Session.GetObject<List<CartItem>>("Cart")
                       ?? new List<CartItem>();

            var item = cart.FirstOrDefault(x => x.Product.ProductId == id);

            if (item != null)
            {
                cart.Remove(item);
            }

            HttpContext.Session.SetObject("Cart", cart);

            return RedirectToAction(nameof(Index));
        }
        public IActionResult Checkout()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return RedirectToAction("Index", "Checkout");
        }
    }
}