using System;
using System.Collections.Generic;

namespace OnlineBookStore_Web.Models
{
    public partial class KhuyenMai
    {
        public int MaKm { get; set; }
        public string TenKm { get; set; } = null!;
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public decimal GiaTriGiam { get; set; }
        public string? LoaiApDung { get; set; }
    }
}
