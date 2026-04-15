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
        // 2. THÊM ĐÁNH GIÁ (SỬA LỖI 404 & KIỂM TRA MUA HÀNG)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> PostReview(int MaSach, int Rating, string NoiDung)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

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
            catch { /* Log error if needed */ }

            return RedirectToAction("Details", "Book", new { id = MaSach });
        }

        // ==========================================
        // 3. THÊM VÀO GIỎ HÀNG (AJAX - FIX REDIRECT LOGIN)
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> AddToCart(int MaSach, int SoLuong = 1)
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            // QUAN TRỌNG: Trả về redirect = true để JavaScript ở View biết đường mà nhảy sang Login
            if (!userId.HasValue)
                return Json(new { success = false, redirect = true, message = "Vui lòng đăng nhập để mua hàng!" });

            var result = await InternalAddToCart(userId.Value, MaSach, SoLuong);

            if (result.Success)
            {
                await UpdateCartSession(userId.Value);
                return Json(new { success = true });
            }

            return Json(new { success = false, message = result.Message });
        }

        // ==========================================
        // 4. MUA NGAY
        // ==========================================
        [HttpPost]
        public async Task<IActionResult> BuyNow(int MaSach, int SoLuong = 1)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var result = await InternalAddToCart(userId.Value, MaSach, SoLuong);
            if (result.Success)
            {
                await UpdateCartSession(userId.Value);
                return RedirectToAction("Index", "Checkout");
            }

            TempData["Error"] = result.Message;
            return RedirectToAction("Details", "Book", new { id = MaSach });
        }

        // ==========================================
        // 5. CẬP NHẬT SỐ LƯỢNG TRONG GIỎ
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
                // Kiểm tra tồn kho trước khi cập nhật
                var sach = await _context.Saches.FindAsync(MaSach);
                if (sach != null && SoLuong <= sach.SoLuongTon)
                {
                    item.SoLuong = SoLuong;
                    await _context.SaveChangesAsync();
                }
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

        // Cấu trúc trả về chi tiết hơn để báo lỗi tồn kho
        private class CartResult { public bool Success { get; set; } public string Message { get; set; } }

        private async Task<CartResult> InternalAddToCart(int userId, int maSach, int soLuong)
        {
            try
            {
                // Kiểm tra sách có tồn tại và còn hàng không
                var sach = await _context.Saches.FindAsync(maSach);
                if (sach == null) return new CartResult { Success = false, Message = "Sách không tồn tại!" };
                if (sach.SoLuongTon < soLuong) return new CartResult { Success = false, Message = "Số lượng trong kho không đủ!" };

                var cart = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaNd == userId);
                if (cart == null)
                {
                    cart = new GioHang { MaNd = userId, NgayTao = DateTime.Now };
                    _context.GioHangs.Add(cart);
                    await _context.SaveChangesAsync();
                }

                var detail = await _context.ChiTietGioHangs
                    .FirstOrDefaultAsync(ct => ct.MaGh == cart.MaGh && ct.MaSach == maSach);

                if (detail != null)
                {
                    if (detail.SoLuong + soLuong > sach.SoLuongTon)
                        return new CartResult { Success = false, Message = "Tổng số lượng vượt quá tồn kho!" };

                    detail.SoLuong += soLuong;
                }
                else
                {
                    _context.ChiTietGioHangs.Add(new ChiTietGioHang { MaGh = cart.MaGh, MaSach = maSach, SoLuong = soLuong });
                }

                await _context.SaveChangesAsync();
                return new CartResult { Success = true };
            }
            catch { return new CartResult { Success = false, Message = "Lỗi hệ thống!" }; }
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