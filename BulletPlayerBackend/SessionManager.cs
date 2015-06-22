using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
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
        private readonly BackgroundWorker _play;
        private readonly WindowsForm _window;
        public readonly EngineHandler EngineHandlerInstance = new EngineHandler();
        public readonly MovesHandler MovesHandlerInstance = new MovesHandler();

        //public bool IsScanning { get; set; }
        public bool IsPlaying { get; set; }

        public SessionManager(WindowsForm window)
        {
            _window = window;
            _driver = new ChromeDriver();
            _resolver = new MovesResolver();
            _play = new BackgroundWorker();

            _play.DoWork += new DoWorkEventHandler(_play_DoWork);
            _play.WorkerSupportsCancellation = true;
        }

        public string Login(string username, string password)
        {
            _driver.Navigate().GoToUrl("http://live.chess.com/live?v=2015052201");
            var userNameField = _driver.FindElementById("c1");
            var userPasswordField = _driver.FindElementById("loginpassword");
            var loginButton = _driver.FindElementById("btnLogin");

            userNameField.SendKeys(username);
            userPasswordField.SendKeys(password);

            loginButton.Click();

            var loggedAsFiled = _driver.FindElementByClassName("chess_com_username_link");
            return loggedAsFiled.Text;
        }

        public void StartPlaying(Process process)
        {
            try
            {
                _play.RunWorkerAsync(process);
            }
            catch (Exception e)
            {
                _window.SetMsg("Engine still working, try again in 10s...");
                System.Threading.Thread.Sleep(10000);
            }

        }

        public void StopPlaying()
        {
            _play.CancelAsync();
        }

        private void _play_DoWork(object sender, DoWorkEventArgs e)
        {
            var span = new TimeSpan(0, 0, 0, 60, 0);
            var wait = new WebDriverWait(_driver, span);
            var shortMovesList = _resolver.GetShortMovesList(_driver, MovesHandlerInstance);
            var resolvedLongMovesList = _resolver.GetSuggestedLongMovesList(shortMovesList);
            var process = e.Argument as Process;

            while (true)
            {
                if (_play.CancellationPending)
                {
                    e.Cancel = true;
                    EngineHandlerInstance.KillEngineProcess(process);
                    return;
                }
                if (MovesHandlerInstance.PlayerToMove())
                {
                    var move = EngineHandlerInstance.GetCalculateMove(process, resolvedLongMovesList);
                    MovesHandlerInstance.DoMove(move);
                }
                try
                {
                    wait.Until(ExpectedConditions.ElementIsVisible(By.XPath("//*[@id='movelist_" + MovesHandlerInstance.Count + "']/a")));
                }
                catch (Exception)
                {
                    EngineHandlerInstance.KillEngineProcess(process);
                    break;
                }
                shortMovesList.Add(_driver.FindElementByXPath("//*[@id='movelist_" + MovesHandlerInstance.Count + "']/a").Text);
                resolvedLongMovesList.Add(_resolver.GetSuggestedSplittedMove(_resolver.GetComputerMove(shortMovesList)));
                MovesHandlerInstance.Count++;
            }
        }
    }
}