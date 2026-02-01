using System.Linq;
using System.Windows.Forms;

namespace KBShift.Core.Services
{

public class InputLanguageManager
{
    public void SetLanguage(string name)
    {
        var lang = InputLanguage.InstalledInputLanguages
            .Cast<InputLanguage>()
            .FirstOrDefault(l => l.Culture.EnglishName.Contains(name));

        if (lang != null) InputLanguage.CurrentInputLanguage = lang;
    }
    }
}