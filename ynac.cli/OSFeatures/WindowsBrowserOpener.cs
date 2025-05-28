using System.Diagnostics;

namespace ynac.OSFeatures;

internal class WindowsBrowserOpener : IBrowserOpener
{
    public void OpenBrowser(string url)
    {
        var psi = new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        };
        Process.Start (psi);
    }
}