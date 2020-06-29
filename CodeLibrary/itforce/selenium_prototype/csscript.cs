using System;
using System.Drawing;
using System.IO;
using System.Data.SQLite;
using System.Data;
using Microsoft.CSharp;
using System.Collections.Generic;
using DevelopWorkspace.Base;
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Chrome;
//css_reference WebDriver.dll
//ExtensionsコードをCurrentDomainで一回しか実行できない、２回目以降、全体を実行したい場合、Extensionsコードを一旦削除してからしてください
public static class WebDriverExtensions
{
    public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
    {
        Logger.WriteLine("FindElement..." + by.ToString(), Level.DEBUG);
        if (timeoutInSeconds > 0)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
            return wait.Until(drv => drv.FindElement(by));
        }
        return driver.FindElement(by);
    }
}

public class Script
{
    public static Func<IWebDriver, IWebElement> ElementIsClickable(By locator)
    {
        return driver =>
        {
            var element = driver.FindElement(locator);

            if (element != null && element.Displayed)
            {
                Logger.WriteLine("ElementIsClickable...Displayed", Level.DEBUG);
                if (element.Enabled) Logger.WriteLine("ElementIsClickable...Enabled", Level.DEBUG);
            }

            return (element != null && element.Displayed && element.Enabled) ? element : null;
        };
    }
    public static Func<IWebDriver, IWebElement> InvisibilityOfElementLocated(By locator)
    {
        return driver =>
        {
            var element = driver.FindElement(locator);
            if (element != null && element.Displayed)
            {
                Logger.WriteLine("InvisibilityOfElementLocated...Displayed", Level.DEBUG);
            }
            return (element != null && !element.Displayed) ? element : null;
        };
    }
    public static void Main(string[] args)
    {
        IWebDriver driver = new ChromeDriver();
        IWebElement textbox;
        IWebElement findbuttom;
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMinutes(1);
        driver.Manage().Window.Maximize();
        //Webページを開く
        driver.Navigate().GoToUrl("http://localhost:4200/#/login");
        for (int i = 0; i < 10; i++)
        {
            //検索ボックス
            textbox = driver.FindElement(By.Name("user_id"), 60);
            //検索ボックスに検索ワードを入力
            textbox.SendKeys("admin");

            textbox = driver.FindElement(By.Name("password"), 60);
            //検索ボックスに検索ワードを入力
            textbox.SendKeys("test01");

            //検索ボタン
            findbuttom = driver.FindElement(By.Name("login"), 60);
            //検索ボタンをクリック
            findbuttom.Click();

            //Loading待ち画面消えるまで、なにもしない、待機する
            Logger.WriteLine("loading...", Level.DEBUG);
            var wait = new WebDriverWait(driver, TimeSpan.FromMinutes(1));
            wait.Until(InvisibilityOfElementLocated(By.Id("loading-center")));

            Logger.WriteLine("Logout...", Level.DEBUG);
            wait = new WebDriverWait(driver, TimeSpan.FromMinutes(1));
            var clickableElement = wait.Until(ElementIsClickable(By.LinkText("Logout")));

            //OpenQA.Selenium.Interactions.Actions actions = new OpenQA.Selenium.Interactions.Actions(driver);
            //actions.MoveToElement(clickableElement).Click().Perform();
            //JavascriptExecutor jse = (JavascriptExecutor)driver;
            //jse.executeScript("arguments[0].scrollIntoView()", Webelement);

            clickableElement.Click();
            Logger.WriteLine("login...", Level.DEBUG);
        }
    }
}