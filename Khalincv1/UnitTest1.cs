using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

[TestFixture]
public class AmazonTests
{
    private IPlaywright _playwright;
    private IBrowser _browser;

    public class Book
    {
        public string Title { get; set; }
        public bool IsBestSeller { get; set; }
    }

    [SetUp]
    public async Task SetUp()
    {
        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Firefox.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false 
        });
    }

    [TearDown]
    public async Task TearDown()
    {
        await _browser.CloseAsync();
        _playwright.Dispose();
    }

    [Test]
    public async Task VerifyJavaBooksAndCheckSpecificBook()
    {
        List<Book> books = new List<Book>();

        var context = await _browser.NewContextAsync();
        var page = await context.NewPageAsync();

        try
        {
            // ������� �� Amazon
            await page.GotoAsync("https://www.amazon.com/");

            // ���������� ������ Books
            await page.Locator("//select[@id='searchDropdownBox']").SelectOptionAsync("Books");

            /// ������ �������� ����� "Java" �� �������� �����
            await page.Locator("//input[@id='twotabsearchtextbox']").FillAsync("Java");
            await page.Locator("//input[@id='twotabsearchtextbox']").PressAsync("Enter");

            // ��������, ���� �'�������� ����������
            await page.WaitForSelectorAsync("(//div[@role='listitem'])[16]");
            //Thread.Sleep(3000);

            //�������� ���� ���� �� �������� �������� ����������
            int counter = await page.Locator("//div[@role='listitem']").CountAsync();

            for (int i = 1; i <= counter; i++)
            {
                string title = await page.Locator($"//div[@role='listitem'][{i}]//h2").InnerTextAsync();
                string bestSellerBadge = await page.Locator($"//div[@role='listitem'][{i}]//div[@class='a-section a-spacing-none aok-relative puis-status-badge-container s-list-status-badge-container']").InnerTextAsync();

                books.Add(new Book
                {
                    Title = title,
                    IsBestSeller = !string.IsNullOrEmpty(bestSellerBadge)
                });
            }

            // ���������, �� � ����� "Head First Java, 2nd edition"
            //await page.GotoAsync("https://www.amazon.com/Head-First-Java-Kathy-Sierra/dp/0596009208");
            //������ ��������� �� ����� ���� ����� �� ���� �� ������ �������
            await page.GotoAsync("https://a.co/d/9S4jsgL");
            await page.WaitForSelectorAsync("//span[@id='productTitle']");

            string targetBook = await page.Locator("//span[@id='productTitle']").InnerTextAsync();
            bool containsTargetBook = books.Exists(book => book.Title.Contains(targetBook.Trim()));

            Assert.IsTrue(containsTargetBook, $"The book '{targetBook}' was not found in the search results.");
        }
        catch (Exception ex)
        {
            Assert.Fail($"An error occurred: {ex.Message}");
        }
        finally
        {
            await page.CloseAsync();
        }
    }
}
