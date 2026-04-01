using System;
using System.Collections.Generic;

namespace OnlineBookStore_Web.Models
{
    public partial class GioHang
    {
        public GioHang()
        {
            ChiTietGioHangs = new HashSet<ChiTietGioHang>();
        }

        public int MaGh { get; set; }
        public int MaNd { get; set; }
        public DateTime NgayTao { get; set; }

        public virtual NguoiDung MaNdNavigation { get; set; } = null!;
        public virtual ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; }
    }
}
