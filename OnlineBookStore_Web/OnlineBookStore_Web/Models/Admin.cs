using System;
using System.Collections.Generic;

namespace OnlineBookStore_Web.Models
{
    public partial class Admin
    {
        public int MaAdmin { get; set; }
        public string TenDangNhap { get; set; } = null!;
        public string MatKhau { get; set; } = null!;
        public string HoTen { get; set; } = null!;
    }
}
