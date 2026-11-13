using System.ComponentModel;
using LibraryManagement.BUS;
using LibraryManagement.DTO;

namespace LibraryManagement.GUI;

/// <summary>Dialog lập phiếu mượn mới (chọn độc giả, hạn trả, các đầu sách).</summary>
public class FrmLapPhieu : Form
{
    private readonly PhieuMuonBUS phieuBus = new();
    private readonly DocGiaBUS docGiaBus = new();
    private readonly DauSachBUS dauSachBus = new();

    private readonly ComboBox cboDocGia = new() { Left = 90, Top = 15, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker dtpHanTra = new()
    {
        Left = 90, Top = 50, Width = 140,
        Format = DateTimePickerFormat.Short,
        Value = DateTime.Today.AddDays(14)
    };
    private readonly ComboBox cboSach = new() { Left = 90, Top = 95, Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown nudSoLuong = new() { Left = 460, Top = 95, Width = 60, Minimum = 1, Maximum = 1000, Value = 1 };
    private readonly DataGridView grid = UiHelper.CreateGrid();
    private readonly BindingList<ChiTietMuonDTO> gio = new();
    private List<DauSachDTO> dsSach = new();

    public FrmLapPhieu()
    {
        Text = "Lập phiếu mượn sách";
        Size = new Size(640, 520);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        dtpHanTra.MinDate = DateTime.Today;

        var pnl = new Panel { Dock = DockStyle.Top, Height = 190 };
        pnl.Controls.AddRange(new Control[]
        {
            UiHelper.Label("Độc giả:", 15, 19), cboDocGia,
            UiHelper.Label("Hạn trả:", 15, 54), dtpHanTra,
            UiHelper.Label("Đầu sách:", 15, 99), cboSach,
            UiHelper.Label("SL:", 420, 99), nudSoLuong,
            UiHelper.Button("Thêm vào phiếu", 90, 135, (s, e) => ThemSach(), 130),
            UiHelper.Button("Bỏ dòng chọn", 230, 135, (s, e) => BoSach(), 120)
        });

        var pnlBottom = new Panel { Dock = DockStyle.Bottom, Height = 55 };
        pnlBottom.Controls.AddRange(new Control[]
        {
            UiHelper.Button("Lưu phiếu", 400, 12, (s, e) => Luu(), 100),
            UiHelper.Button("Hủy", 510, 12, (s, e) => DialogResult = DialogResult.Cancel, 90)
        });

        Controls.Add(grid);
        Controls.Add(pnl);
        Controls.Add(pnlBottom);

        Shown += (s, e) => LoadData();
    }

    private void LoadData()
    {
        try
        {
            cboDocGia.DataSource = docGiaBus.GetAll();
            cboDocGia.DisplayMember = "HoTen";
            cboDocGia.ValueMember = "MaDocGia";

            RefreshSach();
            UiHelper.Bind(grid, gio,
                ("MaSach", "Mã sách"), ("TenSach", "Tên sách"), ("SoLuong", "Số lượng"));
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void RefreshSach()
    {
        dsSach = dauSachBus.GetAll().Where(s => s.SoLuongCon > 0).ToList();
        cboSach.DataSource = null;
        cboSach.DataSource = dsSach.Select(s => new
        {
            s.MaSach,
            Hien = $"{s.TenSach}  (còn {s.SoLuongCon})"
        }).ToList();
        cboSach.DisplayMember = "Hien";
        cboSach.ValueMember = "MaSach";
    }

    private void ThemSach()
    {
        if (cboSach.SelectedValue is not int maSach) return;
        var sach = dsSach.First(s => s.MaSach == maSach);
        int sl = (int)nudSoLuong.Value;

        var existing = gio.FirstOrDefault(c => c.MaSach == maSach);
        int tong = sl + (existing?.SoLuong ?? 0);
        if (tong > sach.SoLuongCon)
        {
            UiHelper.Info($"Chỉ còn {sach.SoLuongCon} cuốn \"{sach.TenSach}\" trong kho.");
            return;
        }

        if (existing != null)
        {
            existing.SoLuong = tong;
            gio.ResetBindings();
        }
        else
        {
            gio.Add(new ChiTietMuonDTO { MaSach = maSach, TenSach = sach.TenSach, SoLuong = sl });
        }
    }

    private void BoSach()
    {
        if (grid.CurrentRow?.DataBoundItem is ChiTietMuonDTO ct)
            gio.Remove(ct);
    }

    private void Luu()
    {
        try
        {
            if (cboDocGia.SelectedValue is not int maDocGia)
                throw new ArgumentException("Vui lòng chọn độc giả.");

            var phieu = new PhieuMuonDTO
            {
                MaDocGia = maDocGia,
                // hạn trả tính đến hết ngày được chọn
                HanTra = dtpHanTra.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                ChiTiet = gio.ToList()
            };

            int ma = phieuBus.LapPhieu(phieu);
            UiHelper.Info($"Đã lập phiếu mượn #{ma}.");
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }
}
