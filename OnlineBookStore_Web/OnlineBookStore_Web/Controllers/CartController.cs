using Microsoft.AspNetCore.Mvc;
using OnlineBookStore_Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace OnlineBookStore_Web.Controllers
{
    public class CartController : Controller
    {
        private readonly OnlineBookstore_DOANContext _context;

        public CartController(OnlineBookstore_DOANContext context)
        {
            _context = context;
        }

        // INDEX (Hiển thị chi tiết Giỏ hàng)
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            // 1. Kiểm tra đăng nhập
            if (!userId.HasValue)
            {
                TempData["Message"] = "Vui lòng đăng nhập để xem giỏ hàng.";
                return RedirectToAction("Login", "Account");
            }

            // 2. Lấy Mã Giỏ hàng
            var cart = await _context.GioHangs
                .FirstOrDefaultAsync(g => g.MaNd == userId.Value);

            if (cart == null)
            {
                return View(new List<ChiTietGioHang>());
            }

            // 3. Lấy Chi tiết Giỏ hàng (JOIN với bảng Sach)
            var cartDetails = await _context.ChiTietGioHangs
                .Where(ct => ct.MaGh == cart.MaGh)
                // Phải Include để lấy thông tin Tên Sách, Giá Bán, Tác giả
                .Include(ct => ct.MaSachNavigation)
                .ToListAsync();

            // 4. Truyền chi tiết giỏ hàng sang View
            return View(cartDetails);
        }

        // THÊM SÁCH VÀO GIỎ (ADD TO CART)
        // POST: /Cart/AddToCart 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int MaSach, int SoLuong = 1)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                TempData["Message"] = "Vui lòng đăng nhập để thêm sản phẩm vào giỏ hàng.";
                return RedirectToAction("Login", "Account");
            }

            var cart = await _context.GioHangs
                .FirstOrDefaultAsync(g => g.MaNd == userId.Value);

            // Kiểm tra số lượng tồn kho 
            var sach = await _context.Saches.FindAsync(MaSach);
            if (sach == null || sach.SoLuongTon < SoLuong)
            {
                TempData["Error"] = "Số lượng đặt vượt quá tồn kho.";
                return RedirectToAction("Details", "Book", new { id = MaSach });
            }

            var cartDetail = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(ct => ct.MaGh == cart.MaGh && ct.MaSach == MaSach);

            if (cartDetail != null)
            {
                cartDetail.SoLuong += SoLuong;
                _context.Update(cartDetail);
            }
            else
            {
                var newDetail = new ChiTietGioHang
                {
                    MaGh = cart.MaGh,
                    MaSach = MaSach,
                    SoLuong = SoLuong
                };
                _context.Add(newDetail);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Thêm sản phẩm vào giỏ hàng thành công!";
            return RedirectToAction("Index"); // Chuyển hướng về trang Giỏ hàng
        }

        // CẬP NHẬT VÀ XÓA (Logic đã được triển khai trước đó)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveItem(int MaSach)
        {
            // Logic xóa sản phẩm khỏi giỏ hàng
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var cart = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaNd == userId.Value);
            if (cart == null) return RedirectToAction("Index");

            var cartDetail = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(ct => ct.MaGh == cart.MaGh && ct.MaSach == MaSach);

            if (cartDetail != null)
            {
                _context.ChiTietGioHangs.Remove(cartDetail);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int MaSach, int SoLuong)
        {
            // Logic cập nhật số lượng
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            if (SoLuong <= 0) return RedirectToAction("RemoveItem", new { MaSach = MaSach });

            var cart = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaNd == userId.Value);
            if (cart == null) return RedirectToAction("Index");

            var cartDetail = await _context.ChiTietGioHangs
                .FirstOrDefaultAsync(ct => ct.MaGh == cart.MaGh && ct.MaSach == MaSach);

            if (cartDetail != null)
            {
                cartDetail.SoLuong = SoLuong;
                _context.Update(cartDetail);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật số lượng thành công.";
            }

            return RedirectToAction("Index");
        }
    }
}