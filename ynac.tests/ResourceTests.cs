using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ynac.Tests;

[TestClass]
public class ResourceTests
{
   [TestMethod]
    public void EmbeddedResource_Config_template_ini_ExistsWithContent()
    { 
        var cliAssembly = typeof(Program).Assembly; 

        using var stream = cliAssembly.GetManifestResourceStream(Constants.ConfigFileTemplate);
        
        Assert.IsNotNull(stream, $"Embedded resource '{Constants.ConfigFileTemplate}' not found.");

        using var reader = new StreamReader(stream);
        var content = reader.ReadToEnd();

        Assert.IsFalse(string.IsNullOrEmpty(content));
    } 
}