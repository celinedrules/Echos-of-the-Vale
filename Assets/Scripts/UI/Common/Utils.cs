using Utilities.Enums;

namespace UI.Common
{
    public static class Utils
    {
        public static bool IsPercentageStat(StatType type)
        {
            return type is StatType.CriticalChance or StatType.CriticalPower or StatType.ArmorReduction
                or StatType.IceResistance or StatType.FireResistance or StatType.LightningResistance
                or StatType.AttackSpeed or StatType.Evasion;
        }
    }
}