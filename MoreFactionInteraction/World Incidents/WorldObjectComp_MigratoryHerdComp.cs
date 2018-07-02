using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction.World_Incidents
{
    public class WorldObjectComp_MigratoryHerdComp : WorldObjectComp
    {
        public PawnKindDef pawnKindDef;
        public IncidentParms parmesan;
        public Faction faction;

        public override string CompInspectStringExtra()
        {
            return "MFI_HuntersLodgeInspectString".Translate(new object[] { faction, pawnKindDef.GetLabelPlural() });
        }

        public override void PostMapGenerate()
        {
            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, Current.Game.FindMap(this.parent.Tile));
            QueuedIncident queuedIncident = new QueuedIncident(new FiringIncident(DefDatabase<IncidentDef>.GetNamed("MFI_HerdMigration_Ambush"), null, incidentParms), Find.TickManager.TicksGame + Rand.RangeInclusive(GenDate.TicksPerDay * 2, GenDate.TicksPerDay * 5));
            Find.Storyteller.incidentQueue.Add(queuedIncident);
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Defs.Look(ref pawnKindDef, "MFI_HuntersLodgepawnKindDef");
            Scribe_References.Look(ref faction, "MFI_HuntersLodgeFaction");
            Scribe_Deep.Look<IncidentParms>(ref this.parmesan, "MFI_HuntersLodgeincidentParms", new object[0]);
        }
    }
}
