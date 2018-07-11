using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace MoreFactionInteraction
{
    public class FactionInteractionTimeSeperator
    {
        public static readonly SimpleCurve TimeBetweenInteraction = new SimpleCurve
        {
            new CurvePoint(x: 0, y: GenDate.TicksPerDay * (15 * MoreFactionInteraction_Settings.timeModifierBetweenFactionInteraction)),
            new CurvePoint(x: 50, y: GenDate.TicksPerDay * (8 * MoreFactionInteraction_Settings.timeModifierBetweenFactionInteraction)),
            new CurvePoint(x: 100, y: GenDate.TicksPerDay * (4 * MoreFactionInteraction_Settings.timeModifierBetweenFactionInteraction))
        };
    }
}
