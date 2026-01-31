using System.Windows.Forms;
using System.Drawing;

namespace KBShift.UI;

public class TrayIconController
{
    private NotifyIcon? _trayIcon;

    public void Initialize(Action onShow, Action onExit)
    {
        _trayIcon = new NotifyIcon
        {
            // Using a built-in system icon to bypass file path issues
            Icon = SystemIcons.Shield,
            Visible = true,
            Text = "KBShift Active"
        };

        _trayIcon.DoubleClick += (s, e) => onShow?.Invoke();

        var menu = new ContextMenuStrip();
        menu.Items.Add("Settings", null, (s, e) => onShow?.Invoke());
        menu.Items.Add("-");
        menu.Items.Add("Exit", null, (s, e) => {
            _trayIcon.Dispose();
            onExit?.Invoke();
        });
        _trayIcon.ContextMenuStrip = menu;
    }
}