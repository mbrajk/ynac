using System.Diagnostics;

namespace ynac.OSFeatures;

internal class WindowsBrowserOpener : IBrowserOpener
{
    public void OpenBrowser(string url)
    {
        ProcessStartInfo psi = new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        };
        Process.Start (psi);
    }
}