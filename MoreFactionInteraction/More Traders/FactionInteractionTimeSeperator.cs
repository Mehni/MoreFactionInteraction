using RimWorld;
using Verse;

namespace MoreFactionInteraction
{
    public static class FactionInteractionTimeSeperator
    {
        public static SimpleCurve TimeBetweenInteraction = new SimpleCurve
        {
            new CurvePoint(x: 0,   y: GenDate.TicksPerDay * 15),
            new CurvePoint(x: 50,  y: GenDate.TicksPerDay *  9),
            new CurvePoint(x: 100, y: GenDate.TicksPerDay *  5)
        };
    }
}
