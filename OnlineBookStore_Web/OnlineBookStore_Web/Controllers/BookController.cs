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

        // ==========================================
        // 1. TRANG DANH MỤC SÁCH
        // ==========================================
        public async Task<IActionResult> Index(string searchString, int? categoryId, string sortOrder, decimal? minPrice, decimal? maxPrice)
        {
            // Lấy query gốc từ bảng Sách
            var sachs = _context.Saches.AsQueryable();

            // Lọc theo tên hoặc tác giả
            if (!string.IsNullOrEmpty(searchString))
            {
                sachs = sachs.Where(s =>
                    s.TenSach.Contains(searchString) ||
                    s.TacGia.Contains(searchString));
            }

            // Lọc theo thể loại
            if (categoryId.HasValue)
            {
                sachs = sachs.Where(s => s.MaTl == categoryId.Value);
            }

            // Lọc theo khoảng giá
            if (minPrice.HasValue) sachs = sachs.Where(s => s.GiaBan >= minPrice.Value);
            if (maxPrice.HasValue) sachs = sachs.Where(s => s.GiaBan <= maxPrice.Value);

            // Sắp xếp theo giá
            switch (sortOrder)
            {
                case "price_asc":
                    sachs = sachs.OrderBy(s => s.GiaBan);
                    break;
                case "price_desc":
                    sachs = sachs.OrderByDescending(s => s.GiaBan);
                    break;
                default:
                    sachs = sachs.OrderByDescending(s => s.MaSach); // Mới nhất lên đầu
                    break;
            }

            // Truyền dữ liệu ra View qua ViewBag
            ViewBag.Categories = await _context.TheLoais.ToListAsync();
            ViewBag.CurrentCategory = categoryId;
            ViewBag.CurrentSearch = searchString;
            ViewBag.CurrentSort = sortOrder;

            return View(await sachs.ToListAsync());
        }

        // ==========================================
        // 2. TRANG CHI TIẾT SÁCH (ĐÃ THÊM LOGIC ĐÁNH GIÁ)
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            // 1. Kiểm tra ID có hợp lệ không
            if (id == null)
            {
                return NotFound();
            }

            // 2. Truy vấn sách kèm theo các bảng liên quan
            var sach = await _context.Saches
                .Include(s => s.MaTlNavigation)   // Lấy thông tin Thể loại
                .Include(s => s.MaNxbNavigation)  // Lấy thông tin Nhà xuất bản

                // --- PHẦN QUAN TRỌNG: LẤY ĐÁNH GIÁ ---
                .Include(s => s.DanhGias)          // Lấy danh sách đánh giá của cuốn sách
                    .ThenInclude(dg => dg.MaNdNavigation) // Lấy thông tin người dùng từ bảng NguoiDung (để hiện tên)
                                                          // ------------------------------------

                .FirstOrDefaultAsync(m => m.MaSach == id);

            // 3. Kiểm tra sách có tồn tại không
            if (sach == null)
            {
                return NotFound();
            }

            // 4. Truyền đối tượng Sach sang View
            return View(sach);
        }
    }
}