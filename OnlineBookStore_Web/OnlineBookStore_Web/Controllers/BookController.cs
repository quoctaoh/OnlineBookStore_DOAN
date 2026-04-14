using Microsoft.AspNetCore.Mvc;
using OnlineBookStore_Web.Models;
using Microsoft.EntityFrameworkCore;

namespace OnlineBookStore_Web.Controllers
{
    public class BookController : Controller
    {
        private readonly OnlineBookstore_DOANContext _context;

        public BookController(OnlineBookstore_DOANContext context)
        {
            _context = context;
        }

        // ==========================================
        // 1. TRANG DANH SÁCH SÁCH (Để sửa lỗi 404 /Book)
        // ==========================================
        public async Task<IActionResult> Index(string searchString, int? categoryId)
        {
            // Lấy danh sách sách kèm thể loại
            var sachs = _context.Saches.Include(s => s.MaTlNavigation).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                sachs = sachs.Where(s => s.TenSach.Contains(searchString) || s.TacGia.Contains(searchString));
            }

            if (categoryId.HasValue)
            {
                sachs = sachs.Where(s => s.MaTl == categoryId.Value);
            }

            ViewBag.Categories = await _context.TheLoais.ToListAsync();
            return View(await sachs.ToListAsync());
        }

        // ==========================================
        // 2. TRANG CHI TIẾT SÁCH (Đã ép quyền đánh giá)
        // ==========================================
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var sach = await _context.Saches
                .Include(s => s.MaTlNavigation)
                .Include(s => s.MaNxbNavigation)
                .Include(s => s.DanhGias)
                    .ThenInclude(dg => dg.MaNdNavigation)
                .FirstOrDefaultAsync(m => m.MaSach == id);

            if (sach == null) return NotFound();

            // Lấy UserId từ Session để kiểm tra quyền đánh giá
            var userId = HttpContext.Session.GetInt32("UserId");
            bool hasPurchased = false;

            if (userId.HasValue)
            {
                // Kiểm tra: Đã mua sách + Trạng thái đơn hàng khớp với DB của Nguyên (TrangThaiDh)
                hasPurchased = await _context.ChiTietDonHangs
                    .AnyAsync(ct => ct.MaSach == id &&
                                    ct.MaDhNavigation.MaNd == userId &&
                                 ct.MaDhNavigation.TrangThaiDh == "Đã giao thành công");
            }

            ViewBag.HasPurchased = hasPurchased;
            ViewBag.IsLoggedIn = userId.HasValue;

            return View(sach);
        }
    }
}