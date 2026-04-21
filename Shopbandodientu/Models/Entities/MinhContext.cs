using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Shopbandodientu.Models.Entities;

public partial class MinhContext : DbContext
{
    public MinhContext()
    {
    }

    public MinhContext(DbContextOptions<MinhContext> options)
        : base(options)
    {
    }//

    public virtual DbSet<Chitietdonhang> Chitietdonhangs { get; set; }

    public virtual DbSet<Chitietgiohang> Chitietgiohangs { get; set; }

    public virtual DbSet<Danhgium> Danhgia { get; set; }

    public virtual DbSet<Danhmuc> Danhmucs { get; set; }

    public virtual DbSet<Donhang> Donhangs { get; set; }

    public virtual DbSet<Giohang> Giohangs { get; set; }

    public virtual DbSet<Lichsusudungmagiamgium> Lichsusudungmagiamgia { get; set; }

    public virtual DbSet<Loaitaikhoan> Loaitaikhoans { get; set; }

    public virtual DbSet<Magiamgium> Magiamgia { get; set; }

    public virtual DbSet<Sanpham> Sanphams { get; set; }

    public virtual DbSet<Sanphamhinhanh> Sanphamhinhanhs { get; set; }

    public virtual DbSet<Taikhoan> Taikhoans { get; set; }

    public virtual DbSet<Thanhtoan> Thanhtoans { get; set; }

    public virtual DbSet<Thongso> Thongsos { get; set; }

    public virtual DbSet<Tinnhan> Tinnhans { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseNpgsql("Name=ConnectionStrings:Connection");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Chitietdonhang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("chitietdonhang_pkey");

            entity.ToTable("chitietdonhang");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Dongia)
                .HasPrecision(18, 2)
                .HasColumnName("dongia");
            entity.Property(e => e.Donhangid).HasColumnName("donhangid");
            entity.Property(e => e.Sanphamid).HasColumnName("sanphamid");
            entity.Property(e => e.Soluong).HasColumnName("soluong");

            entity.HasOne(d => d.Donhang).WithMany(p => p.Chitietdonhangs)
                .HasForeignKey(d => d.Donhangid)
                .HasConstraintName("chitietdonhang_donhangid_fkey");

            entity.HasOne(d => d.Sanpham).WithMany(p => p.Chitietdonhangs)
                .HasForeignKey(d => d.Sanphamid)
                .HasConstraintName("chitietdonhang_sanphamid_fkey");
        });

        modelBuilder.Entity<Chitietgiohang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("chitietgiohang_pkey");

            entity.ToTable("chitietgiohang");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Giohangid).HasColumnName("giohangid");
            entity.Property(e => e.Sanphamid).HasColumnName("sanphamid");
            entity.Property(e => e.Soluong).HasColumnName("soluong");

            entity.HasOne(d => d.Giohang).WithMany(p => p.Chitietgiohangs)
                .HasForeignKey(d => d.Giohangid)
                .HasConstraintName("chitietgiohang_giohangid_fkey");

            entity.HasOne(d => d.Sanpham).WithMany(p => p.Chitietgiohangs)
                .HasForeignKey(d => d.Sanphamid)
                .HasConstraintName("chitietgiohang_sanphamid_fkey");
        });

        modelBuilder.Entity<Danhgium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("danhgia_pkey");

            entity.ToTable("danhgia");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Diem).HasColumnName("diem");
            entity.Property(e => e.Ngaydanhgia)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngaydanhgia");
            entity.Property(e => e.Noidung)
                .HasMaxLength(500)
                .HasColumnName("noidung");
            entity.Property(e => e.Sanphamid).HasColumnName("sanphamid");
            entity.Property(e => e.Taikhoanid).HasColumnName("taikhoanid");

            entity.HasOne(d => d.Sanpham).WithMany(p => p.Danhgia)
                .HasForeignKey(d => d.Sanphamid)
                .HasConstraintName("danhgia_sanphamid_fkey");

            entity.HasOne(d => d.Taikhoan).WithMany(p => p.Danhgia)
                .HasForeignKey(d => d.Taikhoanid)
                .HasConstraintName("danhgia_taikhoanid_fkey");
        });

        modelBuilder.Entity<Danhmuc>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("danhmuc_pkey");

            entity.ToTable("danhmuc");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Tendanhmuc)
                .HasMaxLength(100)
                .HasColumnName("tendanhmuc");
        });

        modelBuilder.Entity<Donhang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("donhang_pkey");

            entity.ToTable("donhang");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Diachigiaohang)
                .HasMaxLength(255)
                .HasColumnName("diachigiaohang");
            entity.Property(e => e.Ngaydat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngaydat");
            entity.Property(e => e.Sdtnguoinhan)
                .HasMaxLength(20)
                .HasColumnName("sdtnguoinhan");
            entity.Property(e => e.Taikhoanid).HasColumnName("taikhoanid");
            entity.Property(e => e.Tennguoinhan)
                .HasMaxLength(100)
                .HasColumnName("tennguoinhan");
            entity.Property(e => e.Tongtien)
                .HasPrecision(18, 2)
                .HasColumnName("tongtien");
            entity.Property(e => e.Trangthai)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Chờ xử lý'::character varying")
                .HasColumnName("trangthai");

            entity.HasOne(d => d.Taikhoan).WithMany(p => p.Donhangs)
                .HasForeignKey(d => d.Taikhoanid)
                .HasConstraintName("donhang_taikhoanid_fkey");
        });

        modelBuilder.Entity<Giohang>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("giohang_pkey");

            entity.ToTable("giohang");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Ngaycapnhat)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngaycapnhat");
            entity.Property(e => e.Taikhoanid).HasColumnName("taikhoanid");

            entity.HasOne(d => d.Taikhoan).WithMany(p => p.Giohangs)
                .HasForeignKey(d => d.Taikhoanid)
                .HasConstraintName("giohang_taikhoanid_fkey");
        });

        modelBuilder.Entity<Lichsusudungmagiamgium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("lichsusudungmagiamgia_pkey");

            entity.ToTable("lichsusudungmagiamgia");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Donhangid).HasColumnName("donhangid");
            entity.Property(e => e.Giatrigiamthucte)
                .HasPrecision(18, 2)
                .HasColumnName("giatrigiamthucte");
            entity.Property(e => e.Magiamgiaid).HasColumnName("magiamgiaid");
            entity.Property(e => e.Ngaysudung)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngaysudung");
            entity.Property(e => e.Taikhoanid).HasColumnName("taikhoanid");

            entity.HasOne(d => d.Donhang).WithMany(p => p.Lichsusudungmagiamgia)
                .HasForeignKey(d => d.Donhangid)
                .HasConstraintName("lichsusudungmagiamgia_donhangid_fkey");

            entity.HasOne(d => d.Magiamgia).WithMany(p => p.Lichsusudungmagiamgia)
                .HasForeignKey(d => d.Magiamgiaid)
                .HasConstraintName("lichsusudungmagiamgia_magiamgiaid_fkey");

            entity.HasOne(d => d.Taikhoan).WithMany(p => p.Lichsusudungmagiamgia)
                .HasForeignKey(d => d.Taikhoanid)
                .HasConstraintName("lichsusudungmagiamgia_taikhoanid_fkey");
        });

        modelBuilder.Entity<Loaitaikhoan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("loaitaikhoan_pkey");

            entity.ToTable("loaitaikhoan");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Tenloai)
                .HasMaxLength(50)
                .HasColumnName("tenloai");
        });

        modelBuilder.Entity<Magiamgium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("magiamgia_pkey");

            entity.ToTable("magiamgia");

            entity.HasIndex(e => e.Macode, "magiamgia_macode_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Giamtoida)
                .HasPrecision(18, 2)
                .HasColumnName("giamtoida");
            entity.Property(e => e.Giatridonhangtoithieu)
                .HasPrecision(18, 2)
                .HasDefaultValueSql("0")
                .HasColumnName("giatridonhangtoithieu");
            entity.Property(e => e.Giatrigiam)
                .HasPrecision(18, 2)
                .HasColumnName("giatrigiam");
            entity.Property(e => e.Loaigiamgia)
                .HasMaxLength(20)
                .HasColumnName("loaigiamgia");
            entity.Property(e => e.Macode)
                .HasMaxLength(50)
                .HasColumnName("macode");
            entity.Property(e => e.Mota).HasColumnName("mota");
            entity.Property(e => e.Ngaybatdau)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngaybatdau");
            entity.Property(e => e.Ngayketthuc)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngayketthuc");
            entity.Property(e => e.Ngaytao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngaytao");
            entity.Property(e => e.Soluong).HasColumnName("soluong");
            entity.Property(e => e.Soluongdasudung)
                .HasDefaultValue(0)
                .HasColumnName("soluongdasudung");
            entity.Property(e => e.Tenchuongtrinh)
                .HasMaxLength(255)
                .HasColumnName("tenchuongtrinh");
            entity.Property(e => e.Trangthai)
                .HasDefaultValue(true)
                .HasColumnName("trangthai");
        });

        modelBuilder.Entity<Sanpham>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sanpham_pkey");

            entity.ToTable("sanpham");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Danhmucid).HasColumnName("danhmucid");
            entity.Property(e => e.Gia)
                .HasPrecision(18, 2)
                .HasColumnName("gia");
            entity.Property(e => e.Khuyenmai).HasColumnName("khuyenmai");
            entity.Property(e => e.Mota).HasColumnName("mota");
            entity.Property(e => e.Ngaythem)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngaythem");
            entity.Property(e => e.Soluong).HasColumnName("soluong");
            entity.Property(e => e.Tensanpham)
                .HasMaxLength(255)
                .HasColumnName("tensanpham");
            entity.Property(e => e.Thuonghieu)
                .HasMaxLength(30)
                .HasColumnName("thuonghieu");
            entity.Property(e => e.Trangthai)
                .HasDefaultValue(true)
                .HasColumnName("trangthai");

            entity.HasOne(d => d.Danhmuc).WithMany(p => p.Sanphams)
                .HasForeignKey(d => d.Danhmucid)
                .HasConstraintName("sanpham_danhmucid_fkey");
        });

        modelBuilder.Entity<Sanphamhinhanh>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("sanphamhinhanh_pkey");

            entity.ToTable("sanphamhinhanh");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Duongdan)
                .HasMaxLength(255)
                .HasColumnName("duongdan");
            entity.Property(e => e.Sanphamid).HasColumnName("sanphamid");

            entity.HasOne(d => d.Sanpham).WithMany(p => p.Sanphamhinhanhs)
                .HasForeignKey(d => d.Sanphamid)
                .HasConstraintName("sanphamhinhanh_sanphamid_fkey");
        });

        modelBuilder.Entity<Taikhoan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("taikhoan_pkey");

            entity.ToTable("taikhoan");

            entity.HasIndex(e => e.Email, "taikhoan_email_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Anhdaidien)
                .HasMaxLength(255)
                .HasColumnName("anhdaidien");
            entity.Property(e => e.Code)
                .HasMaxLength(30)
                .HasColumnName("code");
            entity.Property(e => e.Diachi)
                .HasMaxLength(255)
                .HasColumnName("diachi");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Hoten)
                .HasMaxLength(100)
                .HasColumnName("hoten");
            entity.Property(e => e.Iddanhmuc).HasColumnName("iddanhmuc");
            entity.Property(e => e.Loaitaikhoanid).HasColumnName("loaitaikhoanid");
            entity.Property(e => e.Matkhau)
                .HasMaxLength(255)
                .HasColumnName("matkhau");
            entity.Property(e => e.Ngaytao)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngaytao");
            entity.Property(e => e.Sodienthoai)
                .HasMaxLength(20)
                .HasColumnName("sodienthoai");
            entity.Property(e => e.Trangthai)
                .HasDefaultValue(true)
                .HasColumnName("trangthai");

            entity.HasOne(d => d.IddanhmucNavigation).WithMany(p => p.Taikhoans)
                .HasForeignKey(d => d.Iddanhmuc)
                .HasConstraintName("taikhoan_iddanhmuc_fkey");

            entity.HasOne(d => d.Loaitaikhoan).WithMany(p => p.Taikhoans)
                .HasForeignKey(d => d.Loaitaikhoanid)
                .HasConstraintName("taikhoan_loaitaikhoanid_fkey");
        });

        modelBuilder.Entity<Thanhtoan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("thanhtoan_pkey");

            entity.ToTable("thanhtoan");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Congthanhtoan)
                .HasMaxLength(50)
                .HasColumnName("congthanhtoan");
            entity.Property(e => e.Donhangid).HasColumnName("donhangid");
            entity.Property(e => e.Magiaodich)
                .HasMaxLength(100)
                .HasColumnName("magiaodich");
            entity.Property(e => e.Ngaythanhtoan)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("ngaythanhtoan");
            entity.Property(e => e.Phuongthuc)
                .HasMaxLength(100)
                .HasColumnName("phuongthuc");
            entity.Property(e => e.Sotien)
                .HasPrecision(18, 2)
                .HasColumnName("sotien");
            entity.Property(e => e.Trangthai)
                .HasMaxLength(50)
                .HasDefaultValueSql("'Chưa thanh toán'::character varying")
                .HasColumnName("trangthai");

            entity.HasOne(d => d.Donhang).WithMany(p => p.Thanhtoans)
                .HasForeignKey(d => d.Donhangid)
                .HasConstraintName("thanhtoan_donhangid_fkey");
        });

        modelBuilder.Entity<Thongso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("thongso_pkey");

            entity.ToTable("thongso");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cpu)
                .HasMaxLength(100)
                .HasColumnName("cpu");
            entity.Property(e => e.Idsanpham).HasColumnName("idsanpham");
            entity.Property(e => e.Ram)
                .HasMaxLength(100)
                .HasColumnName("ram");
            entity.Property(e => e.Rom)
                .HasMaxLength(100)
                .HasColumnName("rom");
            entity.Property(e => e.Vga)
                .HasMaxLength(100)
                .HasColumnName("vga");

            entity.HasOne(d => d.IdsanphamNavigation).WithMany(p => p.Thongsos)
                .HasForeignKey(d => d.Idsanpham)
                .HasConstraintName("thongso_idsanpham_fkey");
        });

        modelBuilder.Entity<Tinnhan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("tinnhan_pkey");

            entity.ToTable("tinnhan");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Nguoiguiid).HasColumnName("nguoiguiid");
            entity.Property(e => e.Nguoinhanid).HasColumnName("nguoinhanid");
            entity.Property(e => e.Noidung)
                .HasMaxLength(1000)
                .HasColumnName("noidung");
            entity.Property(e => e.Thoigian)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("thoigian");

            entity.HasOne(d => d.Nguoigui).WithMany(p => p.TinnhanNguoiguis)
                .HasForeignKey(d => d.Nguoiguiid)
                .HasConstraintName("tinnhan_nguoiguiid_fkey");

            entity.HasOne(d => d.Nguoinhan).WithMany(p => p.TinnhanNguoinhans)
                .HasForeignKey(d => d.Nguoinhanid)
                .HasConstraintName("tinnhan_nguoinhanid_fkey");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
