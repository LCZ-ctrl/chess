using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessLogic
{
    public class Direction
    {
        public static readonly Direction North = new Direction(-1, 0);
        public static readonly Direction South = new Direction(1, 0);
        public static readonly Direction West = new Direction(0, -1);
        public static readonly Direction East = new Direction(0, 1);
        public static readonly Direction NorthWest = North + West;
        public static readonly Direction NorthEast = North + East;
        public static readonly Direction SouthWest = South + West;
        public static readonly Direction SouthEast = South + East;

        public int RowDelta { get; }
        public int ColumnDelta { get; }

        public Direction(int rowDelta, int colDelta)
        {
            RowDelta = rowDelta;
            ColumnDelta = colDelta;
        }

        public static Direction operator+(Direction d1, Direction d2)
        {
            return new Direction(d1.RowDelta + d2.RowDelta, d1.ColumnDelta + d2.ColumnDelta);
        }

        public static Direction operator *(int scalar, Direction dir)
        {
            return new Direction(scalar * dir.RowDelta, scalar * dir.ColumnDelta);
        }
    }
}
