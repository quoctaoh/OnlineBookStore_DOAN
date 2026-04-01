using Microsoft.AspNetCore.Mvc;
using OnlineBookStore_Web.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace OnlineBookStore_Web.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly OnlineBookstore_DOANContext _context;

        public CheckoutController(OnlineBookstore_DOANContext context)
        {
            _context = context;
        }

        // INDEX (GET) - HIỂN THỊ FORM
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account"); // Yêu cầu đăng nhập
            }

            // 1. Lấy thông tin Giỏ hàng (đã JOIN với Sách)
            var cart = await _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                .ThenInclude(ct => ct.MaSachNavigation)
                .FirstOrDefaultAsync(g => g.MaNd == userId.Value);

            if (cart == null || !cart.ChiTietGioHangs.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống. Vui lòng thêm sản phẩm.";
                return RedirectToAction("Index", "Cart"); // Chuyển hướng nếu giỏ hàng trống
            }

            // 2. Lấy thông tin Hồ sơ người dùng (để điền tự động vào form)
            var user = await _context.NguoiDungs.FindAsync(userId.Value);

            // 3. Truyền dữ liệu sang View
            ViewBag.CartDetails = cart.ChiTietGioHangs.ToList();
            ViewBag.CurrentUser = user;

            return View();
        }

        // PLACEORDER (POST) - TẠO ĐƠN HÀNG
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(string HoTen, string DienThoai, string DiaChiGiaoHang, string GhiChu)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // 1. Lấy thông tin Giỏ hàng hiện tại (JOIN Chi tiết)
            var cart = await _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                .ThenInclude(ct => ct.MaSachNavigation)
                .FirstOrDefaultAsync(g => g.MaNd == userId.Value);

            // Kiểm tra tồn kho trước khi đặt hàng (Nên làm)
            foreach (var item in cart.ChiTietGioHangs)
            {
                var sach = await _context.Saches.FindAsync(item.MaSach);
                if (sach == null || sach.SoLuongTon < item.SoLuong)
                {
                    TempData["Error"] = $"Sản phẩm '{item.MaSachNavigation.TenSach}' không đủ số lượng tồn kho.";
                    return RedirectToAction("Index", "Cart");
                }
            }


            // Tính tổng tiền
            decimal totalAmount = cart.ChiTietGioHangs.Sum(ct => ct.SoLuong * ct.MaSachNavigation.GiaBan);

            // 2. TẠO ĐƠN HÀNG (Bảng DonHang)
            var order = new DonHang
            {
                MaNd = userId.Value,
                NgayDatHang = DateTime.Now,
                TongTien = totalAmount,
                TrangThaiDh = "Chờ xác nhận", // Trạng thái mặc định
                DiaChiGiaoHang = DiaChiGiaoHang // Lấy từ form
            };

            _context.DonHangs.Add(order);
            await _context.SaveChangesAsync(); // Lưu để lấy MaDH tự tăng

            // 3. TẠO CHI TIẾT ĐƠN HÀNG (Bảng ChiTietDonHang) & Cập nhật tồn kho
            foreach (var item in cart.ChiTietGioHangs)
            {
                var orderDetail = new ChiTietDonHang
                {
                    MaDh = order.MaDh, // Dùng MaDH vừa tạo
                    MaSach = item.MaSach,
                    SoLuong = item.SoLuong,
                    DonGia = item.MaSachNavigation.GiaBan // Giá tại thời điểm đặt hàng
                };
                _context.ChiTietDonHangs.Add(orderDetail);

                // Cập nhật tồn kho (trừ đi số lượng đã đặt)
                var sach = await _context.Saches.FindAsync(item.MaSach);
                if (sach != null)
                {
                    sach.SoLuongTon -= item.SoLuong;
                }
            }

            // 4. XÓA GIỎ HÀNG cũ và lưu thay đổi tồn kho
            _context.ChiTietGioHangs.RemoveRange(cart.ChiTietGioHangs);

            await _context.SaveChangesAsync();

            // 5. Chuyển hướng đến trang Xác nhận Đơn hàng
            return RedirectToAction("OrderConfirmation", new { orderId = order.MaDh });
        }

        // ORDERCONFIRMATION (GET) - TRANG XÁC NHẬN
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            // Lấy thông tin đơn hàng và chi tiết để hiển thị
            var order = await _context.DonHangs
                .Include(o => o.ChiTietDonHangs)
                .ThenInclude(ct => ct.MaSachNavigation)
                .FirstOrDefaultAsync(o => o.MaDh == orderId && o.MaNd == userId.Value);

            if (order == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin đơn hàng này.";
                return RedirectToAction("Index", "Book");
            }

            return View(order);
        }
    }
}