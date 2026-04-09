using Microsoft.AspNetCore.Mvc;
using OnlineBookStore_Web.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;

namespace OnlineBookStore_Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly OnlineBookstore_DOANContext _context;

        public AccountController(OnlineBookstore_DOANContext context)
        {
            _context = context;
        }

        // --- ĐĂNG KÝ (REGISTER) ---
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(NguoiDung model)
        {
            ModelState.Remove("MaNd");
            ModelState.Remove("MaXacThuc");

            if (ModelState.IsValid)
            {
                if (await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == model.TenDangNhap))
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                model.MatKhau = HashPassword(model.MatKhau);

                Random generator = new Random();
                string otp = generator.Next(0, 1000000).ToString("D6");
                model.MaXacThuc = otp;

                _context.Add(model);
                await _context.SaveChangesAsync();

                var newCart = new GioHang { MaNd = model.MaNd, NgayTao = DateTime.Now };
                _context.Add(newCart);
                await _context.SaveChangesAsync();

                try { GuiEmailOTP(model.Email, otp); }
                catch { }

                return RedirectToAction("VerifyOtp", new { email = model.Email });
            }
            return View(model);
        }

        // --- XÁC THỰC OTP ---
        public IActionResult VerifyOtp(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(string email, string otpInput)
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return RedirectToAction("Register");

            if (user.MaXacThuc == otpInput)
            {
                user.MaXacThuc = null;
                _context.Update(user);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xác thực thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            ViewBag.Email = email;
            ViewBag.Error = "Mã OTP không chính xác.";
            return View();
        }

        // --- ĐĂNG NHẬP (LOGIN) ---
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(NguoiDung model)
        {
            if (string.IsNullOrEmpty(model.TenDangNhap) || string.IsNullOrEmpty(model.MatKhau))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ thông tin.");
                return View(model);
            }

            string hashedPassword = HashPassword(model.MatKhau);
            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap && u.MatKhau == hashedPassword);

            if (user != null)
            {
                if (user.MaXacThuc != null)
                {
                    return RedirectToAction("VerifyOtp", new { email = user.Email });
                }

                // Thiết lập Session
                HttpContext.Session.SetInt32("UserId", user.MaNd);
                HttpContext.Session.SetString("UserName", user.HoTen);

                // CẬP NHẬT SỐ LƯỢNG GIỎ HÀNG LÊN NAVBAR NGAY KHI ĐĂNG NHẬP
                var cart = await _context.GioHangs.FirstOrDefaultAsync(g => g.MaNd == user.MaNd);
                if (cart != null)
                {
                    var count = await _context.ChiTietGioHangs
                        .Where(ct => ct.MaGh == cart.MaGh)
                        .SumAsync(ct => ct.SoLuong);
                    HttpContext.Session.SetInt32("CartCount", count);
                }

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
            return View(model);
        }

        // --- ĐĂNG XUẤT (LOGOUT) ---
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        // --- QUÊN MẬT KHẨU ---
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) { ViewBag.Error = "Email chưa đăng ký."; return View(); }

            string otp = new Random().Next(0, 1000000).ToString("D6");
            user.MaXacThuc = otp;
            _context.Update(user);
            await _context.SaveChangesAsync();

            try { GuiEmailOTP(user.Email, otp); } catch { }
            return RedirectToAction("ResetPassword", new { email = email });
        }

        public IActionResult ResetPassword(string email) { ViewBag.Email = email; return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string email, string otp, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword) { ViewBag.Error = "Mật khẩu không khớp."; ViewBag.Email = email; return View(); }
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
            if (user != null && user.MaXacThuc == otp)
            {
                user.MatKhau = HashPassword(newPassword);
                user.MaXacThuc = null;
                _context.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction("Login");
            }
            ViewBag.Error = "OTP không đúng."; return View();
        }

        // --- ADMIN LOGIN ---
        public IActionResult LoginAdmin() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAdmin(Admin model)
        {
            ModelState.Remove("HoTen");
            ModelState.Remove("MaAdmin");
            if (ModelState.IsValid)
            {
                string hashedPassword = HashPassword(model.MatKhau);
                var adminUser = await _context.Admins
                    .FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap && u.MatKhau == hashedPassword);

                if (adminUser != null)
                {
                    HttpContext.Session.SetInt32("AdminId", adminUser.MaAdmin);
                    HttpContext.Session.SetString("AdminName", adminUser.HoTen);
                    return RedirectToAction("Index", "Admin");
                }
                ModelState.AddModelError(string.Empty, "Tài khoản Admin sai.");
            }
            return View(model);
        }

        public IActionResult LogoutAdmin()
        {
            HttpContext.Session.Remove("AdminId");
            HttpContext.Session.Remove("AdminName");
            return RedirectToAction("Index", "Home");
        }

        // --- HELPERS ---
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        private void GuiEmailOTP(string emailNhan, string otp)
        {
            var fromAddress = new MailAddress("quoctaoh@gmail.com", "Online Bookstore");
            var toAddress = new MailAddress(emailNhan);
            const string fromPassword = "afdc ttqf xuud wxfd";
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = "Ma OTP - Online Bookstore",
                Body = $"OTP: {otp}",
                IsBodyHtml = true
            }) smtp.Send(message);
        }
    }
}