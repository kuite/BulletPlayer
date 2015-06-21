using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using BulletPlayerBackend.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace BulletPlayerBackend
{
    public class SessionManager
    {
        private const int SW_HIDE = 0;
        private const int SW_SHOW = 5;
        private readonly ChromeDriver _driver;
        private readonly MovesResolver _resolver;
        private readonly WindowsForm _window;
        public readonly EngineHandler EngineHandlerInstance = new EngineHandler();
        public readonly MovesHandler MovesHandlerInstance = new MovesHandler();

        //public bool IsScanning { get; set; }
        public bool IsPlaying { get; set; }

        public SessionManager(WindowsForm window)
        {
            _driver = new ChromeDriver();
            _resolver = new MovesResolver();
            _window = window;
        }

        public string Login()
        {
            _driver.Navigate().GoToUrl("http://live.chess.com/live?v=2015052201");
            var userNameField = _driver.FindElementById("c1");
            var userPasswordField = _driver.FindElementById("loginpassword");
            var loginButton = _driver.FindElementById("btnLogin");

            userNameField.SendKeys("kuite92");
            userPasswordField.SendKeys("");

            loginButton.Click();

            var loggedAsFiled = _driver.FindElementByClassName("chess_com_username_link");
            return loggedAsFiled.Text;
        }

        public void StartPlay(EngineHandler engineHandler, Process process)
        {
            ShowConsoleWindow();
            IsPlaying = true;
            var span = new TimeSpan(0, 0, 0, 60, 0);
            var wait = new WebDriverWait(_driver, span);
            var shortMovesList = _resolver.GetShortMovesList(_driver, MovesHandlerInstance);
            var resolvedLongMovesList = _resolver.GetSuggestedLongMovesList(shortMovesList);
            while (IsPlaying)
            {
                
                if (MovesHandlerInstance.PlayerToMove())
                {
                    var move = engineHandler.GetCalculateMove(process, resolvedLongMovesList);
                    Console.WriteLine(move);
                    MovesHandlerInstance.DoMove(move);
                }
                try
                {
                    wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='movelist_" + MovesHandlerInstance.Count + "']/a")));
                }
                catch (Exception)
                {
                    engineHandler.KillEngineProcess(process);
                    IsPlaying = false;
                    break;
                }
                shortMovesList.Add(_driver.FindElementByXPath("//*[@id='movelist_" + MovesHandlerInstance.Count + "']/a").Text);
                resolvedLongMovesList.Add(_resolver.GetSuggestedSplittedMove(_resolver.GetComputerMove(shortMovesList)));
                MovesHandlerInstance.Count++;
            }
        }

        public static void ShowConsoleWindow()
        {
            var handle = GetConsoleWindow();

            if (handle == IntPtr.Zero)
            {
                AllocConsole();
            }
            else
            {
                ShowWindow(handle, SW_SHOW);
            }
        }

        public static void HideConsoleWindow()
        {
            var handle = GetConsoleWindow();

            ShowWindow(handle, SW_HIDE);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
    }
}