using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulletPlayerBackend.Utils
{
    public class MovesHandler
    {
        public string PlayerColor { get; set; }
        public int Count { get; set; }

        public void DoMove(string move)
        {
            //TODO: make mouse move


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
    }
}
