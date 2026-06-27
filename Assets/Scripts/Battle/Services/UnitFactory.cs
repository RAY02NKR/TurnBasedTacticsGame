using TurnBasedGame.Battle.Units;

namespace TurnBasedGame.Battle.Services
{
    public static class UnitFactory
    {
        public static UnitRuntime Create(UnitDefinition def)
            => new UnitRuntime(
                  def.unitName, def.team,
                  def.maxHP, def.attack, def.speed, def.step,
                  def.basicAttack, def.activeSkills,
                  def.resistSlash, def.resistPierce, def.resistBlunt
                );
    }
}