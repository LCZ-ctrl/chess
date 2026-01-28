using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class Position
    {
        public int Row { get; }
        public int Column { get; }

        public Position(int row, int col)
        {
            Row = row;
            Column = col;
        }

        public Player SquareColor()
        {
            if((Row + Column) % 2 == 0)
            {
                return Player.White;
            }
            return Player.Black;
        }

        public override bool Equals(object obj)
        {
            if(!(obj is Position pos))
            {
                return false;
            }
            return Row == pos.Row && Column == pos.Column;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Row, Column);
        }

        public static bool operator ==(Position left, Position right)
        {
            return EqualityComparer<Position>.Default.Equals(left, right);
        }

        public static bool operator !=(Position left, Position right)
        {
            return !(left == right);
        }

        public static Position operator +(Position pos, Direction dir)
        {
            return new Position(pos.Row + dir.RowDelta, pos.Column + dir.ColumnDelta);
        }
    }
}
