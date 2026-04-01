using System;
using System.Collections.Generic;

namespace OnlineBookStore_Web.Models
{
    public partial class ChiTietDonHang
    {
        public int MaDh { get; set; }
        public int MaSach { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }

        public virtual DonHang MaDhNavigation { get; set; } = null!;
        public virtual Sach MaSachNavigation { get; set; } = null!;
    }
}
