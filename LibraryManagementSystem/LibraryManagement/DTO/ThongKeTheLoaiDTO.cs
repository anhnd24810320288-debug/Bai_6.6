namespace LibraryManagement.DTO;

public class ThongKeTheLoaiDTO
{
    public string TheLoai { get; set; }
    public int SoLuotMuon { get; set; }   // số dòng chi tiết mượn
    public int TongSoCuon { get; set; }   // tổng số cuốn đã mượn
}
