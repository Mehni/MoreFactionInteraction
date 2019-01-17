using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using RimWorld.Planet;

namespace MoreFactionInteraction
{
    public class WorldObject_RoadConstruction : WorldObject
    {
        //Path ?
        //Tick until done
        //Road to be
        public int projectedTimeOfCompletion;
        public RoadDef road;
        public int nextTile;

        public override string GetInspectString() => $"Estimated time of completion: {(projectedTimeOfCompletion - Find.TickManager.TicksGame).ToStringTicksToPeriodVague(false)}";

        public override void Tick()
        {
            if (Find.TickManager.TicksGame > projectedTimeOfCompletion)
            {
                Messages.Message("MFI_RoadSectionCompleted", this, MessageTypeDefOf.TaskCompletion);
                Find.WorldGrid.OverlayRoad(this.Tile, this.nextTile, this.road);
                Find.WorldObjects.Remove(this);
                Find.World.renderer.SetDirty<WorldLayer_Roads>();
                Find.World.renderer.SetDirty<WorldLayer_Paths>();
                Find.WorldPathGrid.RecalculatePerceivedMovementDifficultyAt(this.Tile);
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
