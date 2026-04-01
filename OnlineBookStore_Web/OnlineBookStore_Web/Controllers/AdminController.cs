using Microsoft.AspNetCore.Mvc;
using OnlineBookStore_Web.Models;
using Microsoft.EntityFrameworkCore;
using OnlineBookStore_Web.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace OnlineBookStore_Web.Controllers
{
    // Áp dụng bộ lọc cho toàn bộ Controller để bảo vệ các Action
    [AdminAuthorization]
    public class AdminController : Controller
    {
        private readonly OnlineBookstore_DOANContext _context;

        public AdminController(OnlineBookstore_DOANContext context)
        {
            _context = context;
        }

        // GET: /Admin/Index (Trang chủ Admin Dashboard)
        public IActionResult Index()
        {
            // Lấy tên Admin từ Session để hiển thị
            ViewData["AdminName"] = HttpContext.Session.GetString("AdminName");
            return View();
        }

        // QUẢN LÝ SÁCH (CRUD)
        // GET: /Admin/ListBooks - Hiển thị danh sách tất cả sách
        public async Task<IActionResult> ListBooks()
        {
            var sachs = await _context.Saches
                .Include(s => s.MaTlNavigation)
                .Include(s => s.MaNxbNavigation)
                .ToListAsync();

            return View(sachs);
        }

        // GET: /Admin/CreateBook (Hiển thị form thêm mới)
        public IActionResult CreateBook()
        {
            ViewBag.MaTl = new SelectList(_context.TheLoais, "MaTl", "TenTheLoai");
            ViewBag.MaNxb = new SelectList(_context.NhaXuatBans, "MaNxb", "TenNxb");
            return View();
        }

        // POST: /Admin/CreateBook (Xử lý lưu sách mới)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBook(
        [Bind("TenSach,TacGia,MaTl,MaNxb,GiaBan,SoLuongTon,MoTa")] Sach sach,
        IFormFile imageFile)
        {
            ModelState.Remove("MaSach");
            ModelState.Remove("HinhAnh");
            ModelState.Remove("MaTlNavigation");
            ModelState.Remove("MaNxbNavigation");

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    // 1. Định nghĩa đường dẫn lưu trữ file
                    string wwwRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                    string uploadsFolder = Path.Combine(wwwRootPath, "images", "bookcovers");

                    // 2. Tạo tên file duy nhất 
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string filePath = Path.Combine(uploadsFolder, fileName);

                    // 3. Đảm bảo thư mục tồn tại và lưu file vật lý
                    Directory.CreateDirectory(uploadsFolder);
                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(fileStream);
                    }

                    // 4. LƯU ĐƯỜNG DẪN TƯƠNG ĐỐI VÀO CSDL (giá trị của cột HinhAnh)
                    sach.HinhAnh = Path.Combine("/images/bookcovers", fileName).Replace("\\", "/");
                }
                else
                {
                    // Nếu Admin không tải ảnh lên, gán ảnh mặc định
                    sach.HinhAnh = "/images/default.png";
                }

                _context.Add(sach);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm sách mới thành công!";
                return RedirectToAction(nameof(ListBooks));
            }

            // Nếu lỗi, load lại ViewBag
            ViewBag.MaTl = new SelectList(_context.TheLoais, "MaTl", "TenTheLoai", sach.MaTl);
            ViewBag.MaNxb = new SelectList(_context.NhaXuatBans, "MaNxb", "TenNxb", sach.MaNxb);
            return View(sach);
        }

        // GET: /Admin/EditBook/5 (Hiển thị form sửa)
        public async Task<IActionResult> EditBook(int? id)
        {
            if (id == null) return NotFound();

            var sach = await _context.Saches.FindAsync(id);
            if (sach == null) return NotFound();

            ViewBag.MaTl = new SelectList(_context.TheLoais, "MaTl", "TenTheLoai", sach.MaTl);
            ViewBag.MaNxb = new SelectList(_context.NhaXuatBans, "MaNxb", "TenNxb", sach.MaNxb);

            return View(sach);
        }

        // POST: /Admin/EditBook/5 (Xử lý lưu thay đổi)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBook(int MaSach, [Bind("MaSach,TenSach,TacGia,MaTl,MaNxb,GiaBan,SoLuongTon,MoTa,HinhAnh")] Sach sach)
        {
            if (MaSach != sach.MaSach) return NotFound();

            // BỎ QUA KIỂM TRA VALIDATION
            ModelState.Remove("HinhAnh");
            ModelState.Remove("MaTlNavigation");
            ModelState.Remove("MaNxbNavigation");
            ModelState.Remove("MaSach");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(sach);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật sách thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Saches.Any(e => e.MaSach == MaSach))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(ListBooks));
            }

            ViewBag.MaTl = new SelectList(_context.TheLoais, "MaTl", "TenTheLoai", sach.MaTl);
            ViewBag.MaNxb = new SelectList(_context.NhaXuatBans, "MaNxb", "TenNxb", sach.MaNxb);
            return View(sach);
        }

        // Action EditProfile
        //CẬP NHẬT THÔNG TIN CÁ NHÂN ADMIN
        // GET: /Admin/EditProfile (Hiển thị form cập nhật thông tin)
        public async Task<IActionResult> EditProfile()
        {
            var adminId = HttpContext.Session.GetInt32("AdminId");

            if (!adminId.HasValue)
            {
                return RedirectToAction("LoginAdmin", "Account");
            }

            var admin = await _context.Admins.FindAsync(adminId.Value);

            if (admin == null)
            {
                return NotFound();
            }
            return View(admin);
        }

        // POST: /Admin/EditProfile (Xử lý lưu thông tin mới)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(int MaAdmin, [Bind("MaAdmin, HoTen")] Admin updatedAdmin)
        {
            if (MaAdmin != updatedAdmin.MaAdmin)
            {
                return NotFound();
            }

            ModelState.Remove("MatKhau");
            ModelState.Remove("TenDangNhap");

            if (ModelState.IsValid)
            {
                try
                {
                    // 1. Lấy Admin hiện có để giữ MatKhau và TenDangNhap cũ
                    var existingAdmin = await _context.Admins.AsNoTracking().FirstOrDefaultAsync(a => a.MaAdmin == MaAdmin);

                    if (existingAdmin == null) return NotFound();

                    // 2. Cập nhật duy nhất trường HoTen (Tên hiển thị)
                    existingAdmin.HoTen = updatedAdmin.HoTen;

                    // 3. Update CSDL và lưu
                    _context.Update(existingAdmin);
                    await _context.SaveChangesAsync();

                    // 4. Cập nhật Session
                    HttpContext.Session.SetString("AdminName", existingAdmin.HoTen);

                    TempData["SuccessMessage"] = "Cập nhật tên hiển thị Admin thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Admins.Any(e => e.MaAdmin == MaAdmin))
                    {
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(updatedAdmin);
        }
        // GET: /Admin/DeleteBook/5 (Hiển thị xác nhận xóa)
        public async Task<IActionResult> DeleteBook(int? id)
        {
            if (id == null) return NotFound();

            var sach = await _context.Saches
                .Include(s => s.MaTlNavigation)
                .Include(s => s.MaNxbNavigation)
                .FirstOrDefaultAsync(m => m.MaSach == id);

            if (sach == null) return NotFound();

            return View(sach);
        }

        // POST: /Admin/DeleteBook/5 (Thực hiện xóa)
        [HttpPost, ActionName("DeleteBook")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var sach = await _context.Saches.FindAsync(id);
            if (sach != null)
            {
                _context.Saches.Remove(sach);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa sách thành công.";
            }
            return RedirectToAction(nameof(ListBooks));
        }

        // QUẢN LÝ ĐƠN HÀNG VÀ BÁO CÁO
        // GET: /Admin/ListOrders - Hiển thị danh sách tất cả đơn hàng
        public async Task<IActionResult> ListOrders()
        {
            var orders = await _context.DonHangs
                .Include(o => o.MaNdNavigation) // Lấy thông tin người đặt hàng
                .OrderByDescending(o => o.NgayDatHang)
                .ToListAsync();

            return View(orders);
        }

        // GET: /Admin/EditStatus/1001 (Hiển thị form cập nhật)
        public async Task<IActionResult> EditStatus(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.DonHangs.FindAsync(id);
            if (order == null) return NotFound();

            // Dùng ViewBag để truyền danh sách trạng thái cố định
            ViewBag.StatusList = new List<string> {
                "Chờ xác nhận",
                "Đang xử lý",
                "Đang giao hàng",
                "Đã giao thành công",
                "Đã hủy"
            };

            return View(order);
        }

        // POST: /Admin/EditStatus/1001 (Xử lý lưu trạng thái mới)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditStatus(int MaDh, [Bind("MaDh,TrangThaiDh")] DonHang updatedOrder)
        {
            if (MaDh != updatedOrder.MaDh) return NotFound();

            var order = await _context.DonHangs.FindAsync(MaDh);
            if (order == null) return NotFound();

            // Cập nhật duy nhất trường TrangThaiDh
            order.TrangThaiDh = updatedOrder.TrangThaiDh;

            try
            {
                _context.Update(order);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Cập nhật trạng thái đơn hàng #{MaDh} thành công!";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.DonHangs.Any(e => e.MaDh == MaDh)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(ListOrders));
        }

        // GET: /Admin/OrderDetails/1001 (Xem chi tiết sản phẩm trong đơn hàng)
        public async Task<IActionResult> OrderDetails(int id)
        {
            // Lấy thông tin đơn hàng và thông tin khách hàng
            var order = await _context.DonHangs
                .Include(o => o.MaNdNavigation)
                .FirstOrDefaultAsync(o => o.MaDh == id);

            if (order == null) return NotFound();

            // Lấy chi tiết sản phẩm của đơn hàng 
            var orderDetails = await _context.ChiTietDonHangs
                .Where(ct => ct.MaDh == id)
                .Include(ct => ct.MaSachNavigation)
                .ToListAsync();
            ViewBag.Order = order;
            return View(orderDetails); 
        }

        // GET: /Admin/SalesSummary
        public async Task<IActionResult> SalesSummary()
        {
            // 1. Chỉ tính toán trên các đơn hàng đã HOÀN THÀNH (Đã giao thành công)
            var completedOrders = await _context.DonHangs
                .Where(o => o.TrangThaiDh == "Đã giao thành công")
                // Cần include MaNdNavigation để lấy tên khách hàng trong View
                .Include(o => o.MaNdNavigation)
                .ToListAsync();

            // 2. Tính toán tổng doanh thu và tổng số đơn hàng
            var totalRevenue = completedOrders.Sum(o => o.TongTien);
            var totalOrders = completedOrders.Count();

            // 3. Truyền dữ liệu sang View
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.CompletedOrders = completedOrders; // Truyền chi tiết đơn hàng hoàn thành

            return View();
        }
        // Trong AdminController.cs

        // ---------------------------------------------------
        // API HỖ TRỢ AJAX: THÊM NHANH THỂ LOẠI & NXB
        // ---------------------------------------------------

        [HttpPost]
        public async Task<IActionResult> QuickCreateGenre(string tenTheLoai)
        {
            if (string.IsNullOrWhiteSpace(tenTheLoai))
            {
                return Json(new { success = false, message = "Tên thể loại không được để trống." });
            }

            // Kiểm tra trùng lặp
            if (await _context.TheLoais.AnyAsync(t => t.TenTheLoai == tenTheLoai))
            {
                return Json(new { success = false, message = "Thể loại này đã tồn tại." });
            }

            var newTheLoai = new TheLoai { TenTheLoai = tenTheLoai };
            _context.TheLoais.Add(newTheLoai);
            await _context.SaveChangesAsync();

            // Trả về ID và Tên để cập nhật giao diện
            return Json(new { success = true, id = newTheLoai.MaTl, name = newTheLoai.TenTheLoai });
        }

        [HttpPost]
        public async Task<IActionResult> QuickCreatePublisher(string tenNxb)
        {
            if (string.IsNullOrWhiteSpace(tenNxb))
            {
                return Json(new { success = false, message = "Tên NXB không được để trống." });
            }

            if (await _context.NhaXuatBans.AnyAsync(n => n.TenNxb == tenNxb))
            {
                return Json(new { success = false, message = "Nhà xuất bản này đã tồn tại." });
            }

            var newNxb = new NhaXuatBan { TenNxb = tenNxb };
            _context.NhaXuatBans.Add(newNxb);
            await _context.SaveChangesAsync();

            return Json(new { success = true, id = newNxb.MaNxb, name = newNxb.TenNxb });
        }
    }
}