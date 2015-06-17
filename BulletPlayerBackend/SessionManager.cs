using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using BulletPlayerBackend.Utils;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;


namespace BulletPlayerBackend
{
    public class SessionManager
    {
        private readonly ChromeDriver _driver = new ChromeDriver();
        private readonly MovesResolver _resolver = new MovesResolver();
        public readonly MovesHandler MovesHandlerInstance = new MovesHandler();
        public readonly EngineHandler EngineHandlerInstance = new EngineHandler();
        
        public bool IsScanning { get; set; }
        public bool IsPlaying { get; set; }

        public SessionManager()
        {
        }

        public string Login() //string login, string password, string url
        {
            _driver.Navigate().GoToUrl("http://live.chess.com/live?v=2015052201");
            var userNameField = _driver.FindElementById("c1");
            var userPasswordField = _driver.FindElementById("loginpassword");
            var loginButton = _driver.FindElementById("btnLogin");      

            //_driver.Navigate().GoToUrl("67.201.34.165");
            //var userNameField = _driver.FindElementById("loginusername");
            //var userPasswordField = _driver.FindElementById("loginpassword");
            //var loginButton = _driver.FindElementById("btnLogin");

            userNameField.SendKeys("");
            userPasswordField.SendKeys("");

            loginButton.Click();

            var loggedAsFiled = _driver.FindElementByClassName("chess_com_username_link");
            return loggedAsFiled.Text;
            //return null;
        }

        public void StartScann(EngineHandler engineHandler, Process process)
        {
            //var engineHandler = new EngineHandler();
            var moveList = new List<string>();
            var resolvedMoveList = new List<string>(); //computer list moves with " " after each move 
            var count = 1;
            var span = new TimeSpan(0, 0, 0, 60, 0);
            var wait = new WebDriverWait(_driver, span);

            //TODO: start engineuser program

            while (true)
            {
                MovesHandlerInstance.Count = count;   
                try
                {
                    wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='movelist_" + count + "']/a")));
                }
                catch (Exception)
                {
                    engineHandler.KillEngineProcess(process);
                    HideConsoleWindow();
                    break;
                }
                moveList.Add(_driver.FindElementByXPath("//*[@id='movelist_" + count + "']/a").Text);
                resolvedMoveList.Add(_resolver.GetEngineMove(_resolver.GetComputerMove(moveList)));

                if (MovesHandlerInstance.PlayerToMove())
                {
                    ShowConsoleWindow();
                    Console.WriteLine(engineHandler.GetCalculateMove(process, resolvedMoveList));
                    MovesHandlerInstance.DoMove(engineHandler.GetCalculateMove(process, resolvedMoveList));
                }
                count++;
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
        static extern bool AllocConsole();

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
    }
}
