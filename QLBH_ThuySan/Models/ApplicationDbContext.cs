using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QLBH_ThuySan.Models
{
    public partial class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public virtual DbSet<BangKeChietKhau> BangKeChietKhaus { get; set; }
        public virtual DbSet<CauHinhChietKhau> CauHinhChietKhaus { get; set; }
        public virtual DbSet<ChiTietPhieuNhap> ChiTietPhieuNhaps { get; set; }
        public virtual DbSet<ChiTietPhieuXuat> ChiTietPhieuXuats { get; set; }
        public virtual DbSet<ChiTietTon> ChiTietTons { get; set; }
        public virtual DbSet<DaiLy> DaiLies { get; set; }
        public virtual DbSet<HangHoa> HangHoas { get; set; }
        public virtual DbSet<KhachHang> KhachHangs { get; set; }
        public virtual DbSet<Kho> Khos { get; set; }
        public virtual DbSet<NhaCungCap> NhaCungCaps { get; set; }
        public virtual DbSet<PhieuNhap> PhieuNhaps { get; set; }
        public virtual DbSet<PhieuThuChi> PhieuThuChis { get; set; }
        public virtual DbSet<PhieuTinhChietKhau> PhieuTinhChietKhaus { get; set; }
        public virtual DbSet<PhieuXuat> PhieuXuats { get; set; }
        public virtual DbSet<SoRiengKhachHang> SoRiengKhachHangs { get; set; }
        public virtual DbSet<NguoiDung> NguoiDungs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<BangKeChietKhau>(entity =>
            {
                entity.HasKey(e => e.MaBangKe).HasName("PK__BangKeCh__91E230ADB0EB5061");

                entity.ToTable("BangKeChietKhau");

                entity.Property(e => e.MaBangKe)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maBangKe");
                entity.Property(e => e.MaDoiTuong)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maDoiTuong");
                entity.Property(e => e.MaPhieuTinh)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maPhieuTinh");
                entity.Property(e => e.TongTien)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("tongTien");
                entity.Property(e => e.TrangThai)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasDefaultValue("Pending")
                    .HasColumnName("trangThai");

                entity.HasOne(d => d.MaPhieuTinhNavigation).WithMany(p => p.BangKeChietKhaus)
                    .HasForeignKey(d => d.MaPhieuTinh)
                    .HasConstraintName("FK__BangKeChi__maPhi__53D770D6");
            });

            modelBuilder.Entity<CauHinhChietKhau>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__CauHinhC__3213E83F12C2C922");

                entity.ToTable("CauHinhChietKhau");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.ChiTietLuat).HasColumnName("chiTietLuat");
                entity.Property(e => e.DoiTuongApDung)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("doiTuongApDung");
                entity.Property(e => e.LoaiQuyLuat)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("loaiQuyLuat");
                entity.Property(e => e.TenCauHinh)
                    .HasMaxLength(100)
                    .HasColumnName("tenCauHinh");
            });

            modelBuilder.Entity<ChiTietPhieuNhap>(entity =>
            {
                entity.HasKey(e => new { e.MaPhieu, e.MaHang }).HasName("PK__ChiTietP__458D7B2C093EF2CD");

                entity.ToTable("ChiTietPhieuNhap");

                entity.Property(e => e.MaPhieu)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maPhieu");
                entity.Property(e => e.MaHang)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maHang");
                entity.Property(e => e.DonGiaNhap)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("donGiaNhap");
                entity.Property(e => e.SoLuong).HasColumnName("soLuong");
                entity.Property(e => e.ThanhTien)
                    .HasComputedColumnSql("([soLuong]*[donGiaNhap])", false)
                    .HasColumnName("thanhTien");

                entity.HasOne(d => d.MaHangNavigation).WithMany(p => p.ChiTietPhieuNhaps)
                    .HasForeignKey(d => d.MaHang)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietPh__maHan__3EDC53F0");

                entity.HasOne(d => d.MaPhieuNavigation).WithMany(p => p.ChiTietPhieuNhaps)
                    .HasForeignKey(d => d.MaPhieu)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietPh__maPhi__3DE82FB7");
            });

            modelBuilder.Entity<ChiTietPhieuXuat>(entity =>
            {
                entity.HasKey(e => new { e.MaPhieu, e.MaHang }).HasName("PK__ChiTietP__458D7B2C6ABCE357");

                entity.ToTable("ChiTietPhieuXuat");

                entity.Property(e => e.MaPhieu)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maPhieu");
                entity.Property(e => e.MaHang)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maHang");
                entity.Property(e => e.GiaBan)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("giaBan");
                entity.Property(e => e.GiaVonTaiThoiDiem)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("giaVonTaiThoiDiem");
                entity.Property(e => e.SoLuong).HasColumnName("soLuong");
                entity.Property(e => e.ThanhTien)
                    .HasComputedColumnSql("([soLuong]*[giaBan])", false)
                    .HasColumnName("thanhTien");

                entity.HasOne(d => d.MaHangNavigation).WithMany(p => p.ChiTietPhieuXuats)
                    .HasForeignKey(d => d.MaHang)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietPh__maHan__4865BE2A");

                entity.HasOne(d => d.MaPhieuNavigation).WithMany(p => p.ChiTietPhieuXuats)
                    .HasForeignKey(d => d.MaPhieu)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietPh__maPhi__477199F1");
            });

            modelBuilder.Entity<ChiTietTon>(entity =>
            {
                entity.HasKey(e => new { e.MaKho, e.MaHang }).HasName("PK__ChiTietT__2AF7B9E70EBDD425");

                entity.ToTable("ChiTietTon");

                entity.Property(e => e.MaKho)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maKho");
                entity.Property(e => e.MaHang)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maHang");
                entity.Property(e => e.GiaTriTon)
                    .HasDefaultValue(0m)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("giaTriTon");
                entity.Property(e => e.SoLuongTon)
                    .HasDefaultValue(0.0)
                    .HasColumnName("soLuongTon");

                entity.HasOne(d => d.MaHangNavigation).WithMany(p => p.ChiTietTons)
                    .HasForeignKey(d => d.MaHang)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietTo__maHan__3552E9B6");

                entity.HasOne(d => d.MaKhoNavigation).WithMany(p => p.ChiTietTons)
                    .HasForeignKey(d => d.MaKho)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK__ChiTietTo__maKho__345EC57D");
            });

            modelBuilder.Entity<DaiLy>(entity =>
            {
                entity.HasKey(e => e.MaDaiLy).HasName("PK__DaiLy__29D3D8D36DED8AAA");

                entity.ToTable("DaiLy");

                entity.Property(e => e.MaDaiLy)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maDaiLy");
                entity.Property(e => e.LoaiDaiLy)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("loaiDaiLy");
                entity.Property(e => e.TenDaiLy)
                    .HasMaxLength(100)
                    .HasColumnName("tenDaiLy");
            });

            modelBuilder.Entity<HangHoa>(entity =>
            {
                entity.HasKey(e => e.MaHang).HasName("PK__HangHoa__C28CA331F1F0035B");

                entity.ToTable("HangHoa");

                entity.Property(e => e.MaHang)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maHang");
                entity.Property(e => e.DonViTinh)
                    .HasMaxLength(20)
                    .HasColumnName("donViTinh");
                entity.Property(e => e.GiaBanHienTai)
                    .HasDefaultValue(0m)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("giaBanHienTai");
                entity.Property(e => e.GiaVonHienTai)
                    .HasDefaultValue(0m)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("giaVonHienTai");
                entity.Property(e => e.QuyCach)
                    .HasMaxLength(50)
                    .HasColumnName("quyCach");
                entity.Property(e => e.TenHang)
                    .HasMaxLength(100)
                    .HasColumnName("tenHang");
            });

            modelBuilder.Entity<KhachHang>(entity =>
            {
                entity.HasKey(e => e.MaDoiTuong).HasName("PK__KhachHan__8B6358B408DF22E4");

                entity.ToTable("KhachHang");

                entity.Property(e => e.MaDoiTuong)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maDoiTuong");
                entity.Property(e => e.AoNuoi)
                    .HasMaxLength(100)
                    .HasColumnName("aoNuoi");
                entity.Property(e => e.DiaChi)
                    .HasMaxLength(200)
                    .HasColumnName("diaChi");
                entity.Property(e => e.DuNoLuyKe)
                    .HasDefaultValue(0m)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("duNoLuyKe");
                entity.Property(e => e.SoDienThoai)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("soDienThoai");
                entity.Property(e => e.TenDoiTuong)
                    .HasMaxLength(100)
                    .HasColumnName("tenDoiTuong");
            });

            modelBuilder.Entity<Kho>(entity =>
            {
                entity.HasKey(e => e.MaKho).HasName("PK__Kho__26DF73D49E2E612A");

                entity.ToTable("Kho");

                entity.Property(e => e.MaKho)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maKho");
                entity.Property(e => e.LoaiKho)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("loaiKho");
                entity.Property(e => e.MaDaiLyPhuTrach)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maDaiLyPhuTrach");
                entity.Property(e => e.TenKho)
                    .HasMaxLength(100)
                    .HasColumnName("tenKho");

                entity.HasOne(d => d.MaDaiLyPhuTrachNavigation).WithMany(p => p.Khos)
                    .HasForeignKey(d => d.MaDaiLyPhuTrach)
                    .HasConstraintName("FK__Kho__maDaiLyPhuT__2F9A1060");
            });

            modelBuilder.Entity<NhaCungCap>(entity =>
            {
                entity.HasKey(e => e.MaDoiTuong).HasName("PK__NhaCungC__8B6358B489523D70");

                entity.ToTable("NhaCungCap");

                entity.Property(e => e.MaDoiTuong)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maDoiTuong");
                entity.Property(e => e.DiaChi)
                    .HasMaxLength(200)
                    .HasColumnName("diaChi");
                entity.Property(e => e.MaSoThue)
                    .HasMaxLength(50)
                    .IsUnicode(false)
                    .HasColumnName("maSoThue");
                entity.Property(e => e.SoDienThoai)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("soDienThoai");
                entity.Property(e => e.SoNgayDuocNo)
                    .HasDefaultValue(0)
                    .HasColumnName("soNgayDuocNo");
                entity.Property(e => e.TenDoiTuong)
                    .HasMaxLength(100)
                    .HasColumnName("tenDoiTuong");
            });

            modelBuilder.Entity<PhieuNhap>(entity =>
            {
                entity.HasKey(e => e.MaPhieu).HasName("PK__PhieuNha__49A5B11FB051D1BD");

                entity.ToTable("PhieuNhap", tb => tb.HasTrigger("trg_PhieuNhap_TinhHanThanhToan"));

                entity.Property(e => e.MaPhieu)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maPhieu");
                entity.Property(e => e.HanThanhToan)
                    .HasColumnType("datetime")
                    .HasColumnName("hanThanhToan");
                entity.Property(e => e.IdDaiLyNhap)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("idDaiLyNhap");
                entity.Property(e => e.IdNhaCungCap)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("idNhaCungCap");
                entity.Property(e => e.NgayNhap)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime")
                    .HasColumnName("ngayNhap");
                entity.Property(e => e.TongTien)
                    .HasDefaultValue(0m)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("tongTien");

                entity.HasOne(d => d.IdDaiLyNhapNavigation).WithMany(p => p.PhieuNhaps)
                    .HasForeignKey(d => d.IdDaiLyNhap)
                    .HasConstraintName("FK__PhieuNhap__idDai__3A179ED3");

                entity.HasOne(d => d.IdNhaCungCapNavigation).WithMany(p => p.PhieuNhaps)
                    .HasForeignKey(d => d.IdNhaCungCap)
                    .HasConstraintName("FK__PhieuNhap__idNha__3B0BC30C");
            });

            modelBuilder.Entity<PhieuThuChi>(entity =>
            {
                entity.HasKey(e => e.MaPhieu).HasName("PK__PhieuThu__49A5B11FF04AD33C");

                entity.ToTable("PhieuThuChi");

                entity.Property(e => e.MaPhieu)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maPhieu");
                entity.Property(e => e.LoaiPhieu)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("loaiPhieu");
                entity.Property(e => e.LyDo)
                    .HasMaxLength(200)
                    .HasColumnName("lyDo");
                entity.Property(e => e.NgayLap)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime")
                    .HasColumnName("ngayLap");
                entity.Property(e => e.SoTien)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("soTien");
            });

            modelBuilder.Entity<PhieuTinhChietKhau>(entity =>
            {
                entity.HasKey(e => e.MaPhieuTinh).HasName("PK__PhieuTin__4BED573111C7DFF5");

                entity.ToTable("PhieuTinhChietKhau");

                entity.Property(e => e.MaPhieuTinh)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maPhieuTinh");
                entity.Property(e => e.DenNgay).HasColumnName("denNgay");
                entity.Property(e => e.LoaiDoiTuong)
                    .HasMaxLength(10)
                    .IsUnicode(false)
                    .HasColumnName("loaiDoiTuong");
                entity.Property(e => e.NgayTao)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime")
                    .HasColumnName("ngayTao");
                entity.Property(e => e.TuNgay).HasColumnName("tuNgay");
            });

            modelBuilder.Entity<PhieuXuat>(entity =>
            {
                entity.HasKey(e => e.MaPhieu).HasName("PK__PhieuXua__49A5B11FB6FD6FC7");

                entity.ToTable("PhieuXuat");

                entity.Property(e => e.MaPhieu)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maPhieu");
                entity.Property(e => e.IdDaiLyBan)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("idDaiLyBan");
                entity.Property(e => e.IdKhachHang)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("idKhachHang");
                entity.Property(e => e.NgayXuat)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime")
                    .HasColumnName("ngayXuat");
                entity.Property(e => e.TongTien)
                    .HasDefaultValue(0m)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("tongTien");

                entity.HasOne(d => d.IdDaiLyBanNavigation).WithMany(p => p.PhieuXuats)
                    .HasForeignKey(d => d.IdDaiLyBan)
                    .HasConstraintName("FK__PhieuXuat__idDai__43A1090D");

                entity.HasOne(d => d.IdKhachHangNavigation).WithMany(p => p.PhieuXuats)
                    .HasForeignKey(d => d.IdKhachHang)
                    .HasConstraintName("FK__PhieuXuat__idKha__44952D46");
            });

            modelBuilder.Entity<SoRiengKhachHang>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__SoRiengK__3213E83FD432CDC9");

                entity.ToTable("SoRiengKhachHang");

                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.DienGiai)
                    .HasMaxLength(200)
                    .HasColumnName("dienGiai");
                entity.Property(e => e.LoaiGiaoDich)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("loaiGiaoDich");
                entity.Property(e => e.MaKhachHang)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maKhachHang");
                entity.Property(e => e.NgayGiaoDich)
                    .HasColumnType("datetime")
                    .HasColumnName("ngayGiaoDich");
                entity.Property(e => e.SoTienPhatSinh)
                    .HasColumnType("decimal(18, 2)")
                    .HasColumnName("soTienPhatSinh");

                entity.HasOne(d => d.MaKhachHangNavigation).WithMany(p => p.SoRiengKhachHangs)
                    .HasForeignKey(d => d.MaKhachHang)
                    .HasConstraintName("FK__SoRiengKh__maKha__57A801BA");
            });

            modelBuilder.Entity<NguoiDung>(entity =>
            {
                entity.HasKey(e => e.MaNguoiDung).HasName("PK_NguoiDung");

                entity.ToTable("NguoiDung");

                entity.Property(e => e.MaNguoiDung)
                    .HasMaxLength(20)
                    .IsUnicode(false)
                    .HasColumnName("maNguoiDung");
                entity.Property(e => e.TenNguoiDung)
                    .HasMaxLength(100)
                    .HasColumnName("tenNguoiDung");
                entity.Property(e => e.MatKhau)
                    .HasMaxLength(100)
                    .HasColumnName("matKhau");
                entity.Property(e => e.QuyenNguoiDung)
                    .HasColumnName("quyenNguoiDung");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
