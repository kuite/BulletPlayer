using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using BulletPlayerBackend.Utils;
using Gma.UserActivityMonitor;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace BulletPlayerBackend
{
    public class SessionManager
    {
        private static int _i = 0;
        private readonly ChromeDriver _driver;
        private readonly WindowsForm _windows;
        private readonly EngineHandler _engineHandler;

        public string Website;
        public string PlayerColor;
        public readonly MovesHandler MovesHandlerInstance;

        public SessionManager(WindowsForm windows)
        {
            _driver = new ChromeDriver();
            MovesHandlerInstance = new MovesHandler(this, _driver);
            _engineHandler = new EngineHandler();
            _windows = windows;
        }

        public void Login(string username, string password, string site)
        {
            IWebElement[] model = null;
            Website = site;

            if (site.Equals("chess.com"))
                model = ChessComLoginModel();

            if (site.Equals("lichess.com"))
                model = LiChessLoginModel();

            if (model == null)
                return;

            var userNameField = model[0];
            var userPasswordField = model[1];
            var loginButton = model[2];

            userNameField.SendKeys(username);
            userPasswordField.SendKeys(password);
            loginButton.Click();
        }

        private IWebElement[] LiChessLoginModel()
        {
            _driver.Navigate().GoToUrl("http://en.lichess.org/login");
            var ret = new IWebElement[3];

            ret[0] = _driver.FindElementById("username");
            ret[1] = _driver.FindElementById("password");
            ret[2] = _driver.FindElement(By.XPath("//*[@class='submit button']"));

            return ret;
        }

        private IWebElement[] ChessComLoginModel()
        {
            _driver.Navigate().GoToUrl("http://live.chess.com/live?v=2015052201");
            var ret = new IWebElement[3];

            ret[0] = _driver.FindElementById("c1");
            ret[1] = _driver.FindElementById("loginpassword");
            ret[2] = _driver.FindElementById("btnLogin");

            return ret;
        }

        public void SetBoardCoordinates()
        {
            HookManager.MouseDown += mouseDown_SetCoordinates;
        }

        private void mouseDown_SetCoordinates(object sender, MouseEventArgs e) //this is event handler, e is instance of triggered event
        {
            switch (_i)
            {
                case 0:
                    MovesHandlerInstance.TopLeftCorner = new Point(e.X, e.Y);
                    _i = 1;
                    return;
                case 1:
                    MovesHandlerInstance.BottomRightCorner = new Point(e.X, e.Y);
                    HookManager.MouseDown -= mouseDown_SetCoordinates;
                    _i++;
                    break;
            }
        }

        public void StartPlaying()
        {
            var process = _engineHandler.TurnEngineOn();
            try
            {
                MovesHandlerInstance.StartPlay(process);
                _windows.SetNotification("BUSY ENGINE");
            }
            catch (Exception e)
            {
                //can not start _play thread
            }

        }

        public void StopPlaying()
        {
            _engineHandler.TurnEngineOff();
            MovesHandlerInstance.StopPlay();
        }
    }
}