using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace MoreFactionInteraction
{
    class WorldObject_RoadConstruction : WorldObject
    {
        //Path ?
        //Tick until done
        //Road to be
        public int projectedTimeOfCompletion;
        public RoadDef road;
        public int nextTile;

        public override string GetInspectString() => $"Estimated time of completion: {(projectedTimeOfCompletion - Find.TickManager.TicksGame).ToStringTicksToPeriodVague()}";

        public override void Tick()
        {
            if (Find.TickManager.TicksGame > projectedTimeOfCompletion)
            {
                Messages.Message("MFI_RoadSectionCompleted", this, MessageTypeDefOf.TaskCompletion);
                Find.WorldGrid.OverlayRoad(this.Tile, this.nextTile, this.road);
                Find.WorldObjects.Remove(this);
                Find.World.renderer.RegenerateAllLayersNow();
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref projectedTimeOfCompletion, "projectedTimeOfCompletion");
            Scribe_Defs.Look(ref road, "roadDef");
            Scribe_Values.Look(ref nextTile, "nextTile");
        }
    }
}
