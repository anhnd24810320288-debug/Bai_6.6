using LibraryManagement.DAL;
using LibraryManagement.DTO;
using Microsoft.Data.SqlClient;

namespace LibraryManagement.BUS;

public class DauSachBUS
{
    private readonly DauSachDAL dal = new();

    public List<DauSachDTO> GetAll() => dal.GetAll();

    public List<string> GetTheLoai() => dal.GetTheLoai();

    public void Add(DauSachDTO s)
    {
        Validate(s);
        dal.Insert(s);
    }

    public void Update(DauSachDTO s)
    {
        if (s.MaSach <= 0) throw new ArgumentException("Vui lòng chọn đầu sách cần sửa.");
        Validate(s);
        if (dal.Update(s) == 0) throw new InvalidOperationException("Không tìm thấy đầu sách.");
    }

    public void Delete(int maSach)
    {
        if (maSach <= 0) throw new ArgumentException("Vui lòng chọn đầu sách cần xóa.");
        try { dal.Delete(maSach); }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new InvalidOperationException("Không thể xóa: đầu sách này đã có trong phiếu mượn.");
        }
    }

    private static void Validate(DauSachDTO s)
    {
        if (string.IsNullOrWhiteSpace(s.TenSach))
            throw new ArgumentException("Tên sách không được để trống.");
        if (s.SoLuongCon < 0)
            throw new ArgumentException("Số lượng còn phải >= 0.");
        s.TenSach = s.TenSach.Trim();
        s.TheLoai = string.IsNullOrWhiteSpace(s.TheLoai) ? null : s.TheLoai.Trim();
    }
}
