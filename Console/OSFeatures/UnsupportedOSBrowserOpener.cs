using System.Diagnostics;

namespace ynac.OSFeatures;

internal class UnsupportedOsBrowserOpener : IBrowserOpener
{
    public void OpenBrowser(string url)
    {
        throw new InvalidOperationException("Not implemented for this OS");
    }
}