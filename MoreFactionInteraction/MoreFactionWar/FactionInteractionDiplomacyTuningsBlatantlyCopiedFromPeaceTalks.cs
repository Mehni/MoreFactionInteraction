using Verse;


namespace MoreFactionInteraction.MoreFactionWar
{
    public class FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
    {
        public static readonly SimpleCurve BadOutcomeFactorAtStatPower = new SimpleCurve
        {
            new CurvePoint(x: 0f, y: 4f),
            new CurvePoint(x: 1f, y: 1f),
            new CurvePoint(x: 1.5f, y: 0.4f)
        };

        public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactHuge = new IntRange(min: 50, max: 80);
        public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactBig = new IntRange(min: 30, max: 70);
        public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactMedium = new IntRange(min: 20, max: 50);
        public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactSmall = new IntRange(min: 10, max: 30);

        public static readonly SimpleCurve PawnValueInGoodWillAmountOut = new SimpleCurve
        {
          new CurvePoint(x: 0f,    y: 0),
          new CurvePoint(x: 500f,  y: 20),
          new CurvePoint(x: 1000f, y: 40),
          new CurvePoint(x: 2000f, y: 60),
          new CurvePoint(x: 4000f, y: 80)
        };
    }
}
