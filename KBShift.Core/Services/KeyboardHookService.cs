using KBShift.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace KBShift.Core.Services
{

public class KeyboardHookService : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYUP = 0x0105;
    private const int WM_INPUTLANGCHANGEREQUEST = 0x0050;

    private const int VK_CAPITAL = 0x14;
    private const int VK_SHIFT = 0x10;
    private const int VK_CONTROL = 0x11;
    private const int VK_LSHIFT = 0xA0;
    private const int VK_RSHIFT = 0xA1;
    private const int VK_LCONTROL = 0xA2;
    private const int VK_RCONTROL = 0xA3;
    private const int VK_LMENU = 0xA4;
    private const int VK_RMENU = 0xA5;

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
    private LowLevelKeyboardProc _proc;
    private IntPtr _hookID = IntPtr.Zero;

    // Thread-safe access to configuration
    private readonly object _configLock = new object();
    private List<InputLanguage> _leftLangs = new List<InputLanguage>();
    private List<InputLanguage> _rightLangs = new List<InputLanguage>();
    private ShortcutType _group1Trigger = ShortcutType.LeftAltShift;
    private ShortcutType _group2Trigger = ShortcutType.RightAltShift;
    
    public List<InputLanguage> LeftLangs 
    { 
        get { lock (_configLock) { return _leftLangs.ToList(); } }
        set { lock (_configLock) { _leftLangs = value ?? new List<InputLanguage>(); } }
    }
    public List<InputLanguage> RightLangs 
    { 
        get { lock (_configLock) { return _rightLangs.ToList(); } }
        set { lock (_configLock) { _rightLangs = value ?? new List<InputLanguage>(); } }
    }
    
    public ShortcutType Group1Trigger 
    { 
        get { lock (_configLock) { return _group1Trigger; } }
        set { lock (_configLock) { _group1Trigger = value; } }
    }
    public ShortcutType Group2Trigger 
    { 
        get { lock (_configLock) { return _group2Trigger; } }
        set { lock (_configLock) { _group2Trigger = value; } }
    }

    public KeyboardHookService()
    {
        _proc = HookCallback;
        _hookID = SetHook(_proc);
    }

    public void Dispose()
    {
        UnhookWindowsHookEx(_hookID);
    }

    private IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (var curProcess = Process.GetCurrentProcess())
        {
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(IntPtr.Zero), 0);
            }
        }
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0)
        {
            try
            {
                int vkCode = Marshal.ReadInt32(lParam);
                bool isDown = wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN;

                // Only check triggers on KeyDown
                if (isDown)
                {
                    if (CheckTrigger(vkCode))
                    {
                        // IF we handled the trigger, we return 1 to SUPPRESS the key
                        // preventing Windows from seeing it and switching language itself.
                        // We also send a dummy key inside CheckTrigger to cancel the Menu activation.
                        return (IntPtr)1;
                    }
                }
            }
            catch (Exception)
            {
                // Swallow errors to prevent hook crashes
            }
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    private bool CheckTrigger(int vkCode)
    {
        // Helper to match trigger and fire action
        
        // Get thread-safe copies of configuration
        ShortcutType trigger1, trigger2;
        List<InputLanguage> langs1, langs2;
        lock (_configLock)
        {
            trigger1 = _group1Trigger;
            trigger2 = _group2Trigger;
            langs1 = _leftLangs.ToList();
            langs2 = _rightLangs.ToList();
        }
        
        // Group 1
        if (IsTriggerMatch(trigger1, vkCode))
        {
             if (langs1.Any()) 
             {
                 CycleLanguage(langs1);
                 // If trigger used Alt, send dummy key to prevent menu bar activation
                 if (IsAltTrigger(trigger1)) 
                 {
                     SendDummyKey();
                 }
                 return true; // Suppress
             }
        }

        // Group 2
        if (IsTriggerMatch(trigger2, vkCode))
        {
             if (langs2.Any()) 
             {
                 CycleLanguage(langs2);
                 // If trigger used Alt, send dummy key to prevent menu bar activation
                 if (IsAltTrigger(trigger2))
                 {
                     SendDummyKey();
                 }
                 return true; // Suppress
             }
        }

        return false;
    }
    
    private bool IsAltTrigger(ShortcutType type)
    {
        return type == ShortcutType.LeftAltShift || type == ShortcutType.RightAltShift;
    }

    private void SendDummyKey()
    {
        // Send a Control key tap (vk=0x11) to cancel the "Alt" menu sequence.
        // This is safe because Ctrl+Alt or Ctrl+Shift+Alt doesn't trigger the menu.
        keybd_event(0x11, 0, 0, UIntPtr.Zero); // Down
        keybd_event(0x11, 0, 0x0002, UIntPtr.Zero); // Up
    }

    private bool IsTriggerMatch(ShortcutType type, int vkCode)
    {
        // Stateless check using GetAsyncKeyState
        
        // Caps Lock
        if (type == ShortcutType.CapsLock)
        {
            return vkCode == VK_CAPITAL;
        }

        // Left Alt + Shift
        if (type == ShortcutType.LeftAltShift)
        {
            // Case 1: Left Alt Held, Shift Pressed
            // Check if vkCode is Shift, and Left Alt is currently down
            if ((vkCode == VK_LSHIFT || vkCode == VK_RSHIFT) && IsKeyDown(VK_LMENU)) return true;
            
            // Case 2: Shift Held, Left Alt Pressed
            // Check if vkCode is Left Alt, and ANY Shift is currently down
            if (vkCode == VK_LMENU && IsKeyDown(VK_SHIFT)) return true;
            
            return false;
        }

        // Right Alt + Shift
        if (type == ShortcutType.RightAltShift)
        {
            // Case 1: Right Alt Held, Shift Pressed
            if ((vkCode == VK_LSHIFT || vkCode == VK_RSHIFT) && IsKeyDown(VK_RMENU)) return true;
            
            // Case 2: Shift Held, Right Alt Pressed
            if (vkCode == VK_RMENU && IsKeyDown(VK_SHIFT)) return true;
            
            return false;
        }

        // Ctrl + Shift
        if (type == ShortcutType.CtrlShift)
        {
             // Case 1: Ctrl Held, Shift Pressed
             // Use generic VK_CONTROL (0x11) for holding check to be permissive
             if ((vkCode == VK_LSHIFT || vkCode == VK_RSHIFT) && IsKeyDown(VK_CONTROL)) return true;
             
             // Case 2: Shift Held, Ctrl Pressed
             if ((vkCode == VK_LCONTROL || vkCode == VK_RCONTROL) && IsKeyDown(VK_SHIFT)) return true;
             
             return false;
        }
        
        return false;
    }

    private bool IsKeyDown(int vKey)
    {
        // & 0x8000 checks if the key is currently down
        return (GetAsyncKeyState(vKey) & 0x8000) != 0;
    }

    private void CycleLanguage(List<InputLanguage> targetList)
    {
        var targetHwnd = GetForegroundWindow();
        if (targetHwnd == IntPtr.Zero) return;

        var threadId = GetWindowThreadProcessId(targetHwnd, out _);
        var currentLayoutHandle = GetKeyboardLayout(threadId);
        
        int currentIndex = -1;
        for (int i = 0; i < targetList.Count; i++)
        {
             if (targetList[i].Handle == currentLayoutHandle)
             {
                 currentIndex = i;
                 break;
             }
        }

        InputLanguage targetLang;
        if (currentIndex >= 0)
        {
            var nextIndex = (currentIndex + 1) % targetList.Count;
            targetLang = targetList[nextIndex];
        }
        else
        {
            targetLang = targetList[0];
        }

        // Fix for Win+R and similar: Send to focused control if possible
        var focusHwnd = GetFocusedHandle(threadId);
        if (focusHwnd != IntPtr.Zero)
        {
            PostMessage(focusHwnd, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, targetLang.Handle);
        }
        
        // Also send to main window as fallback
        PostMessage(targetHwnd, WM_INPUTLANGCHANGEREQUEST, IntPtr.Zero, targetLang.Handle);
    }

    private IntPtr GetFocusedHandle(uint threadId)
    {
        var info = new GUITHREADINFO();
        info.cbSize = (uint)Marshal.SizeOf(typeof(GUITHREADINFO));
        if (GetGUIThreadInfo(threadId, ref info))
        {
            return info.hwndFocus;
        }
        return IntPtr.Zero;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct GUITHREADINFO
    {
        public uint cbSize;
        public uint flags;
        public IntPtr hwndActive;
        public IntPtr hwndFocus;
        public IntPtr hwndCapture;
        public IntPtr hwndMenuOwner;
        public IntPtr hwndMoveSize;
        public IntPtr hwndCaret;
        public RECT rcCaret;
    }

    [DllImport("user32.dll")]
    private static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO lpgui);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(IntPtr lpModuleName);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern IntPtr GetKeyboardLayout(uint idThread);

    [DllImport("user32.dll")]
    private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int vKey);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);
    }
}
