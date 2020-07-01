using Verse;

namespace MoreFactionInteraction.MoreFactionWar
{
    public static class MFI_DiplomacyTunings
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

        public static readonly IntRange GoodWill_DeclinedMarriage_Impact = new IntRange(-5, -15);

        public static readonly SimpleCurve PawnValueInGoodWillAmountOut = new SimpleCurve
        {
            new CurvePoint(x: 0f,    y: 0),
            new CurvePoint(x: 500f,  y: 20),
            new CurvePoint(x: 1000f, y: 40),
            new CurvePoint(x: 2000f, y: 60),
            new CurvePoint(x: 4000f, y: 80)
        };

        public static Outcome FavourDisaster => new Outcome
        {
            goodwillChangeFavouredFaction = -GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange,
            goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
            setHostile = true,
            startWar = true
        };

        public static Outcome FavourBackfire => new Outcome
        {
            goodwillChangeFavouredFaction = -GoodWill_FactionWarPeaceTalks_ImpactSmall.RandomInRange,
            goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
            setHostile = true,
            startWar = true
        };

        public static Outcome FavourFlounder => new Outcome
        {
            goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange,
            goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange,
        };

        public static Outcome FavourSuccess => new Outcome
        {
            goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange,
            goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange,
        };

        public static Outcome FavourTriumph => new Outcome
        {
            goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
            goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactSmall.RandomInRange,
        };

        public static Outcome SabotageDisaster => new Outcome
        {
            goodwillChangeFavouredFaction = -GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
            goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
            setHostile = true,
            startWar = true,
        };

        public static Outcome SabotageBackfire => new Outcome
        {
            goodwillChangeFavouredFaction = -GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
            goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
        };

        public static Outcome SabotageFlounder => new Outcome();

        public static Outcome SabotageSuccess => new Outcome
        {
            goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange,
            goodwillChangeBurdenedFaction = GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange,
            startWar = true,
        };

        public static Outcome SabotageTriumph => new Outcome
        {
            goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
            goodwillChangeBurdenedFaction = GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
            startWar = true,
        };

        public static Outcome BrokerPeaceDisaster => new Outcome
        {
            goodwillChangeFavouredFaction = -GoodWill_FactionWarPeaceTalks_ImpactSmall.RandomInRange,
            goodwillChangeBurdenedFaction = -GoodWill_FactionWarPeaceTalks_ImpactSmall.RandomInRange,
            startWar = true,
        };

        //"rescheduled for later"
        public static Outcome BrokerPeaceBackfire => new Outcome();

        public static Outcome BrokerPeaceFlounder => new Outcome
        {
            goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange,
            goodwillChangeBurdenedFaction = GoodWill_FactionWarPeaceTalks_ImpactMedium.RandomInRange
        };

        public static Outcome BrokerPeaceSuccess => new Outcome
        {
            goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange,
            goodwillChangeBurdenedFaction = GoodWill_FactionWarPeaceTalks_ImpactBig.RandomInRange
        };


        public static Outcome BrokerPeaceTriumph => new Outcome
        {
            goodwillChangeFavouredFaction = GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange,
            goodwillChangeBurdenedFaction = GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange
        };
    }
}