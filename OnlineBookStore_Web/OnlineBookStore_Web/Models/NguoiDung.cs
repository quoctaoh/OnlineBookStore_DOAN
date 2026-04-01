using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace OnlineBookStore_Web.Models
{
    public partial class NguoiDung
    {
        public NguoiDung()
        {
            DonHangs = new HashSet<DonHang>();
        }

        [DisplayName("Mã Người Dùng")]
        public int MaNd { get; set; }

        [DisplayName("Tên Đăng Nhập")]
        public string TenDangNhap { get; set; } = null!;

        [DisplayName("Mật Khẩu")]
        public string MatKhau { get; set; } = null!;

        [DisplayName("Họ Tên")]
        public string HoTen { get; set; } = null!;

        public string? Email { get; set; }

        [DisplayName("Số Điện Thoại")]
        public string? DienThoai { get; set; }

        [DisplayName("Địa Chỉ")]
        public string? DiaChi { get; set; }

        [DisplayName("Mã Xác Thực")]
        public string? MaXacThuc { get; set; }

        [DisplayName("Giỏ hàng")]
        public virtual GioHang? GioHang { get; set; }

        [DisplayName("Đơn hàng")]
        public virtual ICollection<DonHang> DonHangs { get; set; }
    }
}
