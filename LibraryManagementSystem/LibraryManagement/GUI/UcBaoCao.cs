using LibraryManagement.BUS;

namespace LibraryManagement.GUI;

public class UcBaoCao : UserControl, IReloadable
{
    private readonly PhieuMuonBUS phieuBus = new();
    private readonly DauSachBUS dauSachBus = new();

    private readonly DataGridView gridQuaHan = UiHelper.CreateGrid();
    private readonly DataGridView gridThongKe = UiHelper.CreateGrid();
    private readonly Label lblQuaHan = new() { Left = 340, Top = 22, AutoSize = true, ForeColor = Color.Firebrick };
    private readonly Label lblTong = new() { Left = 15, Top = 12, AutoSize = true, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold) };
    private readonly ComboBox cboTheLoai = new() { Left = 330, Top = 8, Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label lblKetQua = new() { Left = 650, Top = 12, AutoSize = true };

    public UcBaoCao()
    {
        // ----- thanh trên cùng -----
        var pnlTop = new Panel { Dock = DockStyle.Top, Height = 60 };
        pnlTop.Controls.AddRange(new Control[]
        {
            UiHelper.Button("Làm mới", 15, 15, (s, e) => Reload(), 100),
            UiHelper.Button("Cập nhật trạng thái Quá hạn", 125, 15, (s, e) => CapNhatQuaHan(), 200),
            lblQuaHan
        });

        // ----- nhóm: phiếu quá hạn -----
        gridQuaHan.DataBindingComplete += (s, e) =>
        {
            foreach (DataGridViewRow r in gridQuaHan.Rows)
                r.DefaultCellStyle.ForeColor = Color.Firebrick;
        };
        var grpQuaHan = new GroupBox { Text = "Cảnh báo: phiếu mượn đã quá hạn trả (HanTra < hiện tại và chưa trả)", Dock = DockStyle.Fill };
        grpQuaHan.Controls.Add(gridQuaHan);

        // ----- nhóm: thống kê -----
        var pnlThongKe = new Panel { Dock = DockStyle.Bottom, Height = 45 };
        pnlThongKe.Controls.AddRange(new Control[]
        {
            lblTong,
            UiHelper.Label("Thể loại:", 265, 14), cboTheLoai,
            UiHelper.Button("Đếm lượt mượn", 540, 8, (s, e) => DemTheoTheLoai(), 100),
            lblKetQua
        });
        // lblKetQua nằm sau nút, dịch sang phải
        lblKetQua.Left = 655;
        lblKetQua.Top = 14;

        var grpThongKe = new GroupBox { Text = "Thống kê số lượt mượn theo thể loại sách", Dock = DockStyle.Fill };
        grpThongKe.Controls.Add(gridThongKe);
        grpThongKe.Controls.Add(pnlThongKe);

        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        table.Controls.Add(grpQuaHan, 0, 0);
        table.Controls.Add(grpThongKe, 0, 1);

        Controls.Add(table);
        Controls.Add(pnlTop);
    }

    public void Reload()
    {
        try
        {
            // Danh sách quá hạn (SqlDataReader)
            var quaHan = phieuBus.GetQuaHan();
            UiHelper.Bind(gridQuaHan, quaHan,
                ("MaPhieu", "Mã phiếu"), ("HoTen", "Độc giả"), ("Email", "Email"),
                ("NgayMuon", "Ngày mượn"), ("HanTra", "Hạn trả"),
                ("SoNgayQuaHan", "Số ngày quá hạn"), ("TrangThai", "Trạng thái"));
            lblQuaHan.Text = quaHan.Count == 0
                ? "Không có phiếu nào quá hạn."
                : $"Có {quaHan.Count} phiếu mượn quá hạn!";

            // Thống kê (GROUP BY) + tổng lượt mượn (ExecuteScalar)
            UiHelper.Bind(gridThongKe, phieuBus.ThongKeTheoTheLoai(),
                ("TheLoai", "Thể loại"), ("SoLuotMuon", "Số lượt mượn"), ("TongSoCuon", "Tổng số cuốn đã mượn"));
            lblTong.Text = $"Tổng số lượt mượn: {phieuBus.TongLuotMuon()}";

            var old = cboTheLoai.Text;
            cboTheLoai.DataSource = dauSachBus.GetTheLoai();
            if (!string.IsNullOrEmpty(old)) cboTheLoai.Text = old;
            lblKetQua.Text = "";
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void CapNhatQuaHan()
    {
        try
        {
            int n = phieuBus.CapNhatQuaHan();
            UiHelper.Info(n == 0
                ? "Không có phiếu nào cần chuyển sang trạng thái Quá hạn."
                : $"Đã chuyển {n} phiếu sang trạng thái Quá hạn.");
            Reload();
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void DemTheoTheLoai()
    {
        try
        {
            string theLoai = cboTheLoai.Text;
            int n = phieuBus.DemLuotMuonTheoTheLoai(theLoai);   // ExecuteScalar
            lblKetQua.Text = $"\"{theLoai}\": {n} lượt mượn";
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }
}
