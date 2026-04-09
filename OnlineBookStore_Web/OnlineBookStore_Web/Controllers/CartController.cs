using Microsoft.AspNetCore.Mvc;
using OnlineBookStore_Web.Models;
using Microsoft.EntityFrameworkCore;

namespace OnlineBookStore_Web.Controllers
{
    public class CartController : Controller
    {
        private readonly OnlineBookstore_DOANContext _context;
        public CartController(OnlineBookstore_DOANContext context) => _context = context;

        // ==========================================
        // 1. TRANG CHỦ GIỎ HÀNG
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var items = await _context.ChiTietGioHangs
                .Include(ct => ct.MaSachNavigation)
                .Where(ct => ct.MaGhNavigation.MaNd == userId.Value)
                .ToListAsync();

            await UpdateCartSession(userId.Value);

            return View(items);
        }

        // ==========================================
        // 2. THÊM ĐÁNH GIÁ (NEW - Giải quyết lỗi 404)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> PostReview(int MaSach, int Rating, string NoiDung)
        {
            // Kiểm tra đăng nhập
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var danhGia = new DanhGia
                {
                    MaSach = MaSach,
                    MaNd = userId.Value,
                    SoSao = Rating,
                    NoiDung = NoiDung,
                    NgayDanhGia = DateTime.Now
                };

                _context.DanhGias.Add(danhGia);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Bạn có thể log lỗi ở đây nếu cần
            }

            // Quay lại trang chi tiết sách sau khi gửi
            return RedirectToAction("Details", "Book", new { id = MaSach });
        }

        // ==========================================
        // 3. THÊM VÀO GIỎ HÀNG (AJAX)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> AddToCart(int MaSach, int SoLuong = 1)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return Json(new { success = false, message = "Vui lòng đăng nhập để mua hàng!" });

            var result = await InternalAddToCart(userId.Value, MaSach, SoLuong);
            if (result) await UpdateCartSession(userId.Value);

            return Json(new { success = result });
        }

        // ==========================================
        // 4. MUA NGAY
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> BuyNow(int MaSach, int SoLuong = 1)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (await InternalAddToCart(userId.Value, MaSach, SoLuong))
            {
                await UpdateCartSession(userId.Value);
                return RedirectToAction("Index", "Checkout");
            }
            return RedirectToAction("Details", "Book", new { id = MaSach });
        }

        // ==========================================
        // 5. CẬP NHẬT SỐ LƯỢNG
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int MaSach, int SoLuong)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var item = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(ct => ct.MaSach == MaSach && ct.MaGhNavigation.MaNd == userId.Value);

            if (item != null && SoLuong > 0)
            {
                item.SoLuong = SoLuong;
                await _context.SaveChangesAsync();
                await UpdateCartSession(userId.Value);
            }
            return RedirectToAction("Index");
        }

        // ==========================================
        // 6. XÓA SẢN PHẨM KHỎI GIỎ
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int MaSach)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                var item = await _context.ChiTietGioHangs
                    .FirstOrDefaultAsync(ct => ct.MaSach == MaSach && ct.MaGhNavigation.MaNd == userId.Value);
                if (item != null)
                {
                    _context.ChiTietGioHangs.Remove(item);
                    await _context.SaveChangesAsync();
                    await UpdateCartSession(userId.Value);
                }
            }
            return RedirectToAction("Index");
        }

        // ==========================================
        // HÀM HỖ TRỢ (HELPERS)
        // ==========================================
        private async Task<bool> InternalAddToCart(int userId, int maSach, int soLuong)
        {
            try
            {
                var cart = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaNd == userId);
                if (cart == null)
                {
                    cart = new GioHang { MaNd = userId, NgayTao = DateTime.Now };
                    _context.GioHangs.Add(cart);
                    await _context.SaveChangesAsync();
                }

                var detail = await _context.ChiTietGioHangs
                    .FirstOrDefaultAsync(ct => ct.MaGh == cart.MaGh && ct.MaSach == maSach);

                if (detail != null) detail.SoLuong += soLuong;
                else _context.ChiTietGioHangs.Add(new ChiTietGioHang { MaGh = cart.MaGh, MaSach = maSach, SoLuong = soLuong });

                await _context.SaveChangesAsync();
                return true;
            }
            catch { return false; }
        }

        private async Task UpdateCartSession(int userId)
        {
            var cart = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaNd == userId);
            if (cart != null)
            {
                var count = await _context.ChiTietGioHangs.Where(ct => ct.MaGh == cart.MaGh).SumAsync(ct => ct.SoLuong);
                HttpContext.Session.SetInt32("CartCount", count);
            }
        }
    }
}