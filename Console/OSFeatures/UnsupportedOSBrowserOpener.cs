using System.Diagnostics;

namespace ynac.OSFeatures;

internal class UnsupportedOSBrowserOpener : IBrowserOpener
{
    public void OpenBrowser(string url)
    {
        throw new InvalidOperationException("Not implemented for this OS");
    }
}