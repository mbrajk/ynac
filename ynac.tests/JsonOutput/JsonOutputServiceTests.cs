using Microsoft.VisualStudio.TestTools.UnitTesting;
using ynab.Budget;
using ynab.Category;
using ynac.JsonOutput;
using System.Text.Json;

namespace ynac.Tests.JsonOutput;

[TestClass]
public class JsonOutputServiceTests
{
    private string _testOutputDirectory = string.Empty;

    [TestInitialize]
    public void TestInitialize()
    {
        _testOutputDirectory = Path.Combine(Path.GetTempPath(), $"ynac-test-{Guid.NewGuid()}");
        Directory.CreateDirectory(_testOutputDirectory);
    }

    [TestCleanup]
    public void TestCleanup()
    {
        if (Directory.Exists(_testOutputDirectory))
        {
            Directory.Delete(_testOutputDirectory, true);
        }
    }

    [TestMethod]
    public async Task OutputBudgetJsonAsync_WritesToStdout()
    {
        // Arrange
        var service = new JsonOutputService();
        var budget = CreateTestBudget();
        var budgetMonth = CreateTestBudgetMonth();
        var categoryGroups = CreateTestCategoryGroups();

        // Capture stdout
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            // Act
            await service.OutputBudgetJsonAsync(budget, budgetMonth, categoryGroups, null);

            // Assert
            var jsonContent = writer.ToString();
            Assert.IsFalse(string.IsNullOrWhiteSpace(jsonContent), "JSON content should not be empty");
            
            // Verify it's valid JSON
            var doc = JsonDocument.Parse(jsonContent);
            Assert.IsNotNull(doc.RootElement.GetProperty("budget_name"));
            Assert.AreEqual("Test Budget", doc.RootElement.GetProperty("budget_name").GetString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [TestMethod]
    public async Task OutputBudgetJsonAsync_OutputsValidJson()
    {
        // Arrange
        var service = new JsonOutputService();
        var budget = CreateTestBudget();
        var budgetMonth = CreateTestBudgetMonth();
        var categoryGroups = CreateTestCategoryGroups();

        // Capture stdout
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            // Act
            await service.OutputBudgetJsonAsync(budget, budgetMonth, categoryGroups, null);

            // Assert - just verify it parses
            var output = writer.ToString();
            Assert.IsFalse(string.IsNullOrWhiteSpace(output), "Should output to stdout");
            
            var doc = JsonDocument.Parse(output);
            Assert.AreEqual("Test Budget", doc.RootElement.GetProperty("budget_name").GetString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [TestMethod]
    public async Task OutputBudgetJsonAsync_ContainsCorrectData()
    {
        // Arrange
        var service = new JsonOutputService();
        var budget = CreateTestBudget();
        var budgetMonth = CreateTestBudgetMonth();
        var categoryGroups = CreateTestCategoryGroups();
        
        // Capture stdout
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            // Act
            await service.OutputBudgetJsonAsync(budget, budgetMonth, categoryGroups, null);

            // Assert
            var jsonContent = writer.ToString();
            var doc = JsonDocument.Parse(jsonContent);
            
            Assert.AreEqual("Test Budget", doc.RootElement.GetProperty("budget_name").GetString());
            Assert.AreEqual(30, doc.RootElement.GetProperty("age_of_money").GetInt32());
            Assert.AreEqual(1000.50m, doc.RootElement.GetProperty("to_be_budgeted").GetDecimal());
            
            var categoryGroupsArray = doc.RootElement.GetProperty("category_groups");
            Assert.AreEqual(1, categoryGroupsArray.GetArrayLength());
            
            // Verify category currency values are converted from milliunits
            var firstGroup = categoryGroupsArray[0];
            var categories = firstGroup.GetProperty("categories");
            Assert.AreEqual(1, categories.GetArrayLength());
            
            var firstCategory = categories[0];
            Assert.AreEqual("Groceries", firstCategory.GetProperty("name").GetString());
            Assert.AreEqual(500.00m, firstCategory.GetProperty("budgeted").GetDecimal());
            Assert.AreEqual(-250.00m, firstCategory.GetProperty("activity").GetDecimal());
            Assert.AreEqual(250.00m, firstCategory.GetProperty("balance").GetDecimal());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    [TestMethod]
    public async Task OutputBudgetJsonAsync_OutputsToStdout()
    {
        // Arrange
        var service = new JsonOutputService();
        var budget = CreateTestBudget();
        var budgetMonth = CreateTestBudgetMonth();
        var categoryGroups = CreateTestCategoryGroups();

        // Capture stdout
        var originalOut = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);

        try
        {
            // Act
            await service.OutputBudgetJsonAsync(budget, budgetMonth, categoryGroups, null);

            // Assert
            var output = writer.ToString();
            Assert.IsFalse(string.IsNullOrWhiteSpace(output), "Should output to stdout");
            
            var doc = JsonDocument.Parse(output);
            Assert.AreEqual("Test Budget", doc.RootElement.GetProperty("budget_name").GetString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    private static Budget CreateTestBudget()
    {
        return new Budget
        {
            Name = "Test Budget",
            Id = Guid.NewGuid(),
            Type = BudgetType.UserBudget
        };
    }

    private static BudgetMonth CreateTestBudgetMonth()
    {
        return new BudgetMonth
        {
            AgeOfMoney = 30,
            ToBeBudgeted = 1000500, // In milliunits: $1000.50
            Note = "Test note"
        };
    }

    private static IReadOnlyCollection<CategoryGroup> CreateTestCategoryGroups()
    {
        var category = new Category
        {
            Id = Guid.NewGuid(),
            Name = "Groceries",
            Budgeted = 500000,
            Activity = -250000,
            Balance = 250000,
            Hidden = false,
            Deleted = false
        };

        var categoryGroup = new CategoryGroup
        {
            Id = Guid.NewGuid(),
            Name = "Essential Expenses",
            Hidden = false,
            Deleted = false,
            Categories = new[] { category }
        };

        return new[] { categoryGroup };
    }
}
