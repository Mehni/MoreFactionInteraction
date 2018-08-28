using System.Linq;
using UnityEngine;
using RimWorld;
using Verse;

namespace MoreFactionInteraction
{
    public static class FactionInteractionTimeSeperator
    {
        public static SimpleCurve TimeBetweenInteraction = new SimpleCurve
        {
            new CurvePoint(x: 0,   y: GenDate.TicksPerDay * 8 * Mathf.Max(1, Find.FactionManager.AllFactionsVisible.Count(f => !f.IsPlayer && !f.HostileTo(Faction.OfPlayer)))),
            new CurvePoint(x: 50,  y: GenDate.TicksPerDay * 5 * Mathf.Max(1, Find.FactionManager.AllFactionsVisible.Count(f => !f.IsPlayer && !f.HostileTo(Faction.OfPlayer)))),
            new CurvePoint(x: 100, y: GenDate.TicksPerDay * 3 * Mathf.Max(1, Find.FactionManager.AllFactionsVisible.Count(f => !f.IsPlayer && !f.HostileTo(Faction.OfPlayer))))
        };
    }
}