using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace BulletPlayerBackend.Utils
{
    public class MovesHandler
    {
        private readonly BackgroundWorker _play;
        private readonly MovesResolver _resolver;
        private readonly EngineHandler _engineHandler;
        private readonly ChromeDriver _driver;
        private readonly SessionManager _session;

        public int Count { get; set; }
        public Point TopLeftCorner { get; set; }
        public Point BottomRightCorner { get; set; }

        public MovesHandler(SessionManager session, ChromeDriver driver)
        {
            _resolver = new MovesResolver();
            _engineHandler = new EngineHandler();
            _driver = driver;
            _session = session;

            _play = new BackgroundWorker();
            _play.DoWork += _play_DoWork;
            _play.WorkerSupportsCancellation = true;
        }

        public void DoMove(string move)
        {
            var alphabet = "abcdefgh";
            var start = move.Substring(0, 2);
            var end = move.Substring(2, 2);
            int startX, startY, endX, endY;

            var d = (BottomRightCorner.X - TopLeftCorner.X) / 8;

            if (_session.PlayerColor == "white")
            {
                startX = (alphabet.LastIndexOf(start.Substring(0, 1)) * d) + TopLeftCorner.X + d / 2;
                startY = BottomRightCorner.Y - d * (Int32.Parse(start.Substring(1, 1)) - 1) - d / 2;
                endX = (alphabet.LastIndexOf(end.Substring(0, 1)) * d) + TopLeftCorner.X + d / 2;
                endY = BottomRightCorner.Y - d * (Int32.Parse(end.Substring(1, 1)) - 1) - d / 2;
            }
            else
            {
                alphabet = "hgfedcba";
                startX = (alphabet.LastIndexOf(start.Substring(0, 1)) * d) + TopLeftCorner.X + d / 2;
                startY = TopLeftCorner.Y + d * (Int32.Parse(start.Substring(1, 1)) - 1) + d / 2;
                endX = (alphabet.LastIndexOf(end.Substring(0, 1)) * d) + TopLeftCorner.X + d / 2;
                endY = TopLeftCorner.Y + d * (Int32.Parse(end.Substring(1, 1)) - 1) + d / 2;
            }

            LeftMouseClick(startX, startY);
            System.Threading.Thread.Sleep(10);
            LeftMouseClick(endX, endY);
        }

        private void _play_DoWork(object sender, DoWorkEventArgs e)
        {
            var span = new TimeSpan(0, 0, 0, 15, 0);
            var wait = new WebDriverWait(_driver, span);
            var shortMovesList = _resolver.GetShortMovesList(_driver, this);
            var resolvedLongMovesList = _resolver.GetSuggestedLongMovesList(shortMovesList);
            var process = e.Argument as Process;

            while (true)
            {
                if (_play.CancellationPending)
                {
                    e.Cancel = true;
                    _engineHandler.TurnEngineOff();
                    return;
                }
                if (PlayerToMove())
                {
                    var move = _engineHandler.GetCalculateMove(process, resolvedLongMovesList);
                    DoMove(move);
                }
                try
                {
                    wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(GetMovePath(Count))));
                }
                catch (Exception)
                {
                    _engineHandler.TurnEngineOff();
                    break;
                }
                shortMovesList.Add(_driver.FindElementByXPath(GetMovePath(Count)).Text);
                resolvedLongMovesList.Add(_resolver.GetSuggestedSplittedMove(_resolver.GetComputerMove(shortMovesList)));
                Count++;
            }
        }


        public void StartPlay(Process process)
        {
            _play.RunWorkerAsync(process);
        }

        public void StopPlay()
        {
            _play.CancelAsync();
        }

        private bool PlayerToMove()
        {
            if ((_session.PlayerColor.Equals("white") && (Count % 2) != 0) ||
                (_session.PlayerColor.Equals("black") && (Count % 2) == 0))
                return true;

            return false;
        }

        public string GetMovePath(int count)
        {
            if (_session.Website.Equals("chess.com"))
                return "//*[@id='movelist_" + count + "']/a";
            if (_session.Website.Equals("lichess.com"))
                return "//*[@data-ply='" + count + "']";
            return null;
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        public static void LeftMouseClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }
    }
}
