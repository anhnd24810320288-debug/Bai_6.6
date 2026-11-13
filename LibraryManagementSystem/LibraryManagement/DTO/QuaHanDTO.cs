namespace LibraryManagement.DTO;

/// <summary>Dòng hiển thị trong danh sách cảnh báo sách quá hạn.</summary>
public class QuaHanDTO
{
    public int MaPhieu { get; set; }
    public string HoTen { get; set; }
    public string Email { get; set; }
    public DateTime NgayMuon { get; set; }
    public DateTime HanTra { get; set; }
    public int SoNgayQuaHan { get; set; }
    public string TrangThai { get; set; }
}
