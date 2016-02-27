using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace BulletPlayerBackend.Utils
{
    public class MovesResolver
    {
        private int[] piMoveList;
        private int iTruncated;
        private Parser _resolver;

        readonly ArrayList _movesList = new ArrayList
        {
            "h1", "g1", "f1", "e1", "d1", "c1", "b1", "a1", 
            "h2", "g2", "f2", "e2", "d2", "c2", "b2", "a2", 
            "h3", "g3", "f3", "e3", "d3", "c3", "b3", "a3", 
            "h4", "g4", "f4", "e4", "d4", "c4", "b4", "a4", 
            "h5", "g5", "f5", "e5", "d5", "c5", "b5", "a5", 
            "h6", "g6", "f6", "e6", "d6", "c6", "b6", "a6",
            "h7", "g7", "f7", "e7", "d7", "c7", "b7", "a7",
            "h8", "g8", "f8", "e8", "d8", "c8", "b8", "a8"
        };

        public string[] GetComputerMove(List<string> moveList)
        {
            _resolver = new Parser(false);
            var listMovePos = new List<ChessBoard.MovePosS>();
            listMovePos = _resolver.ShortToLongMove(ChessBoard.PlayerColorE.White, moveList, out piMoveList, listMovePos, ref iTruncated);

            var move = new string[] { listMovePos[listMovePos.Count - 1].StartPos.ToString(), listMovePos[listMovePos.Count - 1].EndPos.ToString() };
            return move;
        }

        public string GetSuggestedSplittedMove(string[] computerMove)
        {
            var start = computerMove[0];
            var end = computerMove[1];
            start = _movesList[Int16.Parse(start)].ToString();
            end = _movesList[Int16.Parse(end)].ToString();
            return start + end + " ";
        }

        public List<string> GetShortMovesList(ChromeDriver driver, MovesHandler movesHandlerInstance)
        {
            var moveList = new List<string>();
            var span = new TimeSpan(0, 0, 0, 3, 0);
            var wait = new WebDriverWait(driver, span);

            movesHandlerInstance.Count = 1;
            while (true)
            {
                try
                {
                    wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(movesHandlerInstance.GetMovePath(movesHandlerInstance.Count))));
                }
                catch (Exception)
                {
                    return moveList;
                }
                moveList.Add(driver.FindElementByXPath(movesHandlerInstance.GetMovePath(movesHandlerInstance.Count)).Text); //
                movesHandlerInstance.Count++;
            }
        }

        public List<string> GetSuggestedLongMovesList(List<string> moveList)
        {
            if (moveList.Count == 0)
                return null;
            var resolvedLongMovesList = new List<string>();

            for (int i = 1; i <= moveList.Count; i++)
            {
                resolvedLongMovesList.Add(GetSuggestedSplittedMove(GetComputerMove(moveList.GetRange(0, i))));
            }
            return resolvedLongMovesList;
        }
    }
}
