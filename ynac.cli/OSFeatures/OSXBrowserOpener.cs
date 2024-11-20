using System.Diagnostics;

namespace ynac.OSFeatures;

// ReSharper disable once InconsistentNaming
internal class OSXBrowserOpener : IBrowserOpener
{
    public void OpenBrowser(string url)
    {
        Process.Start("open", url);
    }
}