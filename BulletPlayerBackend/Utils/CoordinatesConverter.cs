using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;

namespace BulletPlayerBackend.Utils
{
    ////class borrowed from http://www.codeproject.com/Articles/36112/Chess-Program-in-C
    /// <summary>Parser exception</summary>
    [Serializable]
    public class CoordinatesConverter : System.Exception
    {
        /// <summary>Code which is in error</summary>
        public string CodeInError;
        /// <summary>Array of move position</summary>
        public int[] MoveList;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="strMsg">           Error Message</param>
        /// <param name="strCodeInError">   Code in error</param>
        /// <param name="ex">               Inner exception</param>
        public CoordinatesConverter(string strMsg, string strCodeInError, Exception ex) : base(strMsg, ex) { CodeInError = strCodeInError; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="strMsg">           Error Message</param>
        /// <param name="strCodeInError">   Code in error</param>
        public CoordinatesConverter(string strMsg, string strCodeInError) : this(strMsg, strCodeInError, null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="strMsg">           Error Message</param>
        public CoordinatesConverter(string strMsg) : this(strMsg, "", null) { }

        /// <summary>
        /// Constructor
        /// </summary>
        public CoordinatesConverter() : this("", "", null) { }

        /// <summary>
        /// Unserialize additional data
        /// </summary>
        /// <param name="info">     Serialization Info</param>
        /// <param name="context">  Context Info</param>
        protected CoordinatesConverter(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            CodeInError = info.GetString("CodeInError");
            MoveList = (int[])info.GetValue("MoveList", typeof(int[]));
        }

        /// <summary>
        /// Serialize the additional data
        /// </summary>
        /// <param name="info">     Serialization Info</param>
        /// <param name="context">  Context Info</param>
        public override void GetObjectData(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("CodeInError", CodeInError);
            info.AddValue("MoveList", MoveList);
        }
    } // Class PgnParserException

    /// <summary>Class implementing the parsing of a PGN file. PGN is a standard way of recording chess games.</summary>
    public class Parser
    {

        /// <summary>Type of player (human of computer program)</summary>
        public enum PlayerTypeE
        {
            /// <summary>Player is a human</summary>
            Human,
            /// <summary>Player is a computer program</summary>
            Program
        };

        /// <summary>Text to parse</summary>
        private string m_strText;
        /// <summary>Starting position of a game</summary>
        private int m_iStartPos;
        /// <summary>Current position in text</summary>
        private int m_iPos;
        /// <summary>Size of the text</summary>
        private int m_iSize;
        /// <summary>Board use to play as we decode</summary>
        private ChessBoard m_chessBoard;
        /// <summary>true to diagnose the parser. This generate exception when a move cannot be resolved</summary>
        private bool m_bDiagnose;

        /// <summary>
        /// Class Ctor
        /// </summary>
        /// <param name="bDiagnose">    true to diagnose the parser</param>
        public Parser(bool bDiagnose)
        {
            m_chessBoard = new ChessBoard();
            m_bDiagnose = bDiagnose;
        }

        /// <summary>
        /// Return the code of the current game
        /// </summary>
        /// <returns>
        /// Current game
        /// </returns>
        private string GetCodeInError()
        {
            return (m_strText.Substring(m_iStartPos, m_iPos - m_iStartPos + 1));
        }

        /// <summary>
        /// Decode a move
        /// </summary>
        /// <param name="strPos">       Position</param>
        /// <param name="iStartCol">    Returns the starting column found in move if specified (-1 if not)</param>
        /// <param name="iStartRow">    Returns the starting row found in move if specified (-1 if not)</param>
        /// <param name="iEndPos">      Returns the ending position of the move</param>
        private void DecodeMove(string strPos, out int iStartCol, out int iStartRow, out int iEndPos)
        {
            switch (strPos.Length)
            {
                case 2:
                    if (strPos[0] < 'a' || strPos[0] > 'h' ||
                        strPos[1] < '1' || strPos[1] > '8')
                    {
                        throw new CoordinatesConverter("Unable to decode position", GetCodeInError());
                    }
                    iStartCol = -1;
                    iStartRow = -1;
                    iEndPos = (7 - (strPos[0] - 'a')) + ((strPos[1] - '1') << 3);
                    break;
                case 3:
                    if (strPos[0] >= 'a' && strPos[0] <= 'h')
                    {
                        iStartCol = 7 - (strPos[0] - 'a');
                        iStartRow = -1;
                    }
                    else if (strPos[0] >= '1' && strPos[0] <= '8')
                    {
                        iStartCol = -1;
                        iStartRow = (strPos[0] - '1');
                    }
                    else
                    {
                        throw new CoordinatesConverter("Unable to decode position", GetCodeInError());
                    }
                    if (strPos[1] < 'a' || strPos[1] > 'h' ||
                        strPos[2] < '1' || strPos[2] > '8')
                    {
                        throw new CoordinatesConverter("Unable to decode position", GetCodeInError());
                    }
                    iEndPos = (7 - (strPos[1] - 'a')) + ((strPos[2] - '1') << 3);
                    break;
                case 4:
                    if (strPos[0] < 'a' || strPos[0] > 'h' ||
                        strPos[1] < '1' || strPos[1] > '8' ||
                        strPos[2] < 'a' || strPos[2] > 'h' ||
                        strPos[3] < '1' || strPos[3] > '8')
                    {
                        throw new CoordinatesConverter("Unable to decode position", GetCodeInError());
                    }
                    iStartCol = 7 - (strPos[0] - 'a');
                    iStartRow = (strPos[1] - '1');
                    iEndPos = (7 - (strPos[2] - 'a')) + ((strPos[3] - '1') << 3);
                    break;
                default:
                    throw new CoordinatesConverter("Unable to decode position", GetCodeInError());
            }
        }

        /// <summary>
        /// Find a castle move
        /// </summary>
        /// <param name="ePlayerColor">     Color moving</param>
        /// <param name="bShortCastling">   true for short, false for long</param>
        /// <param name="iTruncated">       Truncated count</param>
        /// <param name="strMove">          Move</param>
        /// <param name="movePos">          Returned moved if found</param>
        /// <returns>
        /// Moving position or -1 if error
        /// </returns>
        private int FindCastling(ChessBoard.PlayerColorE ePlayerColor, bool bShortCastling, ref int iTruncated, string strMove, ref ChessBoard.MovePosS movePos)
        {
            int iRetVal = -1;
            int iWantedDelta;
            int iDelta;
            List<ChessBoard.MovePosS> arrMovePos;

            arrMovePos = m_chessBoard.EnumMoveList(ePlayerColor);
            iWantedDelta = bShortCastling ? 2 : -2;
            foreach (ChessBoard.MovePosS move in arrMovePos)
            {
                if ((move.Type & ChessBoard.MoveTypeE.MoveTypeMask) == ChessBoard.MoveTypeE.Castle)
                {
                    iDelta = ((int)move.StartPos & 7) - ((int)move.EndPos & 7);
                    if (iDelta == iWantedDelta)
                    {
                        iRetVal = (int)move.StartPos + ((int)move.EndPos << 8);
                        movePos = move;
                        m_chessBoard.DoMove(move);
                    }
                }
            }
            if (iRetVal == -1)
            {
                if (m_bDiagnose)
                {
                    throw new CoordinatesConverter("Unable to find compatible move - " + strMove, GetCodeInError());
                }
                iTruncated++;
            }
            return (iRetVal);
        }

        /// <summary>
        /// Find a move using the specification
        /// </summary>
        /// <param name="ePlayerColor">     Color moving</param>
        /// <param name="ePiece">           Piece moving</param>
        /// <param name="iStartCol">        Starting column of the move or -1 if not specified</param>
        /// <param name="iStartRow">        Starting row of the move or -1 if not specified</param>
        /// <param name="iEndPos">          Ending position of the move</param>
        /// <param name="eMoveType">        Type of move. Use for discriminating between different pawn promotion.</param>
        /// <param name="strMove">          Move</param>
        /// <param name="iTruncated">       Truncated count</param>
        /// <param name="movePos">          Move position</param>
        /// <returns>
        /// Moving position or -1 if error
        /// </returns>
        private int FindPieceMove(ChessBoard.PlayerColorE ePlayerColor, ChessBoard.PieceE ePiece, int iStartCol, int iStartRow, int iEndPos, ChessBoard.MoveTypeE eMoveType, string strMove, ref int iTruncated, ref ChessBoard.MovePosS movePos)
        {
            int iRetVal = -1;
            List<ChessBoard.MovePosS> arrMovePos;
            int iCol;
            int iRow;

            ePiece = ePiece | ((ePlayerColor == ChessBoard.PlayerColorE.Black) ? ChessBoard.PieceE.Black : ChessBoard.PieceE.White);
            arrMovePos = m_chessBoard.EnumMoveList(ePlayerColor);
            foreach (ChessBoard.MovePosS move in arrMovePos)
            {
                if ((int)move.EndPos == iEndPos && m_chessBoard[(int)move.StartPos] == ePiece)
                {
                    if (eMoveType == ChessBoard.MoveTypeE.Normal || (move.Type & ChessBoard.MoveTypeE.MoveTypeMask) == eMoveType)
                    {
                        iCol = (int)move.StartPos & 7;
                        iRow = (int)move.StartPos >> 3;
                        if ((iStartCol == -1 || iStartCol == iCol) &&
                            (iStartRow == -1 || iStartRow == iRow))
                        {
                            if (iRetVal != -1)
                            {
                                throw new CoordinatesConverter("More then one piece found for this move - " + strMove, GetCodeInError());
                            }
                            movePos = move;
                            iRetVal = (int)move.StartPos + ((int)move.EndPos << 8);
                            m_chessBoard.DoMove(move);
                        }
                    }
                }
            }
            if (iRetVal == -1)
            {
                if (m_bDiagnose)
                {
                    throw new CoordinatesConverter("Unable to find compatible move - " + strMove, GetCodeInError());
                }
                iTruncated++;
            }
            return (iRetVal);
        }

        /// <summary>
        /// Convert a PGN position into a moving position
        /// </summary>
        /// <param name="ePlayerColor">     Color moving</param>
        /// <param name="strMove">          Move</param>
        /// <param name="iPos">             Returned moving position</param>
        /// <param name="iTruncated">       Truncated count</param>
        /// <param name="movePos">          Move position</param>
        private void CnvRawMoveToPosMove(ChessBoard.PlayerColorE ePlayerColor, string strMove, out int iPos, ref int iTruncated, ref ChessBoard.MovePosS movePos)
        {
            string strPureMove;
            int iIndex;
            int iStartCol;
            int iStartRow;
            int iEndPos;
            int iOfs;
            ChessBoard.PieceE ePiece;
            ChessBoard.MoveTypeE eMoveType;

            eMoveType = ChessBoard.MoveTypeE.Normal;
            iPos = 0;
            strPureMove = strMove.Replace("x", "").Replace("#", "").Replace("ep", "").Replace("+", "");
            iIndex = strPureMove.IndexOf('=');
            if (iIndex != -1)
            {
                if (strPureMove.Length > iIndex + 1)
                {
                    switch (strPureMove[iIndex + 1])
                    {
                        case 'Q':
                            eMoveType = ChessBoard.MoveTypeE.PawnPromotionToQueen;
                            break;
                        case 'R':
                            eMoveType = ChessBoard.MoveTypeE.PawnPromotionToRook;
                            break;
                        case 'B':
                            eMoveType = ChessBoard.MoveTypeE.PawnPromotionToBishop;
                            break;
                        case 'N':
                            eMoveType = ChessBoard.MoveTypeE.PawnPromotionToKnight;
                            break;
                        case 'P':
                            eMoveType = ChessBoard.MoveTypeE.PawnPromotionToPawn;
                            break;
                        default:
                            iPos = -1;
                            iTruncated++;
                            break;
                    }
                    if (iPos != -1)
                    {
                        strPureMove = strPureMove.Substring(0, iIndex);
                    }
                }
                else
                {
                    iPos = -1;
                    iTruncated++;
                }
            }
            if (iPos == 0)
            {
                if (strPureMove == "O-O")
                {
                    iPos = FindCastling(ePlayerColor, true, ref iTruncated, strMove, ref movePos);
                }
                else if (strPureMove == "O-O-O")
                {
                    iPos = FindCastling(ePlayerColor, false, ref iTruncated, strMove, ref movePos);
                }
                else
                {
                    iOfs = 1;
                    switch (strPureMove[0])
                    {
                        case 'K':   // King
                            ePiece = ChessBoard.PieceE.King;
                            break;
                        case 'N':   // Knight
                            ePiece = ChessBoard.PieceE.Knight;
                            break;
                        case 'B':   // Bishop
                            ePiece = ChessBoard.PieceE.Bishop;
                            break;
                        case 'R':   // Rook
                            ePiece = ChessBoard.PieceE.Rook;
                            break;
                        case 'Q':   // Queen
                            ePiece = ChessBoard.PieceE.Queen;
                            break;
                        default:    // Pawn
                            ePiece = ChessBoard.PieceE.Pawn;
                            iOfs = 0;
                            break;
                    }
                    DecodeMove(strPureMove.Substring(iOfs), out iStartCol, out iStartRow, out iEndPos);
                    iPos = FindPieceMove(ePlayerColor, ePiece, iStartCol, iStartRow, iEndPos, eMoveType, strMove, ref iTruncated, ref movePos);
                }
            }
        }

        /// <summary>
        /// Convert a list of PGN positions into a moving positions
        /// </summary>
        /// <param name="eColorToPlay">     Color to play</param>
        /// <param name="arrRawMove">       Array of PGN moves</param> TODO: this is essiential parametr
        /// <param name="piMoveList">       Returned array of moving position</param>
        /// <param name="listMovePos">      Returned the list of move if not null</param>
        /// <param name="iSkip">            Skipped count</param>
        /// <param name="iTruncated">       Truncated count</param>
        public List<ChessBoard.MovePosS> ShortToLongMove(ChessBoard.PlayerColorE eColorToPlay, List<string> arrRawMove, out int[] piMoveList, List<ChessBoard.MovePosS> listMovePos, ref int iTruncated)
        {
            List<int> arrMoveList;
            ChessBoard.MovePosS movePos;
            int iPos;
            listMovePos = new List<ChessBoard.MovePosS>();

            movePos = new ChessBoard.MovePosS();
            movePos.StartPos = 0;
            movePos.EndPos = 0;
            movePos.Type = ChessBoard.MoveTypeE.Normal;
            arrMoveList = new List<int>(256);
            try
            {
                foreach (string strMove in arrRawMove)
                {
                    CnvRawMoveToPosMove(eColorToPlay, strMove, out iPos, ref iTruncated, ref movePos); //TODO: it convert strMove (e4, c6, Sf3..) to movePos(that's what we need)
                    if (iPos != -1)
                    {
                        arrMoveList.Add(iPos);
                        listMovePos.Add(movePos);
                        eColorToPlay = (eColorToPlay == ChessBoard.PlayerColorE.Black) ? ChessBoard.PlayerColorE.White : ChessBoard.PlayerColorE.Black;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            catch (CoordinatesConverter ex)
            {
                ex.MoveList = arrMoveList.ToArray();
                throw;
            }
            piMoveList = arrMoveList.ToArray();
            return listMovePos;
        }
    } // Class PgnParser
} // Namespace
