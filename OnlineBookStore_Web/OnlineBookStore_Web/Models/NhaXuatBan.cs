using System;
using System.Collections.Generic;

namespace OnlineBookStore_Web.Models
{
    public partial class NhaXuatBan
    {
        public NhaXuatBan()
        {
            Saches = new HashSet<Sach>();
        }

        public int MaNxb { get; set; }
        public string TenNxb { get; set; } = null!;
        public string? DiaChi { get; set; }
        public string? DienThoai { get; set; }

        public virtual ICollection<Sach> Saches { get; set; }
    }
}
