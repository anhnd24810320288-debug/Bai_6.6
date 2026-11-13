using LibraryManagement.BUS;
using LibraryManagement.DTO;

namespace LibraryManagement.GUI;

public class UcDocGia : UserControl, IReloadable
{
    private readonly DocGiaBUS bus = new();
    private readonly TextBox txtHoTen = new() { Left = 85, Top = 15, Width = 230 };
    private readonly TextBox txtEmail = new() { Left = 385, Top = 15, Width = 230 };
    private readonly DateTimePicker dtpHan = new()
    {
        Left = 700, Top = 15, Width = 130,
        Format = DateTimePickerFormat.Short,
        Value = DateTime.Today.AddYears(1)
    };
    private readonly DataGridView grid = UiHelper.CreateGrid();
    private int selectedId = 0;

    public UcDocGia()
    {
        var pnl = new Panel { Dock = DockStyle.Top, Height = 100 };
        pnl.Controls.AddRange(new Control[]
        {
            UiHelper.Label("Họ tên:", 15, 19), txtHoTen,
            UiHelper.Label("Email:", 335, 19), txtEmail,
            UiHelper.Label("Hạn thẻ:", 635, 19), dtpHan,
            UiHelper.Button("Thêm", 15, 55, (s, e) => Add()),
            UiHelper.Button("Sửa", 125, 55, (s, e) => Edit()),
            UiHelper.Button("Xóa", 235, 55, (s, e) => Remove()),
            UiHelper.Button("Làm mới", 345, 55, (s, e) => { ClearInput(); Reload(); })
        });

        grid.SelectionChanged += (s, e) => OnRowSelected();
        // Thêm control Fill trước, control Top sau để docking đúng thứ tự
        Controls.Add(grid);
        Controls.Add(pnl);
    }

    public void Reload()
    {
        try
        {
            UiHelper.Bind(grid, bus.GetAll(),
                ("MaDocGia", "Mã độc giả"), ("HoTen", "Họ tên"),
                ("Email", "Email"), ("NgayHetHanThe", "Ngày hết hạn thẻ"));
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void OnRowSelected()
    {
        if (grid.CurrentRow?.DataBoundItem is DocGiaDTO dg)
        {
            selectedId = dg.MaDocGia;
            txtHoTen.Text = dg.HoTen;
            txtEmail.Text = dg.Email;
            dtpHan.Value = dg.NgayHetHanThe < dtpHan.MinDate ? DateTime.Today : dg.NgayHetHanThe;
        }
    }

    private DocGiaDTO ReadInput() => new()
    {
        MaDocGia = selectedId,
        HoTen = txtHoTen.Text,
        Email = txtEmail.Text,
        // Hết hạn vào cuối ngày được chọn để so sánh với GETDATE() cho đúng
        NgayHetHanThe = dtpHan.Value.Date.AddHours(23).AddMinutes(59).AddSeconds(59)
    };

    private void ClearInput()
    {
        selectedId = 0;
        txtHoTen.Clear();
        txtEmail.Clear();
        dtpHan.Value = DateTime.Today.AddYears(1);
    }

    private void Add()
    {
        try
        {
            var dg = ReadInput();
            dg.MaDocGia = 0;
            bus.Add(dg);
            UiHelper.Info("Đã thêm độc giả.");
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
            UiHelper.Info("Đã cập nhật độc giả.");
            Reload();
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }

    private void Remove()
    {
        try
        {
            if (selectedId <= 0) throw new ArgumentException("Vui lòng chọn độc giả cần xóa.");
            if (!UiHelper.Confirm("Bạn có chắc muốn xóa độc giả này?")) return;
            bus.Delete(selectedId);
            ClearInput();
            Reload();
        }
        catch (Exception ex) { UiHelper.Error(ex); }
    }
}
