using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ChessLogic
{
    public class PawnPromotion : Move
    {
        public override MoveType Type => MoveType.PawnPromotion;
        public override Position FromPos { get; }
        public override Position ToPos { get; }
        public PieceType PromotedTo { get; }
        public PawnPromotion(Position from, Position to, PieceType promotedTo)
        {
            FromPos = from;
            ToPos = to;
            PromotedTo = promotedTo;
        }
        private Piece CreatePromotionPiece(Player color)
        {
            if (PromotedTo == PieceType.Knight)
            {
                return new Knight(color);
            }
            else if (PromotedTo == PieceType.Bishop)
            {
                return new Bishop(color);
            }
            else if (PromotedTo == PieceType.Rook)
            {
                return new Rook(color);
            }
            else
            {
                return new Queen(color);
            }
        }
        // Remove pawn, create promoted piece and place it on ToPos
        public override bool Execute(Board board)
        {
            Piece pawn = board[FromPos];
            board[FromPos] = null;
            Piece promotionPiece = CreatePromotionPiece(pawn.Color);
            promotionPiece.HasMoved = true;
            board[ToPos] = promotionPiece;
            return true;
        }
    }
}