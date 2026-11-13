using LibraryManagement.DTO;
using Microsoft.Data.SqlClient;

namespace LibraryManagement.DAL;

public class PhieuMuonDAL
{
    // ---------------------------------------------------------------
    // Lọc phiếu mượn theo độc giả và/hoặc trạng thái (NULL = không lọc)
    // ---------------------------------------------------------------
    public List<PhieuMuonDTO> GetByFilter(int? maDocGia, string trangThai)
    {
        var list = new List<PhieuMuonDTO>();
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            @"SELECT p.MaPhieu, p.MaDocGia, d.HoTen, p.NgayMuon, p.HanTra, p.TrangThai
              FROM PhieuMuon p
              INNER JOIN DocGia d ON p.MaDocGia = d.MaDocGia
              WHERE (@MaDocGia IS NULL OR p.MaDocGia = @MaDocGia)
                AND (@TrangThai IS NULL OR p.TrangThai = @TrangThai)
              ORDER BY p.MaPhieu DESC", conn);
        cmd.Parameters.AddWithValue("@MaDocGia", (object)maDocGia ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@TrangThai", (object)trangThai ?? DBNull.Value);
        conn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new PhieuMuonDTO
            {
                MaPhieu = rd.GetInt32(0),
                MaDocGia = rd.GetInt32(1),
                HoTen = rd.GetString(2),
                NgayMuon = rd.GetDateTime(3),
                HanTra = rd.GetDateTime(4),
                TrangThai = rd.GetString(5)
            });
        }
        return list;
    }

    public List<ChiTietMuonDTO> GetChiTiet(int maPhieu)
    {
        var list = new List<ChiTietMuonDTO>();
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            @"SELECT ct.MaPhieu, ct.MaSach, s.TenSach, ct.SoLuong
              FROM ChiTietMuon ct
              INNER JOIN DauSach s ON ct.MaSach = s.MaSach
              WHERE ct.MaPhieu = @MaPhieu", conn);
        cmd.Parameters.AddWithValue("@MaPhieu", maPhieu);
        conn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new ChiTietMuonDTO
            {
                MaPhieu = rd.GetInt32(0),
                MaSach = rd.GetInt32(1),
                TenSach = rd.GetString(2),
                SoLuong = rd.GetInt32(3)
            });
        }
        return list;
    }

    // ---------------------------------------------------------------
    // Lập phiếu mượn: INSERT phiếu + chi tiết + trừ SoLuongCon (1 transaction)
    // ---------------------------------------------------------------
    public int Insert(PhieuMuonDTO p)
    {
        using var conn = DbHelper.CreateConnection();
        conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            int maPhieu;
            using (var cmd = new SqlCommand(
                @"INSERT INTO PhieuMuon (MaDocGia, HanTra, TrangThai)
                  VALUES (@MaDocGia, @HanTra, N'Đang mượn');
                  SELECT CAST(SCOPE_IDENTITY() AS int);", conn, tran))
            {
                cmd.Parameters.AddWithValue("@MaDocGia", p.MaDocGia);
                cmd.Parameters.AddWithValue("@HanTra", p.HanTra);
                maPhieu = (int)cmd.ExecuteScalar();
            }

            foreach (var ct in p.ChiTiet)
            {
                using (var cmd = new SqlCommand(
                    @"INSERT INTO ChiTietMuon (MaPhieu, MaSach, SoLuong)
                      VALUES (@MaPhieu, @MaSach, @SoLuong)", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@MaPhieu", maPhieu);
                    cmd.Parameters.AddWithValue("@MaSach", ct.MaSach);
                    cmd.Parameters.AddWithValue("@SoLuong", ct.SoLuong);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new SqlCommand(
                    @"UPDATE DauSach SET SoLuongCon = SoLuongCon - @SoLuong
                      WHERE MaSach = @MaSach AND SoLuongCon >= @SoLuong", conn, tran))
                {
                    cmd.Parameters.AddWithValue("@SoLuong", ct.SoLuong);
                    cmd.Parameters.AddWithValue("@MaSach", ct.MaSach);
                    if (cmd.ExecuteNonQuery() == 0)
                        throw new InvalidOperationException(
                            $"Sách mã {ct.MaSach} không đủ số lượng trong kho.");
                }
            }

            tran.Commit();
            return maPhieu;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    // ---------------------------------------------------------------
    // Trả sách: Parameterized UPDATE trạng thái -> "Đã trả"
    //           đồng thời cộng lại SoLuongCon (1 transaction)
    // ---------------------------------------------------------------
    public bool TraSach(int maPhieu)
    {
        using var conn = DbHelper.CreateConnection();
        conn.Open();
        using var tran = conn.BeginTransaction();
        try
        {
            int rows;
            using (var cmd = new SqlCommand(
                @"UPDATE PhieuMuon SET TrangThai = N'Đã trả'
                  WHERE MaPhieu = @MaPhieu AND TrangThai <> N'Đã trả'", conn, tran))
            {
                cmd.Parameters.AddWithValue("@MaPhieu", maPhieu);
                rows = cmd.ExecuteNonQuery();
            }

            if (rows == 0)
            {
                tran.Rollback();
                return false;   // phiếu không tồn tại hoặc đã trả rồi
            }

            using (var cmd = new SqlCommand(
                @"UPDATE ds SET ds.SoLuongCon = ds.SoLuongCon + ct.SoLuong
                  FROM DauSach ds
                  INNER JOIN ChiTietMuon ct ON ds.MaSach = ct.MaSach
                  WHERE ct.MaPhieu = @MaPhieu", conn, tran))
            {
                cmd.Parameters.AddWithValue("@MaPhieu", maPhieu);
                cmd.ExecuteNonQuery();
            }

            tran.Commit();
            return true;
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }

    /// <summary>Đồng bộ trạng thái: phiếu "Đang mượn" đã vượt hạn trả -> "Quá hạn".</summary>
    public int CapNhatQuaHan()
    {
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            @"UPDATE PhieuMuon SET TrangThai = N'Quá hạn'
              WHERE TrangThai = N'Đang mượn' AND HanTra < GETDATE()", conn);
        conn.Open();
        return cmd.ExecuteNonQuery();
    }

    // ---------------------------------------------------------------
    // Cảnh báo quá hạn: SqlDataReader, HanTra < GETDATE() AND TrangThai <> N'Đã trả'
    // ---------------------------------------------------------------
    public List<QuaHanDTO> GetQuaHan()
    {
        var list = new List<QuaHanDTO>();
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            @"SELECT p.MaPhieu, d.HoTen, d.Email, p.NgayMuon, p.HanTra,
                     DATEDIFF(DAY, p.HanTra, GETDATE()) AS SoNgayQuaHan, p.TrangThai
              FROM PhieuMuon p
              INNER JOIN DocGia d ON p.MaDocGia = d.MaDocGia
              WHERE p.HanTra < GETDATE() AND p.TrangThai <> N'Đã trả'
              ORDER BY p.HanTra", conn);
        conn.Open();
        using SqlDataReader rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new QuaHanDTO
            {
                MaPhieu = rd.GetInt32(0),
                HoTen = rd.GetString(1),
                Email = rd.IsDBNull(2) ? null : rd.GetString(2),
                NgayMuon = rd.GetDateTime(3),
                HanTra = rd.GetDateTime(4),
                SoNgayQuaHan = rd.GetInt32(5),
                TrangThai = rd.GetString(6)
            });
        }
        return list;
    }

    // ---------------------------------------------------------------
    // Thống kê lượt mượn theo thể loại (GROUP BY)
    // ---------------------------------------------------------------
    public List<ThongKeTheLoaiDTO> ThongKeTheoTheLoai()
    {
        var list = new List<ThongKeTheLoaiDTO>();
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            @"SELECT ISNULL(s.TheLoai, N'(Chưa phân loại)') AS TheLoai,
                     COUNT(*)        AS SoLuotMuon,
                     SUM(ct.SoLuong) AS TongSoCuon
              FROM ChiTietMuon ct
              INNER JOIN DauSach s ON ct.MaSach = s.MaSach
              GROUP BY ISNULL(s.TheLoai, N'(Chưa phân loại)')
              ORDER BY SoLuotMuon DESC", conn);
        conn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new ThongKeTheLoaiDTO
            {
                TheLoai = rd.GetString(0),
                SoLuotMuon = rd.GetInt32(1),
                TongSoCuon = rd.GetInt32(2)
            });
        }
        return list;
    }

    // ExecuteScalar: tổng số lượt mượn (tất cả thể loại)
    public int TongLuotMuon()
    {
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand("SELECT COUNT(*) FROM ChiTietMuon", conn);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    // ExecuteScalar: số lượt mượn của một thể loại
    public int DemLuotMuonTheoTheLoai(string theLoai)
    {
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            @"SELECT COUNT(*)
              FROM ChiTietMuon ct
              INNER JOIN DauSach s ON ct.MaSach = s.MaSach
              WHERE s.TheLoai = @TheLoai", conn);
        cmd.Parameters.AddWithValue("@TheLoai", theLoai);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar());
    }
}
