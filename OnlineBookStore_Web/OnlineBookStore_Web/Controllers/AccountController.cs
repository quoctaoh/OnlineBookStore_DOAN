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

        // CHỨC NĂNG ĐĂNG KÝ (REGISTER)
        // GET: /Account/Register
        public IActionResult Register()
        {
            return View();
        }

        // Action POST: Xử lý dữ liệu Đăng ký
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(NguoiDung model)
        {
            ModelState.Remove("MaNd");
            ModelState.Remove("MaXacThuc"); // Bỏ qua validate trường này

            if (ModelState.IsValid)
            {
                if (await _context.NguoiDungs.AnyAsync(u => u.TenDangNhap == model.TenDangNhap))
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }

                // Mã hóa mật khẩu
                model.MatKhau = HashPassword(model.MatKhau);

                // TẠO MÃ OTP (6 số ngẫu nhiên)
                Random generator = new Random();
                string otp = generator.Next(0, 1000000).ToString("D6");
                model.MaXacThuc = otp; // Lưu OTP vào user

                // Lưu NguoiDung (kèm OTP)
                _context.Add(model);
                await _context.SaveChangesAsync();

                // Tạo Giỏ hàng
                var newCart = new GioHang { MaNd = model.MaNd, NgayTao = DateTime.Now };
                _context.Add(newCart);
                await _context.SaveChangesAsync();

                // GỬI EMAIL
                try
                {
                    GuiEmailOTP(model.Email, otp);
                }
                catch (Exception ex)
                {
                    // Nếu gửi mail lỗi, có thể xóa user vừa tạo hoặc thông báo lỗi
                    ModelState.AddModelError("", "Không thể gửi email xác thực. Vui lòng kiểm tra lại email.");
                    return View(model);
                }

                // Chuyển hướng đến trang nhập OTP (truyền theo email)
                return RedirectToAction("VerifyOtp", new { email = model.Email });
            }
            return View(model);
        }
        // GET: Hiển thị form nhập OTP
        public IActionResult VerifyOtp(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        // POST: Xử lý xác thực
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyOtp(string email, string otpInput)
        {
            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);

            if (user == null)
            {
                TempData["Error"] = "Tài khoản không tồn tại.";
                return RedirectToAction("Register");
            }

            // Kiểm tra OTP
            if (user.MaXacThuc == otpInput)
            {
                // OTP Đúng -> Xóa mã xác thực (đánh dấu là đã verified)
                user.MaXacThuc = null;
                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Xác thực tài khoản thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            else
            {
                // OTP Sai
                ViewBag.Email = email;
                ViewBag.Error = "Mã OTP không chính xác. Vui lòng thử lại.";
                return View();
            }
        }

        // HÀM HỖ TRỢ: Mã hóa Mật khẩu
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hashedBytes).Replace("-", "").ToLower();
            }
        }

        // CHỨC NĂNG ĐĂNG NHẬP (Login)
        // GET: /Account/Login
        public IActionResult Login()
        {
            return View();
        }
        // Action POST: Xử lý dữ liệu Đăng nhập
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(NguoiDung model)
        {
            if (string.IsNullOrEmpty(model.TenDangNhap) || string.IsNullOrEmpty(model.MatKhau))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu.");
                return View(model);
            }

            // Mã hóa mật khẩu người dùng nhập vào
            string hashedPassword = HashPassword(model.MatKhau);

            // Tìm người dùng theo Tên đăng nhập VÀ Mật khẩu đã mã hóa
            var user = await _context.NguoiDungs
                .FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap && u.MatKhau == hashedPassword);

            if (user != null)
            {
                // Kiểm tra xem tài khoản đã xác thực OTP chưa?
                if (user.MaXacThuc != null)
                {
                    ModelState.AddModelError(string.Empty, "Tài khoản chưa được xác thực. Vui lòng kiểm tra email lấy mã OTP.");
                    return RedirectToAction("VerifyOtp", new { email = user.Email });
                    return View(model); 
                }
                // Đăng nhập thành công
                HttpContext.Session.SetInt32("UserId", user.MaNd);
                HttpContext.Session.SetString("UserName", user.HoTen);
                // Chuyển hướng về trang chủ
                return RedirectToAction("Index", "Home");
            }

            // Đăng nhập thất bại
            ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");

            return View(model);
        }


        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            // Chuyển hướng về trang chủ
            return RedirectToAction("Index", "Home");
        }
        // CHỨC NĂNG QUÊN MẬT KHẨU & ĐẶT LẠI MẬT KHẨU
        // Hiển thị trang nhập Email
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // Xử lý tìm Email và Gửi OTP
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ViewBag.Error = "Vui lòng nhập email.";
                return View();
            }

            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Email này chưa được đăng ký trong hệ thống.";
                return View();
            }

            Random generator = new Random();
            string otp = generator.Next(0, 1000000).ToString("D6");

            user.MaXacThuc = otp;
            _context.Update(user);
            await _context.SaveChangesAsync();

            try
            {
                GuiEmailOTP(user.Email, otp);
            }
            catch (Exception)
            {
                ViewBag.Error = "Lỗi gửi email. Vui lòng thử lại sau.";
                return View();
            }

            return RedirectToAction("ResetPassword", new { email = email });
        }

        // Hiển thị trang Đặt lại mật khẩu
        public IActionResult ResetPassword(string email)
        {
            // Truyền email sang View để người dùng không phải nhập lại
            ViewBag.Email = email;
            return View();
        }

        // POST: Xử lý Đổi mật khẩu mới
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(string email, string otp, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp.";
                ViewBag.Email = email;
                return View();
            }

            var user = await _context.NguoiDungs.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                ViewBag.Error = "Đã có lỗi xảy ra. Không tìm thấy tài khoản.";
                return View();
            }

            // Kiểm tra OTP
            if (user.MaXacThuc != otp)
            {
                ViewBag.Error = "Mã OTP không chính xác hoặc đã hết hạn.";
                ViewBag.Email = email;
                return View();
            }
            user.MatKhau = HashPassword(newPassword);
            // Xóa OTP sau khi dùng xong
            user.MaXacThuc = null;
            _context.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công. Vui lòng đăng nhập lại.";
            return RedirectToAction("Login");
        }

        // GET: /Account/OrderHistory
        public async Task<IActionResult> OrderHistory()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
            {
                // Nếu chưa đăng nhập, chuyển hướng đến trang Đăng nhập
                TempData["Message"] = "Vui lòng đăng nhập để xem lịch sử đơn hàng.";
                return RedirectToAction("Login");
            }

            // Lấy tất cả đơn hàng của người dùng hiện tại
            // Sắp xếp theo ngày đặt hàng mới nhất
            var orders = await _context.DonHangs
                .Where(o => o.MaNd == userId.Value)
                .OrderByDescending(o => o.NgayDatHang)
                .Include(o => o.ChiTietDonHangs)

                .ToListAsync();

            return View(orders);
        }
        // Action POST: /Account/LoginAdmin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginAdmin(Admin model)
        {
            ModelState.Remove("HoTen");
            ModelState.Remove("MaAdmin");
            if (ModelState.IsValid)
            {
                // Mã hóa mật khẩu người dùng nhập vào
                string hashedPassword = HashPassword(model.MatKhau);
                
                // Tìm Admin theo Tên đăng nhập VÀ Mật khẩu đã mã hóa
                var adminUser = await _context.Admins
                    .FirstOrDefaultAsync(u => u.TenDangNhap == model.TenDangNhap && u.MatKhau == hashedPassword);

                if (adminUser != null)
                {
                    // Đăng nhập Admin thành công
                    // Thiết lập Session Admin
                    HttpContext.Session.SetInt32("AdminId", adminUser.MaAdmin);
                    HttpContext.Session.SetString("AdminName", adminUser.HoTen);

                    // Chuyển hướng đến trang Admin Dashboard
                    return RedirectToAction("Index", "Admin");
                }

                // Đăng nhập thất bại
                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu quản trị không đúng.");
            }
            return View(model);
        }

        // GET: /Account/LoginAdmin
        public IActionResult LoginAdmin()
        {
            if (HttpContext.Session.GetString("AdminName") != null)
            {
                return RedirectToAction("Index", "Admin");
            }
            return View();
        }

        // GET: /Account/LogoutAdmin
        public IActionResult LogoutAdmin()
        {
            HttpContext.Session.Remove("AdminId");
            HttpContext.Session.Remove("AdminName");
            return RedirectToAction("Index", "Home");
        }
        // Hàm gửi mail
        private void GuiEmailOTP(string emailNhan, string otp)
        {
            var fromAddress = new MailAddress("quoctaoh@gmail.com", "Online Bookstore");
            var toAddress = new MailAddress(emailNhan);
            const string fromPassword = "afdc ttqf xuud wxfd"; 
            const string subject = "Mã xác thực OTP - Online Bookstore";
            string body = $"Mã xác thực OTP của bạn là: <strong>{otp}</strong>";

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
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            })
            {
                smtp.Send(message);
            }
        }
    }
}