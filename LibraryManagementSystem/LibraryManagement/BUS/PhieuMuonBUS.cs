using LibraryManagement.DAL;
using LibraryManagement.DTO;

namespace LibraryManagement.BUS;

public class PhieuMuonBUS
{
    private readonly PhieuMuonDAL dal = new();
    private readonly DocGiaDAL docGiaDal = new();

    /// <summary>Lọc phiếu theo độc giả / trạng thái. Trước khi lọc, đồng bộ trạng thái "Quá hạn".</summary>
    public List<PhieuMuonDTO> Filter(int? maDocGia, string trangThai)
    {
        dal.CapNhatQuaHan();
        return dal.GetByFilter(maDocGia, trangThai);
    }

    public List<ChiTietMuonDTO> GetChiTiet(int maPhieu) => dal.GetChiTiet(maPhieu);

    /// <summary>Lập phiếu mượn mới. Kiểm tra hạn thẻ (NgayHetHanThe >= GETDATE()) trước.</summary>
    public int LapPhieu(PhieuMuonDTO p)
    {
        if (p.MaDocGia <= 0)
            throw new ArgumentException("Vui lòng chọn độc giả.");
        if (p.ChiTiet == null || p.ChiTiet.Count == 0)
            throw new ArgumentException("Phiếu mượn phải có ít nhất một đầu sách.");
        if (p.HanTra.Date < DateTime.Today)
            throw new ArgumentException("Hạn trả không được nhỏ hơn ngày hiện tại.");
        if (p.ChiTiet.Any(c => c.SoLuong <= 0))
            throw new ArgumentException("Số lượng mượn phải lớn hơn 0.");

        // Nghiệp vụ: thẻ phải còn hạn mới được lập phiếu
        if (!docGiaDal.IsCardValid(p.MaDocGia))
            throw new InvalidOperationException("Thẻ độc giả đã hết hạn, không thể lập phiếu mượn mới.");

        // Gộp các dòng trùng mã sách (khóa chính của ChiTietMuon là (MaPhieu, MaSach))
        p.ChiTiet = p.ChiTiet
            .GroupBy(c => c.MaSach)
            .Select(g => new ChiTietMuonDTO { MaSach = g.Key, SoLuong = g.Sum(x => x.SoLuong) })
            .ToList();

        return dal.Insert(p);
    }

    public void TraSach(int maPhieu)
    {
        if (maPhieu <= 0) throw new ArgumentException("Vui lòng chọn phiếu mượn.");
        if (!dal.TraSach(maPhieu))
            throw new InvalidOperationException("Phiếu không tồn tại hoặc đã được trả trước đó.");
    }

    public List<QuaHanDTO> GetQuaHan() => dal.GetQuaHan();

    public int CapNhatQuaHan() => dal.CapNhatQuaHan();

    public List<ThongKeTheLoaiDTO> ThongKeTheoTheLoai() => dal.ThongKeTheoTheLoai();

    public int TongLuotMuon() => dal.TongLuotMuon();

    public int DemLuotMuonTheoTheLoai(string theLoai)
    {
        if (string.IsNullOrWhiteSpace(theLoai)) throw new ArgumentException("Vui lòng chọn thể loại.");
        return dal.DemLuotMuonTheoTheLoai(theLoai);
    }
}
