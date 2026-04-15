using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace OnlineBookStore_Web.Models
{
    public partial class OnlineBookstore_DOANContext : DbContext
    {
        public OnlineBookstore_DOANContext()
        {
        }

        public OnlineBookstore_DOANContext(DbContextOptions<OnlineBookstore_DOANContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Admin> Admins { get; set; } = null!;
        public virtual DbSet<ChiTietDonHang> ChiTietDonHangs { get; set; } = null!;
        public virtual DbSet<ChiTietGioHang> ChiTietGioHangs { get; set; } = null!;
        public virtual DbSet<DonHang> DonHangs { get; set; } = null!;
        public virtual DbSet<GioHang> GioHangs { get; set; } = null!;
        public virtual DbSet<KhuyenMai> KhuyenMais { get; set; } = null!;
        public virtual DbSet<NguoiDung> NguoiDungs { get; set; } = null!;
        public virtual DbSet<NhaXuatBan> NhaXuatBans { get; set; } = null!;
        public virtual DbSet<Sach> Saches { get; set; } = null!;
        public virtual DbSet<TheLoai> TheLoais { get; set; } = null!;
        // Thêm DbSet cho DanhGia
        public virtual DbSet<DanhGia> DanhGias { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Admin>(entity =>
            {
                entity.HasKey(e => e.MaAdmin)
                    .HasName("PK__Admin__49341E38035B33DB");

                entity.ToTable("Admin");

                entity.HasIndex(e => e.TenDangNhap, "UQ__Admin__55F68FC059D0AB12")
                    .IsUnique();

                entity.Property(e => e.HoTen).HasMaxLength(100);

                entity.Property(e => e.MatKhau)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.TenDangNhap)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            // Cấu hình Fluent API cho bảng DanhGia
            modelBuilder.Entity<DanhGia>(entity =>
            {
                entity.HasKey(e => e.MaDg);

                entity.ToTable("DanhGia");

                entity.Property(e => e.MaDg).HasColumnName("MaDG");
                entity.Property(e => e.MaSach).HasColumnName("MaSach");
                entity.Property(e => e.MaNd).HasColumnName("MaND");
                entity.Property(e => e.NgayDanhGia).HasColumnType("datetime").HasDefaultValueSql("(getdate())");
                entity.Property(e => e.NoiDung).HasMaxLength(1000);

                entity.HasOne(d => d.MaSachNavigation)
                    .WithMany(p => p.DanhGias)
                    .HasForeignKey(d => d.MaSach)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DanhGia_Sach");

                entity.HasOne(d => d.MaNdNavigation)
                    .WithMany(p => p.DanhGias)
                    .HasForeignKey(d => d.MaNd)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_DanhGia_NguoiDung");
            });

            modelBuilder.Entity<ChiTietDonHang>(entity =>
            {
                entity.HasKey(e => new { e.MaDh, e.MaSach })
                    .HasName("PK__ChiTietD__EC06D1234F324EA6");

                entity.ToTable("ChiTietDonHang");

                entity.Property(e => e.MaDh).HasColumnName("MaDH");

                entity.Property(e => e.DonGia).HasColumnType("decimal(10, 2)");

                entity.HasOne(d => d.MaDhNavigation)
                    .WithMany(p => p.ChiTietDonHangs)
                    .HasForeignKey(d => d.MaDh)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietDon__MaDH__5070F446");

                entity.HasOne(d => d.MaSachNavigation)
                    .WithMany(p => p.ChiTietDonHangs)
                    .HasForeignKey(d => d.MaSach)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietDo__MaSac__5165187F");
            });

            modelBuilder.Entity<ChiTietGioHang>(entity =>
            {
                entity.HasKey(e => new { e.MaGh, e.MaSach })
                    .HasName("PK__ChiTietG__EC06F9C72318CD71");

                entity.ToTable("ChiTietGioHang");

                entity.Property(e => e.MaGh).HasColumnName("MaGH");

                entity.HasOne(d => d.MaGhNavigation)
                    .WithMany(p => p.ChiTietGioHangs)
                    .HasForeignKey(d => d.MaGh)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietGio__MaGH__59063A47");

                entity.HasOne(d => d.MaSachNavigation)
                    .WithMany(p => p.ChiTietGioHangs)
                    .HasForeignKey(d => d.MaSach)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietGi__MaSac__59FA5E80");
            });

            modelBuilder.Entity<DonHang>(entity =>
            {
                entity.HasKey(e => e.MaDh)
                    .HasName("PK__DonHang__27258661F09732D0");

                entity.ToTable("DonHang");

                entity.Property(e => e.MaDh).HasColumnName("MaDH");

                entity.Property(e => e.DiaChiGiaoHang).HasMaxLength(255);

                entity.Property(e => e.MaNd).HasColumnName("MaND");

                entity.Property(e => e.NgayDatHang).HasColumnType("datetime");

                entity.Property(e => e.TongTien).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.TrangThaiDh)
                    .HasMaxLength(50)
                    .HasColumnName("TrangThaiDH");

                entity.HasOne(d => d.MaNdNavigation)
                    .WithMany(p => p.DonHangs)
                    .HasForeignKey(d => d.MaNd)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__DonHang__MaND__4CA06362");
            });

            modelBuilder.Entity<GioHang>(entity =>
            {
                entity.HasKey(e => e.MaGh)
                    .HasName("PK__GioHang__2725AE85D57DE823");

                entity.ToTable("GioHang");

                entity.HasIndex(e => e.MaNd, "UQ__GioHang__2725D725C790624B")
                    .IsUnique();

                entity.Property(e => e.MaGh).HasColumnName("MaGH");

                entity.Property(e => e.MaNd).HasColumnName("MaND");

                entity.Property(e => e.NgayTao).HasColumnType("datetime");

                entity.HasOne(d => d.MaNdNavigation)
                    .WithOne(p => p.GioHang)
                    .HasForeignKey<GioHang>(d => d.MaNd)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__GioHang__MaND__5535A963");
            });

            modelBuilder.Entity<KhuyenMai>(entity =>
            {
                entity.HasKey(e => e.MaKm)
                    .HasName("PK__KhuyenMa__2725CF15F9387C15");

                entity.ToTable("KhuyenMai");

                entity.Property(e => e.MaKm).HasColumnName("MaKM");

                entity.Property(e => e.GiaTriGiam).HasColumnType("decimal(5, 2)");

                entity.Property(e => e.LoaiApDung).HasMaxLength(50);

                entity.Property(e => e.NgayBatDau).HasColumnType("date");

                entity.Property(e => e.NgayKetThuc).HasColumnType("date");

                entity.Property(e => e.TenKm)
                    .HasMaxLength(255)
                    .HasColumnName("TenKM");
            });

            modelBuilder.Entity<NguoiDung>(entity =>
            {
                entity.HasKey(e => e.MaNd)
                    .HasName("PK__NguoiDun__2725D724710C6BD0");

                entity.ToTable("NguoiDung");

                entity.HasIndex(e => e.TenDangNhap, "UQ__NguoiDun__55F68FC010ED0780")
                    .IsUnique();

                entity.HasIndex(e => e.Email, "UQ__NguoiDun__A9D10534BC7A2DC4")
                    .IsUnique();

                entity.Property(e => e.MaNd).HasColumnName("MaND");

                entity.Property(e => e.DiaChi).HasMaxLength(255);

                entity.Property(e => e.DienThoai)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.Email)
                    .HasMaxLength(100)
                    .IsUnicode(false);

                entity.Property(e => e.HoTen).HasMaxLength(100);

                entity.Property(e => e.MatKhau)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.TenDangNhap)
                    .HasMaxLength(50)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<NhaXuatBan>(entity =>
            {
                entity.HasKey(e => e.MaNxb)
                    .HasName("PK__NhaXuatB__3A19482C842F8556");

                entity.ToTable("NhaXuatBan");

                entity.HasIndex(e => e.TenNxb, "UQ__NhaXuatB__CCE3868DEA18C7E4")
                    .IsUnique();

                entity.Property(e => e.MaNxb).HasColumnName("MaNXB");

                entity.Property(e => e.DiaChi).HasMaxLength(255);

                entity.Property(e => e.DienThoai)
                    .HasMaxLength(15)
                    .IsUnicode(false);

                entity.Property(e => e.TenNxb)
                    .HasMaxLength(150)
                    .HasColumnName("TenNXB");
            });

            modelBuilder.Entity<Sach>(entity =>
            {
                entity.HasKey(e => e.MaSach)
                    .HasName("PK__Sach__B235742D68BFFB7B");

                entity.ToTable("Sach");

                entity.Property(e => e.GiaBan).HasColumnType("decimal(10, 2)");

                entity.Property(e => e.HinhAnh)
                    .HasMaxLength(255)
                    .IsUnicode(false);

                entity.Property(e => e.MaNxb).HasColumnName("MaNXB");

                entity.Property(e => e.MaTl).HasColumnName("MaTL");

                entity.Property(e => e.TacGia).HasMaxLength(100);

                entity.Property(e => e.TenSach).HasMaxLength(255);

                entity.HasOne(d => d.MaNxbNavigation)
                    .WithMany(p => p.Saches)
                    .HasForeignKey(d => d.MaNxb)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Sach__MaNXB__403A8C7D");

                entity.HasOne(d => d.MaTlNavigation)
                    .WithMany(p => p.Saches)
                    .HasForeignKey(d => d.MaTl)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__Sach__MaTL__3F466844");
            });

            modelBuilder.Entity<TheLoai>(entity =>
            {
                entity.HasKey(e => e.MaTl)
                    .HasName("PK__TheLoai__27250071D134ECB6");

                entity.ToTable("TheLoai");

                entity.HasIndex(e => e.TenTheLoai, "UQ__TheLoai__327F958FEA904F73")
                    .IsUnique();

                entity.Property(e => e.MaTl).HasColumnName("MaTL");

                entity.Property(e => e.TenTheLoai).HasMaxLength(100);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}