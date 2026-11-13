using LibraryManagement.BUS;
using LibraryManagement.DTO;

namespace LibraryManagement.GUI;

public class UcMuonTra : UserControl, IReloadable
{
    private readonly PhieuMuonBUS phieuBus = new();
    private readonly DocGiaBUS docGiaBus = new();

    private readonly ComboBox cboDocGia = new() { Left = 85, Top = 15, Width = 240, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox cboTrangThai = new() { Left = 420, Top = 15, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView gridPhieu = UiHelper.CreateGrid();
    private readonly DataGridView gridChiTiet = UiHelper.CreateGrid();

    public UcMuonTra()
    {
        cboTrangThai.Items.AddRange(new object[] { "Tất cả", "Đang mượn", "Đã trả", "Quá hạn" });
        cboTrangThai.SelectedIndex = 0;

        var pnl = new Panel { Dock = DockStyle.Top, Height = 100 };
        pnl.Controls.AddRange(new Control[]
        {
            UiHelper.Label("Độc giả:", 15, 19), cboDocGia,
            UiHelper.Label("Trạng thái:", 345, 19), cboTrangThai,
            UiHelper.Button("Lọc", 590, 12, (s, e) => LoadPhieu(), 80),
            UiHelper.Button("Lập phiếu mượn", 15, 55, (s, e) => LapPhieu(), 140),
            UiHelper.Button("Trả sách", 165, 55, (s, e) => TraSach(), 120),
            UiHelper.Button("Làm mới", 295, 55, (s, e) => Reload(), 100)
        });
        cboDocGia.SelectionChangeCommitted += (s, e) => LoadPhieu();
        cboTrangThai.SelectionChangeCommitted += (s, e) => LoadPhieu();

        gridPhieu.SelectionChanged += (s, e) => LoadChiTiet();

        var grpPhieu = new GroupBox { Text = "Danh sách phiếu mượn", Dock = DockStyle.Fill };
        grpPhieu.Controls.Add(gridPhieu);
        var grpCt = new GroupBox { Text = "Chi tiết sách của phiếu đang chọn", Dock = DockStyle.Fill };
        grpCt.Controls.Add(gridChiTiet);

        var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
        table.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
        table.Controls.Add(grpPhieu, 0, 0);
        table.Controls.Add(grpCt, 0, 1);

        Controls.Add(table);
        Controls.Add(pnl);
    }

    public void Reload()
    {
        try
        {
            var ds = docGiaBus.GetAll();
            ds.Insert(0, new DocGiaDTO { MaDocGia = 0, HoTen = "-- Tất cả độc giả --" });

            var old = cboDocGia.SelectedValue;
            cboDocGia.DataSource = ds;
            cboDocGia.DisplayMember = "HoTen";
            cboDocGia.ValueMember = "MaDocGia";
            if (old is int id && ds.Any(d => d.MaDocGia == id))
                cboDocGia.SelectedValue = id;

            LoadPhieu();
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void LoadPhieu()
    {
        try
        {
            int? maDocGia = cboDocGia.SelectedValue is int v && v > 0 ? v : null;
            string trangThai = cboTrangThai.SelectedIndex > 0 ? cboTrangThai.Text : null;

            UiHelper.Bind(gridPhieu, phieuBus.Filter(maDocGia, trangThai),
                ("MaPhieu", "Mã phiếu"), ("HoTen", "Độc giả"),
                ("NgayMuon", "Ngày mượn"), ("HanTra", "Hạn trả"), ("TrangThai", "Trạng thái"));
            LoadChiTiet();
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void LoadChiTiet()
    {
        try
        {
            if (gridPhieu.CurrentRow?.DataBoundItem is PhieuMuonDTO p)
            {
                UiHelper.Bind(gridChiTiet, phieuBus.GetChiTiet(p.MaPhieu),
                    ("MaSach", "Mã sách"), ("TenSach", "Tên sách"), ("SoLuong", "Số lượng"));
            }
            else
            {
                gridChiTiet.DataSource = null;
            }
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void LapPhieu()
    {
        using var f = new FrmLapPhieu();
        if (f.ShowDialog(FindForm()) == DialogResult.OK)
            LoadPhieu();
    }

    private void TraSach()
    {
        try
        {
            if (gridPhieu.CurrentRow?.DataBoundItem is not PhieuMuonDTO p)
                throw new ArgumentException("Vui lòng chọn phiếu mượn cần trả.");
            if (p.TrangThai == "Đã trả")
                throw new InvalidOperationException("Phiếu này đã được trả rồi.");
            if (!UiHelper.Confirm($"Xác nhận trả sách cho phiếu #{p.MaPhieu} của {p.HoTen}?")) return;

            phieuBus.TraSach(p.MaPhieu);
            UiHelper.Info("Trả sách thành công, số lượng sách đã được cộng lại vào kho.");
            LoadPhieu();
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }
}
