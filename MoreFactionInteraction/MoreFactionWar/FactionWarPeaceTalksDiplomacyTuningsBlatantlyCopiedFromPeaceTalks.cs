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
                new CurvePoint(x: 0f, y: 4f),
                true
            },
            {
                new CurvePoint(x: 1f, y: 1f),
                true
            },
            {
                new CurvePoint(x: 1.5f, y: 0.4f),
                true
            }
        };

        public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactHuge = new IntRange(min: 50, max: 80);
        public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactBig = new IntRange(min: 30, max: 70);
        public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactMedium = new IntRange(min: 20, max: 50);
        public static readonly IntRange GoodWill_FactionWarPeaceTalks_ImpactSmall = new IntRange(min: 10, max: 30);
    }
}
