using RimWorld;
using Verse;

namespace MoreFactionInteraction
{
    public class FactionInteractionTimeSeperator
    {
        public static readonly SimpleCurve TimeBetweenInteraction = new SimpleCurve
        {
            new CurvePoint(x: 0, y: GenDate.TicksPerDay * (20 * MoreFactionInteraction_Settings.timeModifierBetweenFactionInteraction)),
            new CurvePoint(x: 50, y: GenDate.TicksPerDay * (11 * MoreFactionInteraction_Settings.timeModifierBetweenFactionInteraction)),
            new CurvePoint(x: 100, y: GenDate.TicksPerDay * (7 * MoreFactionInteraction_Settings.timeModifierBetweenFactionInteraction))
        };
    }
}
