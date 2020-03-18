using System;
using System.Collections.Generic;
using System.Linq;

namespace Checkers
{
    public enum CellType { _NONE, _EMPTY, _AVAILABLE, RED_, BLACK_, RED_K, BLACK_K }

    public class Game
    {
        private struct PickedCell
        {
            public CellType Type { get; set; }
            public int Row { get; set; }
            public int Col { get; set; }
        }

        private CellType prev = CellType._NONE;
        private CellType curr = CellType.RED_;
        private PickedCell picked;
        private bool canPickAnother = true;

        public delegate void OnChangeOnFront();
        public OnChangeOnFront ChangeOnFront;

        public char Turn { get; set; }

        public CellType[,] Map { get; private set; }

        public CellType PickedCellType { get { return picked.Type; } }

        private List<CellType> MapList
        {
            get
            {
                List<CellType> list = new List<CellType>();
                for (int row = 0; row < 8; row++)
                    for (int col = 0; col < 8; col++)
                        list.Add(Map[row, col]);
                return list;
            }
        }

        public string Victory
        {
            get
            {
                if (MapList.Count(c => c.ToString()[0] == 'B') == 0) return "Red";
                if (MapList.Count(c => c.ToString()[0] == 'R') == 0) return "Black";
                return null;
            }
        }

        public Game()
        {
            Map = new CellType[8, 8];

            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Map[row, col] = GetCellType(row, col);
                }
            }
            for (int row = 0; row < 3; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (Map[row, col] == CellType._EMPTY)
                        Map[row, col] = CellType.BLACK_;
                }
            }
            for (int row = 5; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    if (Map[row, col] == CellType._EMPTY)
                        Map[row, col] = CellType.RED_;
                }
            }
        }

        public void ChangeTurn()
        {
            if (Turn == 'R') Turn = 'B';
            else Turn = 'R';
        }

        public bool PickCell(int row, int col)
        {
            if (!canPickAnother) return false;
            if (Map[row, col].ToString()[0] == '_') return false;
            if (prev == CellType._NONE && Map[row, col] == CellType.BLACK_) return false;
            if ((prev == CellType._NONE && Map[row, col] == CellType.RED_) || ChangePick(row, col) || PickNextCell(row, col))
            {
                PickNewCell(row, col);
                return true;
            }
            return false;
        }

        private bool PickNextCell(int row, int col)
        {
            return Map[row, col].ToString()[0] != prev.ToString()[0] && !MapList.Any(c => c == CellType._AVAILABLE);
        }

        private void PickNewCell(int row, int col)
        {
            int step = Map[row, col].ToString().EndsWith("_K") ? 8 : 2;
            for(int stepRow = -step; stepRow < step + 1; stepRow++)
            {
                for (int stepCol = -step; stepCol < step + 1; stepCol++)
                {
                    if (stepRow == 0 || stepCol == 0 || Math.Abs(Math.Abs(stepCol) - Math.Abs(stepRow)) != 0) continue;
                    if (InRange(row + stepRow, col + stepCol))
                    {
                        if (Map[row + stepRow, col + stepCol] == CellType._EMPTY)
                        {
                            if ((stepRow > 0 && Map[row, col].ToString()[0] == 'B') || 
                                (stepRow < 0 && Map[row, col].ToString()[0] == 'R') ||
                                Map[row, col].ToString().EndsWith("_K") ||
                                CanHitBack(row, col))
                            {
                                Map[row + stepRow, col + stepCol] = CellType._AVAILABLE;
                                picked = new PickedCell { Row = row, Col = col, Type = Map[row, col] };
                                prev = Map[row, col];
                                curr = Map[row, col];
                            }
                        }
                    }
                }
            }
            for (row = 0; row < 8; row++)
            {
                for (col = 0; col < 8; col++)
                {
                    if (Map[row, col] == CellType._AVAILABLE)
                    {
                        if (picked.Type.ToString().EndsWith("_K"))
                        {
                            if (!ClearPath(picked.Row, picked.Col, row, col))
                            {
                                Map[row, col] = CellType._EMPTY;
                            }
                        }
                        else
                        {
                            if (!Beside(picked.Row, picked.Col, row, col) && !ViaEnemy(picked.Row, picked.Col, row, col))
                            {
                                Map[row, col] = CellType._EMPTY;
                            }
                        }
                    }
                }
            }
        }

        private bool CanHitBack(int row, int col)
        {
            if (Map[row, col] == CellType.RED_)
            {
                if (InRange(row + 1, col + 1) && InRange(row + 2, col + 2))
                    if (Map[row + 1, col + 1].ToString()[0] == 'B')
                        if (Map[row + 2, col + 2] == CellType._EMPTY)
                            return true;
                if (InRange(row + 1, col - 1) && InRange(row + 2, col - 2))
                    if (Map[row + 1, col - 1].ToString()[0] == 'B')
                        if (Map[row + 2, col - 2] == CellType._EMPTY)
                            return true;
            }
            else if (Map[row, col] == CellType.BLACK_)
            {
                if (InRange(row - 1, col + 1) && InRange(row - 2, col + 2))
                    if (Map[row - 1, col + 1].ToString()[0] == 'R')
                        if (Map[row - 2, col + 2] == CellType._EMPTY)
                            return true;
                if (InRange(row - 1, col - 1) && InRange(row - 2, col - 2))
                    if (Map[row - 1, col - 1].ToString()[0] == 'R')
                        if (Map[row - 2, col - 2] == CellType._EMPTY)
                            return true;
            }
            return false;
        }

        private bool ViaEnemy(int row1, int col1, int row2, int col2)
        {
            int dRow = Math.Abs(Math.Abs(row1) - Math.Abs(row2));
            int dCol = Math.Abs(Math.Abs(col1) - Math.Abs(col2));
            if (dRow != 2) return false;

            char curr = Map[row1, col1].ToString()[0];
            if (row2 > row1 && col2 > col1)
            {
                if (Map[row2 - 1, col2 - 1].ToString()[0] != curr && !Map[row2 - 1, col2 - 1].ToString().StartsWith("_")) return true;
            }
            else if (row2 < row1 && col2 < col1)
            {
                if (Map[row2 + 1, col2 + 1].ToString()[0] != curr && !Map[row2 + 1, col2 + 1].ToString().StartsWith("_")) return true;
            }
            else if (row2 > row1 && col2 < col1)
            {
                if (Map[row2 - 1, col2 + 1].ToString()[0] != curr && !Map[row2 - 1, col2 + 1].ToString().StartsWith("_")) return true;
            }
            else
            {
                if (Map[row2 + 1, col2 - 1].ToString()[0] != curr && !Map[row2 + 1, col2 - 1].ToString().StartsWith("_")) return true;
            }
            return false;
        }

        private bool Beside(int row1, int col1, int row2, int col2)
        {
            int dRow = Math.Abs(Math.Abs(row1) - Math.Abs(row2));
            int dCol = Math.Abs(Math.Abs(col1) - Math.Abs(col2));

            return dRow == 1 && dCol == 1;
        }

        private bool ClearDirection(CellType type, int row, int col, int dRow, int dCol, int steps)
        {
            int count = 0, k = 0;
            while (k < steps - 1)
            {
                if (type == CellType.RED_K)
                {
                    k++;
                    row += dRow;
                    col += dCol;
                    if (Map[row, col] == CellType.RED_ || Map[row, col] == CellType.RED_K) return false;
                    if (Map[row, col] == CellType.BLACK_ || Map[row, col] == CellType.BLACK_K) count++;
                    if (count > 1) return false;
                }
                else if (type == CellType.BLACK_K)
                {
                    k++;
                    row += dRow;
                    col += dCol;
                    if (Map[row, col] == CellType.BLACK_ || Map[row, col] == CellType.BLACK_K) return false;
                    if (Map[row, col] == CellType.RED_ || Map[row, col] == CellType.RED_K) count++;
                    if (count > 1) return false;
                }
                else continue;
            }
            return true;
        }

        private bool ClearPath(int row1, int col1, int row2, int col2)
        {
            CellType type = Map[row1, col1];
            int dRow = row2 - row1;
            int dCol = col2 - col1;
            if (dRow > 0 && dCol > 0)
            {
                return ClearDirection(type, row2, col2, -1, -1, row2 - Math.Abs(row1));
            }
            else if (dRow < 0 && dCol < 0)
            {
                return ClearDirection(type, row2, col2, 1, 1, row1 - Math.Abs(row2));
            }
            else if (dRow > 0 && dCol < 0)
            {
                return ClearDirection(type, row2, col2, -1, 1, row2 - Math.Abs(row1));
            }
            else// if(dRow < 0 && dCol > 0)
            {
                return ClearDirection(type, row2, col2, 1, -1, row1 - Math.Abs(row2));
            }
        }

        private bool InRange(int row, int col)
        {
            return row > -1 && col > -1 && row < 8 && col < 8;
        }

        private bool ChangePick(int row, int col)
        {
            bool res = curr.ToString()[0] == Map[row, col].ToString()[0] && Map[row, col].ToString()[0] == prev.ToString()[0];
            if (res)
                ResetPick();
            return res;
        }

        public bool PlaceCell(int row, int col)
        {
            if (MapList.Count(c => c == CellType._AVAILABLE) == 0) return false;
            if (row == picked.Row && col == picked.Col) return false;
            if (Map[row, col] == CellType._AVAILABLE)
            {
                Map[row, col] = picked.Type;
                if (picked.Type == CellType.RED_ && row == 0)
                {
                    Map[row, col] = CellType.RED_K;
                }
                if (picked.Type == CellType.BLACK_ && row == 7)
                {
                    Map[row, col] = CellType.BLACK_K;
                }
                Map[picked.Row, picked.Col] = CellType._EMPTY;
                curr = CellType._NONE;
                ResetPick();
                if (Hit(row, col))
                {
                    TryPickAgain(row, col);
                }
                return true;
            }
            return false;
        }

        private void TryPickAgain(int row, int col)
        {
            CheckDirection(row, col, -1, -1);
            CheckDirection(row, col, -1, 1);
            CheckDirection(row, col, 1, -1);
            CheckDirection(row, col, 1, 1);

            if (MapList.Any(c => c == CellType._AVAILABLE))
            {
                picked = new PickedCell { Row = row, Col = col, Type = Map[row, col] };
                prev = Map[row, col];
                curr = Map[row, col];
                canPickAnother = false;
                ChangeOnFront?.Invoke();
            }
            else
            {
                canPickAnother = true;
            }
        }

        private void CheckDirection(int row, int col, int dRow, int dCol)
        {
            int stepRow = row + dRow, stepCol = col + dCol;
            char picked = Map[row, col].ToString()[0];

            int eRow = 0, eCol = 0;
            bool found = false;
            while (InRange(stepRow, stepCol))
            {
                if (Map[stepRow, stepCol].ToString()[0] == picked) break;
                if (Map[stepRow, stepCol].ToString()[0] != '_' && Map[stepRow, stepCol].ToString()[0] != picked)
                {
                    if (InRange(stepRow + dRow, stepCol + dCol) && Map[stepRow + dRow, stepCol + dCol] == CellType._EMPTY)
                    {
                        eRow = stepRow;
                        eCol = stepCol;
                        found = true;
                        break;
                    }
                }
                stepRow += dRow;
                stepCol += dCol;
            }
            stepRow = row + dRow;
            stepCol = col + dCol;
            if (found)
            {
                if (Math.Abs(eRow - row) > 1 && (Map[row, col] == CellType.BLACK_ || Map[row, col] == CellType.RED_)) return;
                while (stepRow != eRow)
                {
                    Map[stepRow, stepCol] = CellType._AVAILABLE;
                    stepRow += dRow;
                    stepCol += dCol;
                }
                stepRow += dRow;
                stepCol += dCol;
                while (InRange(stepRow, stepCol) && Map[stepRow, stepCol] == CellType._EMPTY)
                {
                    Map[stepRow, stepCol] = CellType._AVAILABLE;
                    stepRow += dRow;
                    stepCol += dCol;
                    if (Map[row, col] == CellType.BLACK_ || Map[row, col] == CellType.RED_) return;
                }
            }
        }

        private bool TryHitInDirection(char type, int row, int col, int dRow, int dCol, int steps)
        {
            int k = 0;
            while (k < steps - 1)
            {
                k++;
                row += dRow;
                col += dCol;
                if (type == 'R')
                {
                    if (Map[row, col] == CellType.BLACK_ || Map[row, col] == CellType.BLACK_K)
                    {
                        Map[row, col] = CellType._EMPTY;
                        return true;
                    }
                }
                else
                {
                    if (Map[row, col] == CellType.RED_ || Map[row, col] == CellType.RED_K)
                    {
                        Map[row, col] = CellType._EMPTY;
                        return true;
                    }
                }
            }
            return false;
        }

        private bool Hit(int row2, int col2)
        {
            int row1 = picked.Row;
            int col1 = picked.Col;

            int dRow = row2 - row1;
            int dCol = col2 - col1;
            if (dRow > 0 && dCol > 0)
            {
                return TryHitInDirection(picked.Type.ToString()[0], row2, col2, -1, -1, row2 - Math.Abs(row1));
            }
            else if (dRow < 0 && dCol < 0)
            {
                return TryHitInDirection(picked.Type.ToString()[0], row2, col2, 1, 1, row1 - Math.Abs(row2));
            }
            else if (dRow > 0 && dCol < 0)
            {
                return TryHitInDirection(picked.Type.ToString()[0], row2, col2, -1, 1, row2 - Math.Abs(row1));
            }
            else// if (dRow < 0 && dCol > 0)
            {
                return TryHitInDirection(picked.Type.ToString()[0], row2, col2, 1, -1, row1 - Math.Abs(row2));
            }
        }

        public void ResetPick()
        {
            for (int row = 0; row < 8; row++)
                for (int col = 0; col < 8; col++)
                    if (Map[row, col] == CellType._AVAILABLE)
                        Map[row, col] = CellType._EMPTY;
        }

        private CellType GetCellType(int row, int col)
        {
            if (row % 2 == 0 && col % 2 == 0) return CellType._NONE;
            if (row % 2 != 0 && col % 2 == 0) return CellType._EMPTY;
            if (row % 2 != 0 && col % 2 != 0) return CellType._NONE;
            return CellType._EMPTY;
        }
    }
}