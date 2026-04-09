using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineBookStore_Web.Models
{
    [Table("DanhGia")]
    public partial class DanhGia
    {
        [Key]
        public int MaDg { get; set; }

        public int MaSach { get; set; }

        public int MaNd { get; set; }

        [Required]
        [Range(1, 5)]
        public int SoSao { get; set; }

        [StringLength(1000)]
        public string? NoiDung { get; set; }

        public DateTime NgayDanhGia { get; set; } = DateTime.Now;

        // Mối quan hệ với bảng Sach
        [ForeignKey("MaSach")]
        public virtual Sach? MaSachNavigation { get; set; }

        // Mối quan hệ với bảng NguoiDung
        [ForeignKey("MaNd")]
        public virtual NguoiDung? MaNdNavigation { get; set; }
    }
}