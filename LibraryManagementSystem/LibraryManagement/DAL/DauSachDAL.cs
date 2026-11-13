using LibraryManagement.DTO;
using Microsoft.Data.SqlClient;

namespace LibraryManagement.DAL;

public class DauSachDAL
{
    public List<DauSachDTO> GetAll()
    {
        var list = new List<DauSachDTO>();
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            "SELECT MaSach, TenSach, TheLoai, SoLuongCon FROM DauSach ORDER BY MaSach", conn);
        conn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read())
        {
            list.Add(new DauSachDTO
            {
                MaSach = rd.GetInt32(0),
                TenSach = rd.GetString(1),
                TheLoai = rd.IsDBNull(2) ? null : rd.GetString(2),
                SoLuongCon = rd.GetInt32(3)
            });
        }
        return list;
    }

    public List<string> GetTheLoai()
    {
        var list = new List<string>();
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            "SELECT DISTINCT TheLoai FROM DauSach WHERE TheLoai IS NOT NULL ORDER BY TheLoai", conn);
        conn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read()) list.Add(rd.GetString(0));
        return list;
    }

    public int Insert(DauSachDTO s)
    {
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            @"INSERT INTO DauSach (TenSach, TheLoai, SoLuongCon)
              VALUES (@TenSach, @TheLoai, @SoLuongCon)", conn);
        cmd.Parameters.AddWithValue("@TenSach", s.TenSach);
        cmd.Parameters.AddWithValue("@TheLoai", (object)s.TheLoai ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SoLuongCon", s.SoLuongCon);
        conn.Open();
        return cmd.ExecuteNonQuery();
    }

    public int Update(DauSachDTO s)
    {
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            @"UPDATE DauSach
              SET TenSach = @TenSach, TheLoai = @TheLoai, SoLuongCon = @SoLuongCon
              WHERE MaSach = @MaSach", conn);
        cmd.Parameters.AddWithValue("@TenSach", s.TenSach);
        cmd.Parameters.AddWithValue("@TheLoai", (object)s.TheLoai ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@SoLuongCon", s.SoLuongCon);
        cmd.Parameters.AddWithValue("@MaSach", s.MaSach);
        conn.Open();
        return cmd.ExecuteNonQuery();
    }

    public int Delete(int maSach)
    {
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand("DELETE FROM DauSach WHERE MaSach = @MaSach", conn);
        cmd.Parameters.AddWithValue("@MaSach", maSach);
        conn.Open();
        return cmd.ExecuteNonQuery();
    }
}
