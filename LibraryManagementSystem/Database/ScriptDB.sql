/* =====================================================================
   BÀI 6.6 - LIBRARY MANAGEMENT SYSTEM
   Script khởi tạo CSDL LibraryDB (SQL Server / LocalDB) + dữ liệu mẫu
   Chạy toàn bộ script trong SSMS (Server: (localdb)\MSSQLLocalDB)
   ===================================================================== */
USE master;
GO
IF DB_ID(N'LibraryDB') IS NOT NULL
BEGIN
    ALTER DATABASE LibraryDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE LibraryDB;
END
GO
CREATE DATABASE LibraryDB;
GO
USE LibraryDB;
GO

/* ---------- 1. DocGia ---------- */
CREATE TABLE DocGia (
    MaDocGia       INT IDENTITY(1,1) NOT NULL,
    HoTen          NVARCHAR(100)     NOT NULL,
    Email          VARCHAR(100)      NULL,
    NgayHetHanThe  DATETIME          NOT NULL,
    CONSTRAINT PK_DocGia PRIMARY KEY (MaDocGia),
    CONSTRAINT UQ_DocGia_Email UNIQUE (Email)
);
GO

/* ---------- 2. DauSach ---------- */
CREATE TABLE DauSach (
    MaSach      INT IDENTITY(1,1) NOT NULL,
    TenSach     NVARCHAR(200)     NOT NULL,
    TheLoai     NVARCHAR(50)      NULL,
    SoLuongCon  INT               NOT NULL CONSTRAINT DF_DauSach_SoLuongCon DEFAULT 0,
    CONSTRAINT PK_DauSach PRIMARY KEY (MaSach),
    CONSTRAINT CK_DauSach_SoLuongCon CHECK (SoLuongCon >= 0)
);
GO

/* ---------- 3. PhieuMuon ---------- */
CREATE TABLE PhieuMuon (
    MaPhieu    INT IDENTITY(1,1) NOT NULL,
    MaDocGia   INT               NOT NULL,
    NgayMuon   DATETIME          NOT NULL CONSTRAINT DF_PhieuMuon_NgayMuon DEFAULT GETDATE(),
    HanTra     DATETIME          NOT NULL,
    TrangThai  NVARCHAR(50)      NOT NULL CONSTRAINT DF_PhieuMuon_TrangThai DEFAULT N'Đang mượn',
    CONSTRAINT PK_PhieuMuon PRIMARY KEY (MaPhieu),
    CONSTRAINT FK_PhieuMuon_DocGia FOREIGN KEY (MaDocGia) REFERENCES DocGia(MaDocGia),
    CONSTRAINT CK_PhieuMuon_TrangThai CHECK (TrangThai IN (N'Đang mượn', N'Đã trả', N'Quá hạn'))
);
GO

/* ---------- 4. ChiTietMuon ---------- */
CREATE TABLE ChiTietMuon (
    MaPhieu   INT NOT NULL,
    MaSach    INT NOT NULL,
    SoLuong   INT NOT NULL,
    CONSTRAINT PK_ChiTietMuon PRIMARY KEY (MaPhieu, MaSach),
    CONSTRAINT FK_ChiTietMuon_PhieuMuon FOREIGN KEY (MaPhieu) REFERENCES PhieuMuon(MaPhieu),
    CONSTRAINT FK_ChiTietMuon_DauSach   FOREIGN KEY (MaSach)  REFERENCES DauSach(MaSach),
    CONSTRAINT CK_ChiTietMuon_SoLuong CHECK (SoLuong > 0)
);
GO

/* ---------- DỮ LIỆU MẪU ---------- */
INSERT INTO DocGia (HoTen, Email, NgayHetHanThe) VALUES
(N'Nguyễn Văn An',  'an.nguyen@example.com',   DATEADD(YEAR, 1, GETDATE())),
(N'Trần Thị Bình',  'binh.tran@example.com',   DATEADD(MONTH, 6, GETDATE())),
(N'Lê Minh Châu',   'chau.le@example.com',     DATEADD(YEAR, 2, GETDATE())),
(N'Phạm Quốc Dũng', 'dung.pham@example.com',   DATEADD(MONTH, 3, GETDATE())),
(N'Hoàng Thu Hà',   'ha.hoang@example.com',    '2025-01-31 23:59:59');   -- thẻ đã hết hạn (để test)
GO

INSERT INTO DauSach (TenSach, TheLoai, SoLuongCon) VALUES
(N'Lập trình C# căn bản',            N'Công nghệ thông tin', 9),
(N'Cơ sở dữ liệu SQL Server',        N'Công nghệ thông tin', 4),
(N'Cấu trúc dữ liệu và giải thuật',  N'Công nghệ thông tin', 6),
(N'Kinh tế vi mô',                   N'Kinh tế',             5),
(N'Nguyên lý Marketing',             N'Kinh tế',             8),
(N'Truyện Kiều',                     N'Văn học',             2),
(N'Số đỏ',                           N'Văn học',             6),
(N'Dế Mèn phiêu lưu ký',             N'Văn học',             5);
GO

-- Các phiếu mượn mẫu (ngày tính tương đối so với hôm nay để luôn có phiếu quá hạn)
INSERT INTO PhieuMuon (MaDocGia, NgayMuon, HanTra, TrangThai) VALUES
(1, DATEADD(DAY, -30, GETDATE()), DATEADD(DAY, -16, GETDATE()), N'Đang mượn'), -- 1: quá hạn
(2, DATEADD(DAY, -20, GETDATE()), DATEADD(DAY,  -6, GETDATE()), N'Đang mượn'), -- 2: quá hạn
(1, DATEADD(DAY, -10, GETDATE()), DATEADD(DAY,   4, GETDATE()), N'Đang mượn'), -- 3: còn hạn
(3, DATEADD(DAY, -40, GETDATE()), DATEADD(DAY, -26, GETDATE()), N'Đã trả'),    -- 4
(4, DATEADD(DAY,  -5, GETDATE()), DATEADD(DAY,   9, GETDATE()), N'Đang mượn'), -- 5: còn hạn
(2, DATEADD(DAY, -60, GETDATE()), DATEADD(DAY, -46, GETDATE()), N'Đã trả');    -- 6
GO

INSERT INTO ChiTietMuon (MaPhieu, MaSach, SoLuong) VALUES
(1, 1, 1), (1, 3, 1),
(2, 2, 1),
(3, 4, 1),
(4, 1, 1), (4, 5, 1),
(5, 6, 2),
(6, 7, 1);
GO

-- Kiểm tra nhanh
SELECT * FROM DocGia;
SELECT * FROM DauSach;
SELECT * FROM PhieuMuon;
SELECT * FROM ChiTietMuon;
GO
