using System;
using System.Collections.Generic;

namespace OnlineBookStore_Web.Models
{
    public partial class TheLoai
    {
        public TheLoai()
        {
            Saches = new HashSet<Sach>();
        }

        public int MaTl { get; set; }
        public string TenTheLoai { get; set; } = null!;

        public virtual ICollection<Sach> Saches { get; set; }
    }
}
