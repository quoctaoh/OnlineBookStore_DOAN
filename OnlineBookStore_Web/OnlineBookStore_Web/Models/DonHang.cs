using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace OnlineBookStore_Web.Models
{
    public partial class DonHang
    {
        public DonHang()
        {
            ChiTietDonHangs = new HashSet<ChiTietDonHang>();
        }

        [DisplayName("Mã Đơn Hàng")]
        public int MaDh { get; set; }

        [DisplayName("Mã Người Dùng")]
        public int MaNd { get; set; }

        [DisplayName("Ngày Đặt Hàng")]
        public DateTime NgayDatHang { get; set; }

        [DisplayName("Tổng Tiền")]
        public decimal TongTien { get; set; }

        [DisplayName("Trạng Thái Đơn Hàng")]
        public string TrangThaiDh { get; set; } = null!;

        [DisplayName("Địa Chỉ Giao Hàng")]
        public string DiaChiGiaoHang { get; set; } = null!;
        public virtual NguoiDung MaNdNavigation { get; set; } = null!;
        public virtual ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; }
    }
}
