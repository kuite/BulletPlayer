using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;

namespace BulletPlayerBackend.Utils
{
    public class MovesHandler
    {
        public string PlayerColor { get; set; }
        public int Count { get; set; }

        public void DoMove(string move)
        {
            var alphabet = "abcdefgh";
            var start = move.Substring(0, 2);
            var end = move.Substring(2, 2);
            int startX, startY, endX, endY;
            if (PlayerColor == "white")
            {
                startX = (alphabet.LastIndexOf(start.Substring(0, 1)) * 83) + 100;
                startY = 800 - 83 * (Int32.Parse(start.Substring(1, 1)) - 1);
                endX = (alphabet.LastIndexOf(end.Substring(0, 1)) * 83) + 100;
                endY = 800 - 83 * (Int32.Parse(end.Substring(1, 1)) - 1);
            }
            else
            {
                alphabet = "hgfedcba";
                startX = (alphabet.LastIndexOf(start.Substring(0, 1)) * 83) + 100;
                startY = 219 + 83 * (Int32.Parse(start.Substring(1, 1)) - 1);
                endX = (alphabet.LastIndexOf(end.Substring(0, 1)) * 83) + 100;
                endY = 219 + 83 * (Int32.Parse(end.Substring(1, 1)) - 1);  
            }

            LeftMouseClick(startX, startY - 25);
            System.Threading.Thread.Sleep(100);
            LeftMouseClick(endX, endY - 25);
        }

        public bool PlayerToMove()
        {
            if ((PlayerColor.Equals("white") && (Count % 2) != 0) ||
                (PlayerColor.Equals("black") && (Count % 2) == 0))
                return true;

            return false;
        }

        public bool WhiteToMove()
        {
            return (Count % 2) == 0;
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
