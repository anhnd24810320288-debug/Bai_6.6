namespace LibraryManagement.GUI;

public class FrmMain : Form
{
    private readonly TabControl tab = new() { Dock = DockStyle.Fill };

    public FrmMain()
    {
        Text = "Quản lý mượn trả sách thư viện";
        Size = new Size(1100, 680);
        MinimumSize = new Size(900, 560);
        StartPosition = FormStartPosition.CenterScreen;

        AddTab("Độc giả", new UcDocGia());
        AddTab("Đầu sách", new UcDauSach());
        AddTab("Mượn / Trả sách", new UcMuonTra());
        AddTab("Quá hạn & Thống kê", new UcBaoCao());
        Controls.Add(tab);

        tab.SelectedIndexChanged += (s, e) => ReloadCurrent();
        Shown += (s, e) => ReloadCurrent();
    }

    private void AddTab(string title, Control uc)
    {
        uc.Dock = DockStyle.Fill;
        var page = new TabPage(title);
        page.Controls.Add(uc);
        tab.TabPages.Add(page);
    }

    private void ReloadCurrent()
    {
        if (tab.SelectedTab is { Controls.Count: > 0 } page && page.Controls[0] is IReloadable r)
            r.Reload();
    }
}
