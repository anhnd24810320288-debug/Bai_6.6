using LibraryManagement.DAL;
using LibraryManagement.DTO;
using Microsoft.Data.SqlClient;

namespace LibraryManagement.BUS;

public class DocGiaBUS
{
    private readonly DocGiaDAL dal = new();

    public List<DocGiaDTO> GetAll() => dal.GetAll();

    public void Add(DocGiaDTO dg)
    {
        Validate(dg);
        try { dal.Insert(dg); }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new InvalidOperationException("Email này đã được đăng ký cho độc giả khác.");
        }
    }

    public void Update(DocGiaDTO dg)
    {
        if (dg.MaDocGia <= 0) throw new ArgumentException("Vui lòng chọn độc giả cần sửa.");
        Validate(dg);
        try
        {
            if (dal.Update(dg) == 0) throw new InvalidOperationException("Không tìm thấy độc giả.");
        }
        catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601)
        {
            throw new InvalidOperationException("Email này đã được đăng ký cho độc giả khác.");
        }
    }

    public void Delete(int maDocGia)
    {
        if (maDocGia <= 0) throw new ArgumentException("Vui lòng chọn độc giả cần xóa.");
        try { dal.Delete(maDocGia); }
        catch (SqlException ex) when (ex.Number == 547)
        {
            throw new InvalidOperationException("Không thể xóa: độc giả này đã có phiếu mượn.");
        }
    }

    private static void Validate(DocGiaDTO dg)
    {
        if (string.IsNullOrWhiteSpace(dg.HoTen))
            throw new ArgumentException("Họ tên không được để trống.");
        if (string.IsNullOrWhiteSpace(dg.Email) || !dg.Email.Contains('@'))
            throw new ArgumentException("Email không hợp lệ.");
        dg.HoTen = dg.HoTen.Trim();
        dg.Email = dg.Email.Trim();
    }
}
