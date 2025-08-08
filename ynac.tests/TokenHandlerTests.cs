using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ynac.Tests;

[TestClass]
public class TokenHandlerTests
{
    private string _testConfigFilePath = string.Empty;

    [TestInitialize]
    public void TestInitialize()
    {
        _testConfigFilePath = Path.GetFullPath($"./{Constants.ConfigFileName}");
        CleanUpTestConfigFile();
    }

    [TestCleanup]
    public void TestCleanup()
    {
        CleanUpTestConfigFile();
        GC.SuppressFinalize(this);
    }

    private void CleanUpTestConfigFile()
    {
        if (File.Exists(_testConfigFilePath))
        {
            File.Delete(_testConfigFilePath);
        }
    }

    private void CreateTestConfigFile(string content)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_testConfigFilePath) ?? throw new InvalidOperationException());
        File.WriteAllText(_testConfigFilePath, content);
    }

    private List<string> ReadTestConfigFile()
    {
        if (!File.Exists(_testConfigFilePath))
        {
            return new List<string>();
        }
        return File.ReadAllLines(_testConfigFilePath).ToList();
    }

    [TestMethod]
    public void MaybeSaveToken_SavesToken_WhenConfigFileDoesNotExistAndTokenIsValid()
    {
        // Arrange
        var newToken = "test_token_123";

        // Act
        TokenHandler.MaybeSaveToken(newToken);

        // Assert
        Assert.IsTrue(File.Exists(_testConfigFilePath), "Config file should be created.");
        var lines = ReadTestConfigFile();
        CollectionAssert.Contains(lines, Constants.YnabApiSectionHeader);
        CollectionAssert.Contains(lines, $"{Constants.TokenString}={newToken}");
    }

    [TestMethod]
    public void MaybeSaveToken_SavesToken_WhenConfigFileHasPlaceholderTokenAndTokenIsValid()
    {
        // Arrange
        var initialContent = $"{Constants.YnabApiSectionHeader}\n{Constants.TokenString}={Constants.DefaultTokenString}\n";
        CreateTestConfigFile(initialContent);
        var newToken = "test_token_456";

        // Act
        TokenHandler.MaybeSaveToken(newToken);

        // Assert
        var lines = ReadTestConfigFile();
        CollectionAssert.Contains(lines, Constants.YnabApiSectionHeader);
        CollectionAssert.Contains(lines, $"{Constants.TokenString}={newToken}");
        CollectionAssert.DoesNotContain(lines, $"{Constants.TokenString}={Constants.DefaultTokenString}");
    }

    [TestMethod]
    public void MaybeSaveToken_DoesNotSaveToken_WhenTokenIsEmptyOrWhitespaceAndConfigFileDoesNotExist()
    {
        // Arrange & Act
        TokenHandler.MaybeSaveToken("");
        TokenHandler.MaybeSaveToken("  ");
        TokenHandler.MaybeSaveToken(Constants.DefaultTokenString);

        // Assert
        Assert.IsFalse(File.Exists(_testConfigFilePath), "Config file should not be created for empty token if it doesn't exist.");
    }

    [TestMethod]
    public void MaybeSaveToken_DoesNotAlterFile_WhenTokenIsEmptyAndConfigFileExists()
    {
        // Arrange
        var existingToken = "existing_valid_token";
        var initialContent = $"{Constants.YnabApiSectionHeader}\n{Constants.TokenString}={existingToken}\n";
        CreateTestConfigFile(initialContent);
        var initialFileSize = new FileInfo(_testConfigFilePath).Length;

        // Act
        TokenHandler.MaybeSaveToken("");

        // Assert
        Assert.IsTrue(File.Exists(_testConfigFilePath));
        var lines = ReadTestConfigFile();
        CollectionAssert.Contains(lines, $"{Constants.TokenString}={existingToken}");
        var finalFileSize = new FileInfo(_testConfigFilePath).Length;
        Assert.AreEqual(initialFileSize, finalFileSize);
    }

    [TestMethod]
    public void MaybeSaveToken_DoesNotAlterFile_WhenTokenIsWhitespaceAndConfigFileExists()
    {
        // Arrange
        var existingToken = "existing_valid_token";
        var initialContent = $"{Constants.YnabApiSectionHeader}\n{Constants.TokenString}={existingToken}\n";
        CreateTestConfigFile(initialContent);
        var initialFileSize = new FileInfo(_testConfigFilePath).Length;

        // Act
        TokenHandler.MaybeSaveToken("   ");

        // Assert
        Assert.IsTrue(File.Exists(_testConfigFilePath));
        var lines = ReadTestConfigFile();
        CollectionAssert.Contains(lines, $"{Constants.TokenString}={existingToken}");
        var finalFileSize = new FileInfo(_testConfigFilePath).Length;
        Assert.AreEqual(initialFileSize, finalFileSize);
    }

    [TestMethod]
    public void MaybeSaveToken_DoesNotAlterFile_WhenTokenIsDefaultPlaceholderAndConfigFileExists()
    {
        // Arrange
        var existingToken = "existing_valid_token";
        var initialContent = $"{Constants.YnabApiSectionHeader}\n{Constants.TokenString}={existingToken}\n";
        CreateTestConfigFile(initialContent);
        var initialFileSize = new FileInfo(_testConfigFilePath).Length;

        // Act
        TokenHandler.MaybeSaveToken(Constants.DefaultTokenString);

        // Assert
        Assert.IsTrue(File.Exists(_testConfigFilePath));
        var lines = ReadTestConfigFile();
        CollectionAssert.Contains(lines, $"{Constants.TokenString}={existingToken}");
        var finalFileSize = new FileInfo(_testConfigFilePath).Length;
        Assert.AreEqual(initialFileSize, finalFileSize);
    }

    [TestMethod]
    public void MaybeSaveToken_OverwritesToken_WhenConfigFileHasDifferentUserTokenAndTokenIsValid()
    {
        // Arrange
        var existingUserToken = "existing_user_token";
        var initialContent = $"{Constants.YnabApiSectionHeader}\n{Constants.TokenString}={existingUserToken}\n";
        CreateTestConfigFile(initialContent);
        var newTokenFromCli = "new_token_from_cli";

        // Act
        TokenHandler.MaybeSaveToken(newTokenFromCli);

        // Assert
        var lines = ReadTestConfigFile();
        CollectionAssert.Contains(lines, Constants.YnabApiSectionHeader);
        CollectionAssert.Contains(lines, $"{Constants.TokenString}={newTokenFromCli}");
        CollectionAssert.DoesNotContain(lines, $"{Constants.TokenString}={existingUserToken}");
    }

    [TestMethod]
    public void MaybeSaveToken_AddsToken_WhenConfigFileExistsWithoutTokenEntryInSectionAndTokenIsValid()
    {
        // Arrange
        var initialContent = $"{Constants.YnabApiSectionHeader}\nSomeOtherKey=SomeValue\n";
        CreateTestConfigFile(initialContent);
        var newToken = "newly_added_token";

        // Act
        TokenHandler.MaybeSaveToken(newToken);

        // Assert
        var lines = ReadTestConfigFile();
        CollectionAssert.Contains(lines, Constants.YnabApiSectionHeader);
        CollectionAssert.Contains(lines, "SomeOtherKey=SomeValue");
        CollectionAssert.Contains(lines, $"{Constants.TokenString}={newToken}");
    }

    [TestMethod]
    public void MaybeSaveToken_AddsTokenAndSection_WhenConfigFileIsEmptyAndTokenIsValid()
    {
        // Arrange
        CreateTestConfigFile("");
        var newToken = "token_for_empty_file";

        // Act
        TokenHandler.MaybeSaveToken(newToken);

        // Assert
        var lines = ReadTestConfigFile();
        CollectionAssert.Contains(lines, Constants.YnabApiSectionHeader);
        CollectionAssert.Contains(lines, $"{Constants.TokenString}={newToken}");
    }

    [TestMethod]
    public void MaybeSaveToken_AddsTokenAndSection_WhenConfigFileExistsWithOtherSectionsAndTokenIsValid()
    {
        // Arrange
        var initialContent = "[OtherSection]\nKey=Value\n";
        CreateTestConfigFile(initialContent);
        var newToken = "token_with_other_sections";

        // Act
        TokenHandler.MaybeSaveToken(newToken);

        // Assert
        var lines = ReadTestConfigFile();
        CollectionAssert.Contains(lines, "[OtherSection]");
        CollectionAssert.Contains(lines, "Key=Value");
        CollectionAssert.Contains(lines, Constants.YnabApiSectionHeader);
        CollectionAssert.Contains(lines, $"{Constants.TokenString}={newToken}");
    }
}