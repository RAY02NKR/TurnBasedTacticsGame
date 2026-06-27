using System.Text;
using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Grid.Debug
{
    public static class BattleGridDebugFormatter
    {
        public static string Format(BattleGrid grid)
        {
            if (grid == null)
                return "[Grid: null]";

            var sb = new StringBuilder();

            sb.Append("   ");
            for (int x = 0; x < grid.Width; x++)
            {
                sb.Append($"{x,2} ");
            }
            sb.AppendLine();

            for (int y = grid.Height - 1; y >= 0; y--)
            {
                sb.Append($"{y,2} ");

                for (int x = 0; x < grid.Width; x++)
                {
                    var pos = new GridPosition(x, y);

                    if (grid.TryGetUnitAt(pos, out var unit))
                    {
                        sb.Append(GetUnitToken(unit));
                    }
                    else
                    {
                        sb.Append(" . ");
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string GetUnitToken(UnitRuntime unit)
        {
            if (unit == null)
                return " ? ";

            if (unit.IsTurret)
            {
                return unit.Team == Team.Ally ? " T " : " t ";
            }

            return unit.Team == Team.Ally ? " A " : " E ";
        }
    }
}