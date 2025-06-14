using Xunit;
using Moq;
using Spectre.Console;
using Spectre.Console.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ynab.Budget;
using ynab.Category;
using ynac.BudgetActions;
using ynac.BudgetSelection;
using ynac.Commands;
using ynac.CurrencyFormatting;

namespace ynac.tests
{
    public class YnacConsoleTests
    {
        private readonly Mock<IBudgetQueryService> _mockBudgetQueryService;
        private readonly Mock<IBudgetBrowserOpener> _mockBudgetBrowserOpener;
        private readonly Mock<IBudgetSelector> _mockBudgetSelector;
        private readonly Mock<IEnumerable<IBudgetAction>> _mockBudgetActions;
        // _ynacConsole field is removed, will be instantiated per test with specific formatter

        private readonly BudgetMonth _sampleBudgetMonth;
        private readonly List<CategoryGroup> _sampleCategoryGroups;
        private readonly BudgetCommandSettings _defaultSettings;

        public YnacConsoleTests()
        {
            _mockBudgetQueryService = new Mock<IBudgetQueryService>();
            _mockBudgetBrowserOpener = new Mock<IBudgetBrowserOpener>();
            _mockBudgetSelector = new Mock<IBudgetSelector>();
            _mockBudgetActions = new Mock<IEnumerable<IBudgetAction>>();
            _defaultSettings = new BudgetCommandSettings(); // Default settings

            // Sample Data
            _sampleBudgetMonth = new BudgetMonth { ToBeBudgeted = 1234500, AgeOfMoney = 30 }; // 1234.50

            _sampleCategoryGroups = new List<CategoryGroup>
            {
                new CategoryGroup
                {
                    Id = "cg1", Name = "Group 1", Hidden = false, Deleted = false,
                    Categories = new List<Category>
                    {
                        new Category { Id = "cat1", Name = "Category 1", Budgeted = 50000, Activity = -20000, Balance = 30000, GoalPercentageComplete = 50, Hidden = false, Deleted = false },
                        new Category { Id = "cat2", Name = "Category 2", Budgeted = 100000, Activity = 0, Balance = 100000, Hidden = false, Deleted = false }
                    }
                }
            };

            // Default Mocks
            _mockBudgetQueryService.Setup(s => s.GetCurrentMonthBudget(It.IsAny<SelectedBudget>()))
                                   .ReturnsAsync(_sampleBudgetMonth);
            _mockBudgetQueryService.Setup(s => s.GetBudgetCategories(It.IsAny<Action<BudgetQueryOptions>>()))
                                   .ReturnsAsync(_sampleCategoryGroups);
             _mockBudgetSelector.Setup(s => s.SelectBudget(It.IsAny<string>(), It.IsAny<bool>()))
                                .ReturnsAsync(new SelectedBudget("budget_id", "Test Budget", BudgetType.Existing));
        }

        // SetHideAmountsFlag and TestInterruptionException are removed.

        [Fact]
        public void CreateTable_WithHiddenFormatter_HidesToBeBudgeted()
        {
            // Arrange
            var ynacConsole = new YnacConsole(
                _mockBudgetQueryService.Object,
                _mockBudgetBrowserOpener.Object,
                _mockBudgetSelector.Object,
                _mockBudgetActions.Object,
                new HiddenCurrencyFormatter()
            );

            // Act
            var table = ynacConsole.CreateTable("Test Budget", _sampleBudgetMonth);
            var column = table.Columns.First(); // Assuming single column layout for the header part

            // Assert
            var output = new TestAnsiConsole().Write(column.Header).Output;
            Assert.Contains("To Be Budgeted: [green]***[/]", output);
            // Check that the actual amount is not present
            var defaultFormatter = new DefaultCurrencyFormatter();
            var actualAmountString = defaultFormatter.Format(_sampleBudgetMonth.ToBeBudgeted / 1000m);
            Assert.DoesNotContain(actualAmountString, output);
        }

        [Fact]
        public void CreateTable_WithDefaultFormatter_ShowsToBeBudgeted()
        {
            // Arrange
            var defaultFormatter = new DefaultCurrencyFormatter();
            var ynacConsole = new YnacConsole(
                _mockBudgetQueryService.Object,
                _mockBudgetBrowserOpener.Object,
                _mockBudgetSelector.Object,
                _mockBudgetActions.Object,
                defaultFormatter
            );

            // Act
            var table = ynacConsole.CreateTable("Test Budget", _sampleBudgetMonth);
            var column = table.Columns.First();
            var expectedAmountString = defaultFormatter.Format(_sampleBudgetMonth.ToBeBudgeted / 1000m);

            // Assert
            var output = new TestAnsiConsole().Write(column.Header).Output;
            Assert.Contains($"To Be Budgeted: [green]{expectedAmountString}[/]", output);
            Assert.DoesNotContain("***", output);
        }

        [Fact]
        public void GenerateCategoryTable_WithHiddenFormatter_HidesMonetaryValues()
        {
            // Arrange
            var ynacConsole = new YnacConsole(
                _mockBudgetQueryService.Object,
                _mockBudgetBrowserOpener.Object,
                _mockBudgetSelector.Object,
                _mockBudgetActions.Object,
                new HiddenCurrencyFormatter()
            );
            var mainTable = new Table();

            // Act
            ynacConsole.GenerateCategoryTable(_sampleCategoryGroups, _defaultSettings, mainTable);

            // Assert
            Assert.NotEmpty(mainTable.Rows);
            var firstRow = mainTable.Rows.First(); // This row contains the subTable
            var subTable = firstRow.OfType<Table>().FirstOrDefault();
            Assert.NotNull(subTable);

            // Check data rows in subTable
            foreach (var dataRow in subTable.Rows.Take(_sampleCategoryGroups.First().Categories.Count)) // Exclude footer
            {
                // Cells are IRenderable. For Markup, check its content.
                // Budgeted (index 1), Activity (index 2), Available (index 3)
                var budgetedCell = dataRow[1] as Markup;
                var activityCell = dataRow[2] as Markup;
                var availableCell = dataRow[3] as Markup;

                Assert.NotNull(budgetedCell);
                Assert.NotNull(activityCell);
                Assert.NotNull(availableCell);

                Assert.Equal("[green]***[/]", budgetedCell.ToString());
                Assert.Equal("[red]***[/]", activityCell.ToString());
                Assert.Equal("[green]***[/]", availableCell.ToString());
            }

            // Check footer row in subTable
            var footerCells = subTable.Rows.Last();
            Assert.Equal("[green]***[/]", footerCells[1].ToString()); // Footer cells are strings
            Assert.Equal("[red]***[/]", footerCells[2].ToString());
            Assert.Equal("[green]***[/]", footerCells[3].ToString());
        }

        [Fact]
        public void GenerateCategoryTable_WithDefaultFormatter_ShowsMonetaryValues()
        {
            // Arrange
            var defaultFormatter = new DefaultCurrencyFormatter();
            var ynacConsole = new YnacConsole(
                _mockBudgetQueryService.Object,
                _mockBudgetBrowserOpener.Object,
                _mockBudgetSelector.Object,
                _mockBudgetActions.Object,
                defaultFormatter
            );
            var mainTable = new Table();
            var settings = new BudgetCommandSettings { ShowGoals = false };

            // Act
            ynacConsole.GenerateCategoryTable(_sampleCategoryGroups, settings, mainTable);

            // Assert
            var subTable = mainTable.Rows.First().OfType<Table>().FirstOrDefault();
            Assert.NotNull(subTable);

            var firstCategory = _sampleCategoryGroups.First().Categories.First();
            var expectedBudgeted = (firstCategory.Budgeted / 1000m).ToString("C");
            var expectedActivity = (firstCategory.Activity / 1000m).ToString("C");
            var expectedAvailable = (firstCategory.Balance / 1000m).ToString("C");

            var firstDataRowCells = subTable.Rows.First();
            Assert.Equal($"[green]{expectedBudgeted}[/]", (firstDataRowCells[1] as Markup)?.ToString());
            Assert.Equal($"[red]{expectedActivity}[/]", (firstDataRowCells[2] as Markup)?.ToString());
            Assert.Equal($"[green]{expectedAvailable}[/]", (firstDataRowCells[3] as Markup)?.ToString());

            // Check footer
            var totalBudgeted = _sampleCategoryGroups.First().Categories.Sum(c => c.Budgeted / 1000m);
            var totalActivity = _sampleCategoryGroups.First().Categories.Sum(c => c.Activity / 1000m);
            var totalAvailable = _sampleCategoryGroups.First().Categories.Sum(c => c.Balance / 1000m);

            var footerCells = subTable.Rows.Last();
             Assert.Equal($"[green]{defaultFormatter.Format(totalBudgeted)}[/]", footerCells[1].ToString());
             Assert.Equal($"[red]{defaultFormatter.Format(totalActivity)}[/]", footerCells[2].ToString());
             Assert.Equal($"[green]{defaultFormatter.Format(totalAvailable)}[/]", footerCells[3].ToString());
        }
    }

    // Helper class to capture Spectre Console output for assertions if needed for complex renderables
    public class TestAnsiConsole : IAnsiConsole
    {
        private readonly System.Text.StringBuilder _builder = new System.Text.StringBuilder();
        public string Output => _builder.ToString();
        public void Clear(bool home) => _builder.Clear();
        public void Write(IRenderable renderable) => _builder.Append(renderable.ToString()); // Simplified: real rendering is more complex
        // Implement other IAnsiConsole members as needed, most can be empty or throw NotImplementedException
        public Spectre.Console.Profile Profile => new Spectre.Console.Profile(System.Console.Out, ColorSystem.NoColors);
        public IAnsiConsoleCursor Cursor => throw new NotImplementedException();
        public IAnsiConsoleInput Input => throw new NotImplementedException();
        public bool ShouldPromptsBeSkipped => false;
        public void FallbackStatus() => throw new NotImplementedException();
        public RenderPipeline Pipeline => throw new NotImplementedException();
        // ... other members
        public void Markup(string markup) => _builder.Append(markup);
        public void MarkupLine(string markup) => _builder.AppendLine(markup);
        // ... and so on for other IAnsiConsole methods. For these tests, Write(IRenderable) and Markup are key.
         public void Attach(RenderHook hook) => throw new NotImplementedException();
        public void ExclusivityMode(Func<Task> action) => throw new NotImplementedException();
        public void ExclusivityMode<T>(Func<T> action) => throw new NotImplementedException();
        public void ExclusivityMode<T>(Func<Task<T>> action) => throw new NotImplementedException();
        public RenderTarget RenderTarget => RenderTarget.Console;
        public Justify? Justification { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Width { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int Height { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Encoding Encoding { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool ProfileSupports(ColorSystem system) => throw new NotImplementedException();
        public string ExpandPath(string path) => throw new NotImplementedException();
        public IReadOnlyList<string> Lines => throw new NotImplementedException();
        public void Update<T>(T task, Action<T> action) where T : ProgressTask => throw new NotImplementedException();
    }
}
