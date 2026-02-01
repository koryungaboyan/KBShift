using System;
using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;

namespace KBShift.UI
{

public class TrayIconController
{
    private NotifyIcon _trayIcon;

    public void Initialize(Action onShow, Action onExit)
    {
        Icon appIcon = null;
        try
        {
            appIcon = Icon.ExtractAssociatedIcon(Process.GetCurrentProcess().MainModule?.FileName ?? "");
        }
        catch { }

        _trayIcon = new NotifyIcon
        {
            Icon = appIcon ?? SystemIcons.Shield,
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
}