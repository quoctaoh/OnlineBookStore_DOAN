using Microsoft.AspNetCore.Mvc;
using OnlineBookStore_Web.Models;
using Microsoft.EntityFrameworkCore;

namespace OnlineBookStore_Web.Controllers
{
    public class BookController : Controller
    {
        // Khai báo Context với tên chính xác
        private readonly OnlineBookstore_DOANContext _context;

        // Constructor: Nhận Context từ hệ thống (Dependency Injection)
        public BookController(OnlineBookstore_DOANContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string searchString, int? categoryId)
        {
            var sachs = _context.Saches.AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                sachs = sachs.Where(s =>
                    s.TenSach.Contains(searchString) ||
                    s.TacGia.Contains(searchString));
            }

            if (categoryId.HasValue)
            {
                sachs = sachs.Where(s => s.MaTl == categoryId.Value);
            }

            // Lấy danh sách Thể loại (để hiển thị trên bộ lọc)
            ViewBag.Categories = await _context.TheLoais.ToListAsync();
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentSearch = searchString;

            return View(await sachs.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id) 
        {
            // 1. Kiểm tra ID có hợp lệ không
            if (id == null)
            {
                return NotFound(); // Trả về lỗi 404 nếu không có ID
            }

            // 2. Truy vấn sách từ CSDL bằng ID
            var sach = await _context.Saches
                // Bao gồm luôn thông tin Thể loại và NXB (Dùng .Include() để JOIN)
                .Include(s => s.MaTlNavigation)
                .Include(s => s.MaNxbNavigation)
                .FirstOrDefaultAsync(m => m.MaSach == id);

            // 3. Kiểm tra sách có tồn tại không
            if (sach == null)
            {
                return NotFound(); // Trả về lỗi 404 nếu không tìm thấy sách
            }

            // 4. Truyền đối tượng Sach sang View
            return View(sach);
        }
    }
}