using Xunit;
using System.IO;
using ynac; // For TokenHandler and Constants
using System.Linq;
using System;
using System.Collections.Generic;

namespace ynac.Tests
{
    public class TokenHandlerTests : IDisposable
    {
        private readonly string _testConfigFilePath;

        public TokenHandlerTests()
        {
            // Constants.ConfigFileLocation is "./config.ini"
            // Path.GetFullPath resolves this against the current working directory,
            // which for xUnit tests is usually the test project's output directory (e.g., bin/Debug/netX.X)
            _testConfigFilePath = Path.GetFullPath(Constants.ConfigFileLocation);
            
            // Ensure a clean state before each test
            CleanUpTestConfigFile();
        }

        private void CleanUpTestConfigFile()
        {
            if (File.Exists(_testConfigFilePath))
            {
                File.Delete(_testConfigFilePath);
            }
        }

        public void Dispose()
        {
            // Ensure a clean state after all tests in the class have run
            CleanUpTestConfigFile();
            GC.SuppressFinalize(this);
        }

        private void CreateTestConfigFile(string content)
        {
            // Ensure directory exists if ./config.ini implies a structure like that (though usually not for a simple filename)
            Directory.CreateDirectory(Path.GetDirectoryName(_testConfigFilePath));
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

        [Fact]
        public void MaybeSaveToken_SavesToken_WhenConfigFileDoesNotExistAndTokenIsValid()
        {
            string newToken = "test_token_123";

            TokenHandler.MaybeSaveToken(newToken);

            Assert.True(File.Exists(_testConfigFilePath), "Config file should be created.");
            var lines = ReadTestConfigFile();
            Assert.Contains(Constants.YnabSectionKey, lines);
            Assert.Contains($"{Constants.TokenString}={newToken}", lines);
        }

        [Fact]
        public void MaybeSaveToken_SavesToken_WhenConfigFileHasPlaceholderTokenAndTokenIsValid()
        {
            string initialContent = $"{Constants.YnabSectionKey}\n{Constants.TokenString}={Constants.DefaultTokenString}\n";
            CreateTestConfigFile(initialContent);
            string newToken = "test_token_456";

            TokenHandler.MaybeSaveToken(newToken);

            var lines = ReadTestConfigFile();
            Assert.Contains(Constants.YnabSectionKey, lines);
            Assert.Contains($"{Constants.TokenString}={newToken}", lines);
            Assert.DoesNotContain($"{Constants.TokenString}={Constants.DefaultTokenString}", lines);
        }

        [Fact]
        public void MaybeSaveToken_DoesNotSaveToken_WhenTokenIsEmptyAndConfigFileDoesNotExist()
        {
            TokenHandler.MaybeSaveToken("");

            Assert.False(File.Exists(_testConfigFilePath), "Config file should not be created for empty token if it doesn't exist.");
        }
        
        [Fact]
        public void MaybeSaveToken_DoesNotAlterFile_WhenTokenIsEmptyAndConfigFileExists()
        {
            string existingToken = "existing_valid_token";
            string initialContent = $"{Constants.YnabSectionKey}\n{Constants.TokenString}={existingToken}\n";
            CreateTestConfigFile(initialContent);
            long initialFileSize = new FileInfo(_testConfigFilePath).Length;

            TokenHandler.MaybeSaveToken("");
            
            Assert.True(File.Exists(_testConfigFilePath));
            var lines = ReadTestConfigFile();
            Assert.Contains($"{Constants.TokenString}={existingToken}", lines);
            long finalFileSize = new FileInfo(_testConfigFilePath).Length;
            Assert.Equal(initialFileSize, finalFileSize); // Ensure file not touched
        }


        [Fact]
        public void MaybeSaveToken_DoesNotSaveToken_WhenTokenIsWhitespaceAndConfigFileDoesNotExist()
        {
            TokenHandler.MaybeSaveToken("   ");

            Assert.False(File.Exists(_testConfigFilePath), "Config file should not be created for whitespace token if it doesn't exist.");
        }

        [Fact]
        public void MaybeSaveToken_DoesNotAlterFile_WhenTokenIsWhitespaceAndConfigFileExists()
        {
            string existingToken = "existing_valid_token";
            string initialContent = $"{Constants.YnabSectionKey}\n{Constants.TokenString}={existingToken}\n";
            CreateTestConfigFile(initialContent);
            long initialFileSize = new FileInfo(_testConfigFilePath).Length;

            TokenHandler.MaybeSaveToken("   ");

            Assert.True(File.Exists(_testConfigFilePath));
            var lines = ReadTestConfigFile();
            Assert.Contains($"{Constants.TokenString}={existingToken}", lines);
            long finalFileSize = new FileInfo(_testConfigFilePath).Length;
            Assert.Equal(initialFileSize, finalFileSize);
        }
        
        [Fact]
        public void MaybeSaveToken_DoesNotSaveToken_WhenTokenIsDefaultPlaceholderAndConfigFileDoesNotExist()
        {
            TokenHandler.MaybeSaveToken(Constants.DefaultTokenString);

            Assert.False(File.Exists(_testConfigFilePath), "Config file should not be created for default placeholder token if it doesn't exist.");
        }

        [Fact]
        public void MaybeSaveToken_DoesNotAlterFile_WhenTokenIsDefaultPlaceholderAndConfigFileExists()
        {
            string existingToken = "existing_valid_token";
            string initialContent = $"{Constants.YnabSectionKey}\n{Constants.TokenString}={existingToken}\n";
            CreateTestConfigFile(initialContent);
            long initialFileSize = new FileInfo(_testConfigFilePath).Length;

            TokenHandler.MaybeSaveToken(Constants.DefaultTokenString);

            Assert.True(File.Exists(_testConfigFilePath));
            var lines = ReadTestConfigFile();
            Assert.Contains($"{Constants.TokenString}={existingToken}", lines);
            long finalFileSize = new FileInfo(_testConfigFilePath).Length;
            Assert.Equal(initialFileSize, finalFileSize);
        }

        // Test scenario 6: Testing that a new valid token *overwrites* an existing user token.
        // This reflects the current implementation's behavior where a token from CLI is prioritized for saving.
        [Fact]
        public void MaybeSaveToken_OverwritesToken_WhenConfigFileHasDifferentUserTokenAndTokenIsValid()
        {
            string existingUserToken = "existing_user_token";
            string initialContent = $"{Constants.YnabSectionKey}\n{Constants.TokenString}={existingUserToken}\n";
            CreateTestConfigFile(initialContent);
            string newTokenFromCli = "new_token_from_cli";

            TokenHandler.MaybeSaveToken(newTokenFromCli);

            var lines = ReadTestConfigFile();
            Assert.Contains(Constants.YnabSectionKey, lines);
            Assert.Contains($"{Constants.TokenString}={newTokenFromCli}", lines);
            Assert.DoesNotContain($"{Constants.TokenString}={existingUserToken}", lines);
        }
        
        // Additional test cases for robustness
        [Fact]
        public void MaybeSaveToken_AddsToken_WhenConfigFileExistsWithoutTokenEntryInSectionAndTokenIsValid()
        {
            string initialContent = $"{Constants.YnabSectionKey}\nSomeOtherKey=SomeValue\n";
            CreateTestConfigFile(initialContent);
            string newToken = "newly_added_token";

            TokenHandler.MaybeSaveToken(newToken);

            var lines = ReadTestConfigFile();
            Assert.Contains(Constants.YnabSectionKey, lines);
            Assert.Contains("SomeOtherKey=SomeValue", lines);
            Assert.Contains($"{Constants.TokenString}={newToken}", lines);
        }
        
        [Fact]
        public void MaybeSaveToken_AddsTokenAndSection_WhenConfigFileIsEmptyAndTokenIsValid()
        {
            CreateTestConfigFile(""); // Empty file
            string newToken = "token_for_empty_file";

            TokenHandler.MaybeSaveToken(newToken);

            var lines = ReadTestConfigFile();
            Assert.Contains(Constants.YnabSectionKey, lines);
            Assert.Contains($"{Constants.TokenString}={newToken}", lines);
        }
        
        [Fact]
        public void MaybeSaveToken_AddsTokenAndSection_WhenConfigFileExistsWithOtherSectionsAndTokenIsValid()
        {
            string initialContent = "[OtherSection]\nKey=Value\n";
            CreateTestConfigFile(initialContent);
            string newToken = "token_with_other_sections";

            TokenHandler.MaybeSaveToken(newToken);

            var lines = ReadTestConfigFile();
            Assert.Contains("[OtherSection]", lines);
            Assert.Contains("Key=Value", lines);
            Assert.Contains(Constants.YnabSectionKey, lines);
            Assert.Contains($"{Constants.TokenString}={newToken}", lines);
        }
    }
}
