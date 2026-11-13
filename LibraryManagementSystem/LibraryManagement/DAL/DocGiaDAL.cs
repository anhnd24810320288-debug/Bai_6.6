using LibraryManagement.DTO;
using Microsoft.Data.SqlClient;

namespace LibraryManagement.DAL;

public class DocGiaDAL
{
    private static DocGiaDTO Map(SqlDataReader rd) => new DocGiaDTO
    {
        MaDocGia = rd.GetInt32(0),
        HoTen = rd.GetString(1),
        Email = rd.IsDBNull(2) ? null : rd.GetString(2),
        NgayHetHanThe = rd.GetDateTime(3)
    };

    public List<DocGiaDTO> GetAll()
    {
        var list = new List<DocGiaDTO>();
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            "SELECT MaDocGia, HoTen, Email, NgayHetHanThe FROM DocGia ORDER BY MaDocGia", conn);
        conn.Open();
        using var rd = cmd.ExecuteReader();
        while (rd.Read()) list.Add(Map(rd));
        return list;
    }

    public DocGiaDTO GetById(int maDocGia)
    {
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            "SELECT MaDocGia, HoTen, Email, NgayHetHanThe FROM DocGia WHERE MaDocGia = @MaDocGia", conn);
        cmd.Parameters.AddWithValue("@MaDocGia", maDocGia);
        conn.Open();
        using var rd = cmd.ExecuteReader();
        return rd.Read() ? Map(rd) : null;
    }

    public int Insert(DocGiaDTO dg)
    {
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            @"INSERT INTO DocGia (HoTen, Email, NgayHetHanThe)
              VALUES (@HoTen, @Email, @NgayHetHanThe)", conn);
        cmd.Parameters.AddWithValue("@HoTen", dg.HoTen);
        cmd.Parameters.AddWithValue("@Email", (object)dg.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@NgayHetHanThe", dg.NgayHetHanThe);
        conn.Open();
        return cmd.ExecuteNonQuery();
    }

    public int Update(DocGiaDTO dg)
    {
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            @"UPDATE DocGia
              SET HoTen = @HoTen, Email = @Email, NgayHetHanThe = @NgayHetHanThe
              WHERE MaDocGia = @MaDocGia", conn);
        cmd.Parameters.AddWithValue("@HoTen", dg.HoTen);
        cmd.Parameters.AddWithValue("@Email", (object)dg.Email ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@NgayHetHanThe", dg.NgayHetHanThe);
        cmd.Parameters.AddWithValue("@MaDocGia", dg.MaDocGia);
        conn.Open();
        return cmd.ExecuteNonQuery();
    }

    public int Delete(int maDocGia)
    {
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand("DELETE FROM DocGia WHERE MaDocGia = @MaDocGia", conn);
        cmd.Parameters.AddWithValue("@MaDocGia", maDocGia);
        conn.Open();
        return cmd.ExecuteNonQuery();
    }

    /// <summary>Thẻ còn hạn khi NgayHetHanThe >= GETDATE() (dùng ExecuteScalar).</summary>
    public bool IsCardValid(int maDocGia)
    {
        using var conn = DbHelper.CreateConnection();
        using var cmd = new SqlCommand(
            "SELECT COUNT(1) FROM DocGia WHERE MaDocGia = @MaDocGia AND NgayHetHanThe >= GETDATE()", conn);
        cmd.Parameters.AddWithValue("@MaDocGia", maDocGia);
        conn.Open();
        return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
    }
}
