namespace LibraryManagement.DTO;

public class PhieuMuonDTO
{
    public int MaPhieu { get; set; }
    public int MaDocGia { get; set; }
    public string HoTen { get; set; }     // chỉ dùng để hiển thị (JOIN DocGia)
    public DateTime NgayMuon { get; set; }
    public DateTime HanTra { get; set; }
    public string TrangThai { get; set; }
    public List<ChiTietMuonDTO> ChiTiet { get; set; } = new();
}
