using System.Runtime.InteropServices;

namespace KBShift.Core.Services;

public class HotkeyService
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

    private const uint MOD_ALT = 0x0001;
    private const uint MOD_SHIFT = 0x0004;
    private const uint MOD_NOREPEAT = 0x4000;

    public void Register(IntPtr handle)
    {
        RegisterHotKey(handle, 9001, MOD_ALT | MOD_SHIFT | MOD_NOREPEAT, 0x00);
        RegisterHotKey(handle, 9002, MOD_ALT | MOD_SHIFT | MOD_NOREPEAT, 0x00);
    }
}