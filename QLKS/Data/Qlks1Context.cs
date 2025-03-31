using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace QLKS.Data;

public partial class Qlks1Context : DbContext
{
    public Qlks1Context()
    {
    }

    public Qlks1Context(DbContextOptions<Qlks1Context> options)
        : base(options)
    {
    }

    public virtual DbSet<DatPhong> DatPhongs { get; set; }

    public virtual DbSet<DichVu> DichVus { get; set; }

    public virtual DbSet<HoaDon> HoaDons { get; set; }

    public virtual DbSet<KhachHang> KhachHangs { get; set; }

    public virtual DbSet<LoaiPhong> LoaiPhongs { get; set; }

    public virtual DbSet<NhanVien> NhanViens { get; set; }

    public virtual DbSet<Phong> Phongs { get; set; }

    public virtual DbSet<PhuThu> PhuThus { get; set; }

    public virtual DbSet<SuDungDichVu> SuDungDichVus { get; set; }

    public virtual DbSet<VaiTro> VaiTros { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=PHIPHI\\PHIPHI;Initial Catalog=QLKS1;Integrated Security=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DatPhong>(entity =>
        {
            entity.HasKey(e => e.MaDatPhong).HasName("PK__DatPhong__6344ADEADDB3EDA3");

            entity.ToTable("DatPhong");

            entity.Property(e => e.MaKh).HasColumnName("MaKH");
            entity.Property(e => e.MaNv).HasColumnName("MaNV");
            entity.Property(e => e.MaPhong)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.NgayDat).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PhuThu)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(12, 2)");
            entity.Property(e => e.TongTienPhong).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.TrangThai).HasMaxLength(20);

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.DatPhongs)
                .HasForeignKey(d => d.MaKh)
                .HasConstraintName("FK__DatPhong__MaKH__4D94879B");

            entity.HasOne(d => d.MaNvNavigation).WithMany(p => p.DatPhongs)
                .HasForeignKey(d => d.MaNv)
                .HasConstraintName("FK__DatPhong__MaNV__4CA06362");

            entity.HasOne(d => d.MaPhongNavigation).WithMany(p => p.DatPhongs)
                .HasForeignKey(d => d.MaPhong)
                .HasConstraintName("FK__DatPhong__MaPhon__4E88ABD4");
        });

        modelBuilder.Entity<DichVu>(entity =>
        {
            entity.HasKey(e => e.MaDichVu).HasName("PK__DichVu__C0E6DE8FDDE3EFC9");

            entity.ToTable("DichVu");

            entity.Property(e => e.DonGia).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.MoTa).HasMaxLength(200);
            entity.Property(e => e.TenDichVu).HasMaxLength(100);
        });

        modelBuilder.Entity<HoaDon>(entity =>
        {
            entity.HasKey(e => e.MaHoaDon).HasName("PK__HoaDon__835ED13B6CA342B5");

            entity.ToTable("HoaDon");

            entity.Property(e => e.MaKh).HasColumnName("MaKH");
            entity.Property(e => e.MaNv).HasColumnName("MaNV");
            entity.Property(e => e.NgayLap).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PhuongThucThanhToan).HasMaxLength(50);
            entity.Property(e => e.TongTien).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.TrangThai).HasMaxLength(20);

            entity.HasOne(d => d.MaKhNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaKh)
                .HasConstraintName("FK__HoaDon__MaKH__59FA5E80");

            entity.HasOne(d => d.MaNvNavigation).WithMany(p => p.HoaDons)
                .HasForeignKey(d => d.MaNv)
                .HasConstraintName("FK__HoaDon__MaNV__5AEE82B9");
        });

        modelBuilder.Entity<KhachHang>(entity =>
        {
            entity.HasKey(e => e.MaKh).HasName("PK__KhachHan__2725CF1E799D98F2");

            entity.ToTable("KhachHang");

            entity.HasIndex(e => e.CccdPassport, "UQ__KhachHan__F045CC196DB2CE6F").IsUnique();

            entity.Property(e => e.MaKh).HasColumnName("MaKH");
            entity.Property(e => e.CccdPassport)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("CCCD_Passport");
            entity.Property(e => e.GhiChu).HasMaxLength(200);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.QuocTich).HasMaxLength(50);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);
        });

        modelBuilder.Entity<LoaiPhong>(entity =>
        {
            entity.HasKey(e => e.MaLoaiPhong).HasName("PK__LoaiPhon__23021217E5C00605");

            entity.ToTable("LoaiPhong");

            entity.Property(e => e.GiaCoBan).HasColumnType("decimal(12, 2)");
            entity.Property(e => e.TenLoaiPhong).HasMaxLength(50);
        });

        modelBuilder.Entity<NhanVien>(entity =>
        {
            entity.HasKey(e => e.MaNv).HasName("PK__NhanVien__2725D70AA5D2D315");

            entity.ToTable("NhanVien");

            entity.Property(e => e.MaNv).HasColumnName("MaNV");
            entity.Property(e => e.DiaChi).HasMaxLength(100);
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.SoDienThoai)
                .HasMaxLength(15)
                .IsUnicode(false);

            entity.HasOne(d => d.MaVaiTroNavigation).WithMany(p => p.NhanViens)
                .HasForeignKey(d => d.MaVaiTro)
                .HasConstraintName("FK__NhanVien__MaVaiT__3A81B327");
        });

        modelBuilder.Entity<Phong>(entity =>
        {
            entity.HasKey(e => e.MaPhong).HasName("PK__Phong__20BD5E5BAA898248");

            entity.ToTable("Phong");

            entity.Property(e => e.MaPhong)
                .HasMaxLength(10)
                .IsUnicode(false);
            entity.Property(e => e.TenPhong).HasMaxLength(50);
            entity.Property(e => e.TrangThai).HasMaxLength(20);

            entity.HasOne(d => d.MaLoaiPhongNavigation).WithMany(p => p.Phongs)
                .HasForeignKey(d => d.MaLoaiPhong)
                .HasConstraintName("FK__Phong__MaLoaiPho__46E78A0C");
        });

        modelBuilder.Entity<PhuThu>(entity =>
        {
            entity.HasKey(e => e.MaPhuThu).HasName("PK__PhuThu__FD0E3BFAE1FB440D");

            entity.ToTable("PhuThu");

            entity.Property(e => e.PhuThuNguoiThem)
                .HasDefaultValue(200000m)
                .HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.MaLoaiPhongNavigation).WithMany(p => p.PhuThus)
                .HasForeignKey(d => d.MaLoaiPhong)
                .HasConstraintName("FK__PhuThu__MaLoaiPh__4316F928");
        });

        modelBuilder.Entity<SuDungDichVu>(entity =>
        {
            entity.HasKey(e => e.MaSuDung).HasName("PK__SuDungDi__73EF96E98996AD9F");

            entity.ToTable("SuDungDichVu");

            entity.Property(e => e.NgaySuDung).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ThanhTien).HasColumnType("decimal(12, 2)");

            entity.HasOne(d => d.MaDatPhongNavigation).WithMany(p => p.SuDungDichVus)
                .HasForeignKey(d => d.MaDatPhong)
                .HasConstraintName("FK__SuDungDic__MaDat__5441852A");

            entity.HasOne(d => d.MaDichVuNavigation).WithMany(p => p.SuDungDichVus)
                .HasForeignKey(d => d.MaDichVu)
                .HasConstraintName("FK__SuDungDic__MaDic__5535A963");
        });

        modelBuilder.Entity<VaiTro>(entity =>
        {
            entity.HasKey(e => e.MaVaiTro).HasName("PK__VaiTro__C24C41CF5FE0FA1C");

            entity.ToTable("VaiTro");

            entity.Property(e => e.TenVaiTro).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
