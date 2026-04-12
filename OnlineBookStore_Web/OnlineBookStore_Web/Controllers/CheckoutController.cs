using Microsoft.AspNetCore.Mvc;
using OnlineBookStore_Web.Models;
using Microsoft.EntityFrameworkCore;
using Net.payOS;
using Net.payOS.Types;

namespace OnlineBookStore_Web.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly OnlineBookstore_DOANContext _context;
        // Sử dụng định danh rõ ràng để tránh lỗi Namespace CS0118
        private readonly Net.payOS.PayOS _payOS;

        public CheckoutController(OnlineBookstore_DOANContext context, Net.payOS.PayOS payOSService)
        {
            _context = context;
            _payOS = payOSService;
        }

        // 1. Hiển thị trang thanh toán
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var user = await _context.NguoiDungs.FindAsync(userId.Value);
            ViewBag.User = user;

            var cartItems = await GetCartItems(userId.Value);
            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            return View(cartItems);
        }

        // 2. Xử lý đặt hàng chính
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOrder(string PaymentMethod)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var cartItems = await GetCartItems(userId.Value);
            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            // Nếu chọn PayOS (VietQR)
            if (PaymentMethod == "PayOS")
            {
                long totalAmount = (long)cartItems.Sum(x => x.SoLuong * x.MaSachNavigation.GiaBan);
                return await CreatePayOSPayment(totalAmount, cartItems);
            }

            // Nếu chọn COD hoặc các phương thức khác, lưu đơn và chuyển về lịch sử
            return await SaveOrderAndRedirect(userId.Value, "COD");
        }

        // 3. Logic tạo link thanh toán PayOS
        private async Task<IActionResult> CreatePayOSPayment(long totalAmount, List<ChiTietGioHang> cartItems)
        {
            // Tạo mã đơn hàng số (PayOS yêu cầu kiểu int/long cho OrderCode)
            int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff"));

            // Fix lỗi tên sản phẩm quá dài (giới hạn 20 ký tự)
            var items = cartItems.Select(x => new ItemData(
                x.MaSachNavigation.TenSach.Length > 20 ? x.MaSachNavigation.TenSach.Substring(0, 20) : x.MaSachNavigation.TenSach,
                (int)x.SoLuong,
                (int)x.MaSachNavigation.GiaBan)).ToList();

            // Fix lỗi Description quá 25 ký tự
            var paymentData = new PaymentData(
                orderCode,
                (int)totalAmount,
                $"Thanh toan OBS {orderCode}",
                items,
                $"{Request.Scheme}://{Request.Host}/Checkout/Cancel",
                $"{Request.Scheme}://{Request.Host}/Checkout/PaymentSuccess"
            );

            var response = await _payOS.createPaymentLink(paymentData);
            return Redirect(response.checkoutUrl);
        }

        // 4. Callback khi PayOS thanh toán thành công
        public async Task<IActionResult> PaymentSuccess()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Index", "Home");

            return await SaveOrderAndRedirect(userId.Value, "PayOS");
        }

        // 5. Hàm dùng chung để lưu đơn hàng, xóa giỏ và chuyển hướng về Lịch sử
        private async Task<IActionResult> SaveOrderAndRedirect(int userId, string method)
        {
            // 1. Lấy thông tin giỏ hàng và người dùng
            var cart = await _context.GioHangs
                .Include(g => g.ChiTietGioHangs)
                .ThenInclude(ct => ct.MaSachNavigation)
                .FirstOrDefaultAsync(g => g.MaNd == userId);

            if (cart == null || !cart.ChiTietGioHangs.Any())
                return RedirectToAction("Index", "Cart");

            var user = await _context.NguoiDungs.FindAsync(userId);

            // 2. Tạo đối tượng đơn hàng mới
            var newOrder = new DonHang
            {
                MaNd = userId,
                NgayDatHang = DateTime.Now,
                TongTien = cart.ChiTietGioHangs.Sum(ct => ct.SoLuong * ct.MaSachNavigation.GiaBan),
                TrangThaiDh = "Đang chờ xử lý", // Trạng thái mặc định
                DiaChiGiaoHang = user.DiaChi, // Lấy địa chỉ từ thông tin user
                ChiTietDonHangs = new List<ChiTietDonHang>()
            };

            // 3. Chuyển đổi từ ChiTietGioHang sang ChiTietDonHang
            foreach (var item in cart.ChiTietGioHangs)
            {
                newOrder.ChiTietDonHangs.Add(new ChiTietDonHang
                {
                    MaSach = item.MaSach,
                    SoLuong = item.SoLuong,
                    DonGia = item.MaSachNavigation.GiaBan
                });
            }

            // 4. Lưu vào Database
            _context.DonHangs.Add(newOrder);

            // 5. Xóa các sản phẩm trong giỏ hàng sau khi đã đặt hàng
            _context.ChiTietGioHangs.RemoveRange(cart.ChiTietGioHangs);

            await _context.SaveChangesAsync();

            // 6. Cập nhật lại Session số lượng giỏ hàng về 0
            HttpContext.Session.SetInt32("CartCount", 0);
            TempData["SuccessMessage"] = $"Đặt hàng thành công qua {method}!";

            // 7. ĐIỀU HƯỚNG: Quay về hàm History trong AccountController
            // Vì hàm History của bạn nằm trong AccountController nên dùng:
            return RedirectToAction("History", "Account");
        }

        public IActionResult Cancel() => RedirectToAction("Index", "Cart");

        // Helper lấy giỏ hàng kèm thông tin sách
        private async Task<List<ChiTietGioHang>> GetCartItems(int userId)
        {
            return await _context.ChiTietGioHangs
                .Include(ct => ct.MaSachNavigation)
                .Where(ct => ct.MaGhNavigation.MaNd == userId)
                .ToListAsync();
        }
    }
}