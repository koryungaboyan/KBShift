using System.Windows;
using System.Threading;
using WinForms = System.Windows.Forms;

namespace KBShift.UI
{

public partial class App : System.Windows.Application
{
    private static Mutex _mutex;
    private static bool _ownsMutex;

    protected override void OnStartup(StartupEventArgs e)
    {
        const string mutexName = "Global\\KBShift_SingleInstance_Mutex";
        _mutex = new Mutex(true, mutexName, out bool createdNew);
        _ownsMutex = createdNew;

        if (!createdNew)
        {
            System.Windows.MessageBox.Show("KBShift is already running.", "KBShift", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            System.Windows.Application.Current.Shutdown();
            return;
        }

        base.OnStartup(e);
        ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
        var mainWindow = new MainWindow();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_ownsMutex && _mutex != null)
        {
            _mutex.ReleaseMutex();
        }
        if (_mutex != null)
        {
            _mutex.Dispose();
        }
        base.OnExit(e);
    }
    }
}