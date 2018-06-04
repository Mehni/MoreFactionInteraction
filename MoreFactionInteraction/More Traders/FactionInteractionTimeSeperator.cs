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
            {
                new CurvePoint(0, GenDate.TicksPerDay * 15),
                true
            },
            {
                new CurvePoint(50, GenDate.TicksPerDay * 8),
                true
            },
            {
                new CurvePoint(100, GenDate.TicksPerDay * 4),
                true
            }
        };
    }
}
