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

        // Action Index() - Cập nhật để lấy nhiều dữ liệu hơn cho giao diện mới
        public async Task<IActionResult> Index()
        {
            // 1. Lấy danh sách TOP Sách Thịnh Hành (Tăng lên 10-12 cuốn để hiển thị nhiều khung)
            // Lưu ý: Nếu DB của bạn là 'Sach' thay vì 'Saches', hãy đổi tên lại cho đúng
            var topBooks = await _context.Saches
                .Take(12)
                .ToListAsync();

            // 2. Lấy danh sách Sách Nổi Bật để làm Banner di chuyển (Lấy 5 cuốn mới nhất)
            var featuredBooks = await _context.Saches
                .Include(s => s.MaTlNavigation)
                .OrderByDescending(s => s.MaSach) // Ưu tiên sách mới nhập
                .Take(5)
                .ToListAsync();

            // Gửi dữ liệu sang View thông qua ViewBag
            ViewBag.TopBooks = topBooks;
            ViewBag.FeaturedBooks = featuredBooks;

            // Trả về Model là danh sách toàn bộ sách để dự phòng
            return View(topBooks);
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