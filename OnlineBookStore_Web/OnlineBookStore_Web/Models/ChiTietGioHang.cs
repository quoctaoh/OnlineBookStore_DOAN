using System;
using System.Collections.Generic;

namespace OnlineBookStore_Web.Models
{
    public partial class ChiTietGioHang
    {
        public int MaGh { get; set; }
        public int MaSach { get; set; }
        public int SoLuong { get; set; }

        public virtual GioHang MaGhNavigation { get; set; } = null!;
        public virtual Sach MaSachNavigation { get; set; } = null!;
    }
}
