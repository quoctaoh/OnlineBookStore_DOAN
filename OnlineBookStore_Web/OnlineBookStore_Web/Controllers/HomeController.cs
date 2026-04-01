using Microsoft.AspNetCore.Mvc;
using OnlineBookStore_Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace OnlineBookStore_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly OnlineBookstore_DOANContext _context;

        public HomeController(OnlineBookstore_DOANContext context)
        {
            _context = context;
        }

        // Action Index()
        public async Task<IActionResult> Index()
        {
            // 1. Lấy danh sách TOP Sách
            var topBooks = await _context.Saches
                .Take(5)
                .ToListAsync();

            // Lấy sách nổi bật
            var featuredBook = await _context.Saches
                .Include(s => s.MaTlNavigation) 
                .FirstOrDefaultAsync(s => s.MaSach == 1);

            ViewBag.TopBooks = topBooks;
            ViewBag.FeaturedBook = featuredBook;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}