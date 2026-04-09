using Microsoft.AspNetCore.Mvc;
using OnlineBookStore_Web.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;

namespace OnlineBookStore_Web.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly OnlineBookstore_DOANContext _context;
        private readonly IConfiguration _configuration;

        public CheckoutController(OnlineBookstore_DOANContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // Hiển thị trang thanh toán
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var user = await _context.NguoiDungs.FindAsync(userId.Value);
            ViewBag.User = user;

            var cartItems = await _context.ChiTietGioHangs
                .Include(ct => ct.MaSachNavigation)
                .Where(ct => ct.MaGhNavigation.MaNd == userId.Value)
                .ToListAsync();

            if (!cartItems.Any()) return RedirectToAction("Index", "Cart");

            return View(cartItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessOrder(string HoTen, string DiaChi, string SoDienThoai, string PaymentMethod)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue) return RedirectToAction("Login", "Account");

            var cartItems = await _context.ChiTietGioHangs
                .Include(ct => ct.MaSachNavigation)
                .Where(ct => ct.MaGhNavigation.MaNd == userId.Value)
                .ToListAsync();

            long totalAmount = (long)cartItems.Sum(x => x.SoLuong * x.MaSachNavigation.GiaBan);

            // --- XỬ LÝ THANH TOÁN MOMO ---
            if (PaymentMethod == "MoMo")
            {
                string endpoint = "https://test-payment.momo.vn/v2/gateway/api/create";
                string partnerCode = _configuration["Momo:PartnerCode"];
                string accessKey = _configuration["Momo:AccessKey"];
                string secretKey = _configuration["Momo:SecretKey"];

                string orderInfo = "Thanh toán đơn hàng OnlineBookStore";
                string redirectUrl = $"{Request.Scheme}://{Request.Host}/Checkout/Success";
                string ipnUrl = $"{Request.Scheme}://{Request.Host}/Checkout/Success";
                string requestId = DateTime.Now.Ticks.ToString();
                string orderId = DateTime.Now.Ticks.ToString();
                string extraData = "";

                string rawHash = $"accessKey={accessKey}&amount={totalAmount}&extraData={extraData}&ipnUrl={ipnUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={partnerCode}&redirectUrl={redirectUrl}&requestId={requestId}&requestType=captureWallet";
                string signature = ComputeHmacSha256(rawHash, secretKey);

                var requestData = new
                {
                    partnerCode,
                    requestId,
                    ipnUrl,
                    redirectUrl,
                    orderId,
                    orderInfo,
                    amount = totalAmount.ToString(),
                    extraData,
                    requestType = "captureWallet",
                    signature,
                    lang = "vi"
                };

                using (var client = new HttpClient())
                {
                    var response = await client.PostAsync(endpoint, new StringContent(JsonConvert.SerializeObject(requestData), Encoding.UTF8, "application/json"));
                    var responseContent = await response.Content.ReadAsStringAsync();
                    dynamic result = JsonConvert.DeserializeObject(responseContent);

                    if (result.payUrl != null) return Redirect(result.payUrl.ToString());

                    TempData["Error"] = "Lỗi MoMo: " + result.message;
                    return RedirectToAction("Index");
                }
            }

            // --- THANH TOÁN COD ---
            // Thêm logic lưu DonHang vào Database của Nguyên tại đây
            TempData["SuccessMessage"] = "Đặt hàng thành công (COD)!";
            HttpContext.Session.SetInt32("CartCount", 0);
            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Success()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                var items = _context.ChiTietGioHangs.Where(ct => ct.MaGhNavigation.MaNd == userId);
                _context.ChiTietGioHangs.RemoveRange(items);
                await _context.SaveChangesAsync();
                HttpContext.Session.SetInt32("CartCount", 0);
            }
            TempData["SuccessMessage"] = "Thanh toán MoMo thành công!";
            return RedirectToAction("Index", "Home");
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}