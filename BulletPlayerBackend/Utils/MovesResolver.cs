using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletPlayerBackend.Utils
{
    public class MovesResolver
    {
        private int[] piMoveList;
        private int iTruncated;
        private Parser _resolver;

        /// <summary>Chess board</summary>
        /// 63 62 61 60 59 58 57 56
        /// 55 54 53 52 51 50 49 48
        /// 47 46 45 44 43 42 41 40
        /// 39 38 37 36 35 34 33 32
        /// 31 30 29 28 27 26 25 24
        /// 23 22 21 20 19 18 17 16
        /// 15 14 13 12 11 10 9  8
        /// 7  6  5  4  3  2  1  0
        /// 
        ArrayList movesList = new ArrayList
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

        public string GetEngineMove(string[] computerMove)
        {
            var start = computerMove[0];
            var end = computerMove[1];
            start = movesList[Int16.Parse(start)].ToString();
            end = movesList[Int16.Parse(end)].ToString();
            return start + end + " ";
        }
    }
}
