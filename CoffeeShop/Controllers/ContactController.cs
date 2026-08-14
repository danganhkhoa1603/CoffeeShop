using Coffee.Models;
using CoffeeShop.Data;
using Microsoft.AspNetCore.Mvc;

namespace CoffeeShop.Controllers
{
    public class ContactController : Controller
    {
        private readonly CoffeeDbContext _context;

        public ContactController(CoffeeDbContext context)
        {
            _context = context;
        }

        // =========================
        // ADMIN - Danh sách liên hệ
        // =========================
        public IActionResult Index()
        {
            var list = _context.Contacts
                               .OrderByDescending(x => x.CreatedAt)
                               .ToList();

            return View(list);
        }

        // =========================
        // KHÁCH GỬI LIÊN HỆ
        // =========================
        [HttpPost]
        public IActionResult Contact(Contact contact)
        {
            if (!ModelState.IsValid)
            {
                return View("~/Views/Home/Contact.cshtml", contact);
            }

            contact.CreatedAt = DateTime.Now;
            contact.IsRead = false;

            _context.Contacts.Add(contact);
            _context.SaveChanges();

            TempData["Success"] = "Gửi liên hệ thành công!";

            return RedirectToAction("Contact", "Home");
        }

        // =========================
        // ADMIN Đánh dấu đã đọc
        // =========================
        public IActionResult Read(int id)
        {
            var contact = _context.Contacts.Find(id);

            if (contact == null)
            {
                return NotFound();
            }

            contact.IsRead = true;
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}