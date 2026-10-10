using System;
using System.Windows.Forms;

namespace SewageStormwaterManagement;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        DatabaseService.Initialize();
        Application.Run(new MainForm());
    }
}
