using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Tests;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Users;
using OpenQA.Selenium;
using Xunit;

namespace ProjectLighthouse.Tests.WebsiteTests.Tests;

public class AdminTests : LighthouseWebTest
{
    public const string AdminPanelButtonXPath = "/html/body/div/header/div/div/div/a[1]";

    [DatabaseFact]
    public async Task ShouldShowAdminPanelButtonWhenAdmin()
    {
        await using DatabaseContext database = new();
        Random random = new();
        UserEntity user = await database.CreateUser($"unitTestUser{random.Next()}", CryptoHelper.BCryptHash("i'm an engineering failure"));

        WebTokenEntity webToken = new()
        {
            UserId = user.UserId,
            UserToken = CryptoHelper.GenerateAuthToken(),
            ExpiresAt = DateTime.Now + TimeSpan.FromHours(1),
            Verified = true,
        };

        database.WebTokens.Add(webToken);
        user.PermissionLevel = PermissionLevel.Administrator;
        await database.SaveChangesAsync();

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/");
        this.Driver.Manage().Cookies.AddCookie(new Cookie("LighthouseToken", webToken.UserToken));
        this.Driver.Navigate().Refresh();

        Assert.Contains("Admin", this.Driver.FindElement(By.XPath(AdminPanelButtonXPath)).Text);
    }

    [DatabaseFact]
    public async Task ShouldNotShowAdminPanelButtonWhenNotAdmin()
    {
        await using DatabaseContext database = new();
        Random random = new();
        UserEntity user = await database.CreateUser($"unitTestUser{random.Next()}", CryptoHelper.BCryptHash("i'm an engineering failure"));

        WebTokenEntity webToken = new()
        {
            UserId = user.UserId,
            UserToken = CryptoHelper.GenerateAuthToken(),
            ExpiresAt = DateTime.Now + TimeSpan.FromHours(1),
            Verified = true,
        };

        database.WebTokens.Add(webToken);
        user.PermissionLevel = PermissionLevel.Default;
        await database.SaveChangesAsync();

        this.Driver.Navigate().GoToUrl(this.BaseAddress + "/");
        this.Driver.Manage().Cookies.AddCookie(new Cookie("LighthouseToken", webToken.UserToken));
        this.Driver.Navigate().Refresh();

        Assert.DoesNotContain("Admin", this.Driver.FindElement(By.XPath(AdminPanelButtonXPath)).Text);
    }
}