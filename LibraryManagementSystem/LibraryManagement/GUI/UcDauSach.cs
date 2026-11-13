using LibraryManagement.BUS;
using LibraryManagement.DTO;

namespace LibraryManagement.GUI;

public class UcDauSach : UserControl, IReloadable
{
    private static readonly string[] DefaultTheLoai = { "Công nghệ thông tin", "Kinh tế", "Văn học" };

    private readonly DauSachBUS bus = new();
    private readonly TextBox txtTen = new() { Left = 85, Top = 15, Width = 300 };
    private readonly ComboBox cboTheLoai = new() { Left = 480, Top = 15, Width = 190, DropDownStyle = ComboBoxStyle.DropDown };
    private readonly NumericUpDown nudSoLuong = new() { Left = 775, Top = 15, Width = 70, Minimum = 0, Maximum = 100000 };
    private readonly DataGridView grid = UiHelper.CreateGrid();
    private int selectedId = 0;

    public UcDauSach()
    {
        var pnl = new Panel { Dock = DockStyle.Top, Height = 100 };
        pnl.Controls.AddRange(new Control[]
        {
            UiHelper.Label("Tên sách:", 15, 19), txtTen,
            UiHelper.Label("Thể loại:", 410, 19), cboTheLoai,
            UiHelper.Label("Số lượng:", 700, 19), nudSoLuong,
            UiHelper.Button("Thêm", 15, 55, (s, e) => Add()),
            UiHelper.Button("Sửa", 125, 55, (s, e) => Edit()),
            UiHelper.Button("Xóa", 235, 55, (s, e) => Remove()),
            UiHelper.Button("Làm mới", 345, 55, (s, e) => { ClearInput(); Reload(); })
        });

        grid.SelectionChanged += (s, e) => OnRowSelected();
        Controls.Add(grid);
        Controls.Add(pnl);
    }

    public void Reload()
    {
        try
        {
            // Gợi ý thể loại = mặc định + các thể loại đang có trong CSDL
            var current = cboTheLoai.Text;
            var theLoai = DefaultTheLoai.Union(bus.GetTheLoai()).OrderBy(x => x).ToList();
            cboTheLoai.DataSource = theLoai;
            cboTheLoai.Text = current;

            UiHelper.Bind(grid, bus.GetAll(),
                ("MaSach", "Mã sách"), ("TenSach", "Tên sách"),
                ("TheLoai", "Thể loại"), ("SoLuongCon", "Số lượng còn"));
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void OnRowSelected()
    {
        if (grid.CurrentRow?.DataBoundItem is DauSachDTO s)
        {
            selectedId = s.MaSach;
            txtTen.Text = s.TenSach;
            cboTheLoai.Text = s.TheLoai ?? "";
            nudSoLuong.Value = s.SoLuongCon;
        }
    }

    private DauSachDTO ReadInput() => new()
    {
        MaSach = selectedId,
        TenSach = txtTen.Text,
        TheLoai = cboTheLoai.Text,
        SoLuongCon = (int)nudSoLuong.Value
    };

    private void ClearInput()
    {
        selectedId = 0;
        txtTen.Clear();
        cboTheLoai.Text = "";
        nudSoLuong.Value = 0;
    }

    private void Add()
    {
        try
        {
            var s = ReadInput();
            s.MaSach = 0;
            bus.Add(s);
            UiHelper.Info("Đã thêm đầu sách.");
            ClearInput();
            Reload();
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void Edit()
    {
        try
        {
            bus.Update(ReadInput());
            UiHelper.Info("Đã cập nhật đầu sách.");
            Reload();
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void Remove()
    {
        try
        {
            if (selectedId <= 0) throw new ArgumentException("Vui lòng chọn đầu sách cần xóa.");
            if (!UiHelper.Confirm("Bạn có chắc muốn xóa đầu sách này?")) return;
            bus.Delete(selectedId);
            ClearInput();
            Reload();
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }
}
