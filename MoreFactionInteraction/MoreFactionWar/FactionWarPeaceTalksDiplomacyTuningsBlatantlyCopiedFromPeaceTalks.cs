using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;


namespace MoreFactionInteraction.MoreFactionWar
{
    public class FactionWarPeaceTalksDiplomacyTuningsBlatantlyCopiedFromPeaceTalks
    {
        public static readonly SimpleCurve BadOutcomeFactorAtDiplomacyPower = new SimpleCurve
        {
            {
                new CurvePoint(0f, 4f),
                true
            },
            {
                new CurvePoint(1f, 1f),
                true
            },
            {
                new CurvePoint(1.5f, 0.4f),
                true
            }
        };

        public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactHuge = new IntRange(50, 80);
        public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactBig = new IntRange(30, 70);
        public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactMedium = new IntRange(20, 50);
        public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactSmall = new IntRange(10, 30);
    }
}
