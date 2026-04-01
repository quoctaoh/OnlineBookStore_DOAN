using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace OnlineBookStore_Web.Models
{
    public partial class Sach
    {
        public Sach()
        {
            ChiTietDonHangs = new HashSet<ChiTietDonHang>();
            ChiTietGioHangs = new HashSet<ChiTietGioHang>();
        }

        [DisplayName("Mã Sách")]
        public int MaSach { get; set; }

        [DisplayName("Tên Sách")]
        public string TenSach { get; set; } = null!;

        [DisplayName("Tác Giả")]
        public string TacGia { get; set; } = null!;

        [DisplayName("Mã Thể Loại")]
        public int MaTl { get; set; }

        [DisplayName("Mã Nhà Xuất Bản")]
        public int MaNxb { get; set; }

        [DisplayName("Giá Bán")]
        public decimal GiaBan { get; set; }

        [DisplayName("Số Lượng Tồn")]
        public int SoLuongTon { get; set; }

        [DisplayName("Mô Tả")]
        public string? MoTa { get; set; }

        [DisplayName("Hình Ảnh")]
        public string? HinhAnh { get; set; }

        public virtual NhaXuatBan MaNxbNavigation { get; set; } = null!;
        public virtual TheLoai MaTlNavigation { get; set; } = null!;
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; }
    }
}
