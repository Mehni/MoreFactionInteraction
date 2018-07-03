using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction.World_Incidents
{
    public class SitePartWorker_MigratoryHerd : SitePartWorker
    {
        public PawnKindDef pawnKindDef;
        //public IncidentParms parmesan;
        //public Faction faction;

        //public override string CompInspectStringExtra()
        //{
        //    return "MFI_HuntersLodgeInspectString".Translate(new object[] { faction, pawnKindDef.GetLabelPlural() });
        //}

        public override void PostMapGenerate(Map map)
        {
            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, map);
            QueuedIncident queuedIncident = new QueuedIncident(new FiringIncident(DefDatabase<IncidentDef>.GetNamed("MFI_HerdMigration_Ambush"), null, incidentParms), Find.TickManager.TicksGame + Rand.RangeInclusive(GenDate.TicksPerDay * 2, GenDate.TicksPerDay * 5));
            Find.Storyteller.incidentQueue.Add(queuedIncident);
        }



        //public override void PostExposeData()
        //{
        //    base.PostExposeData();
        //    Scribe_Defs.Look(ref pawnKindDef, "MFI_HuntersLodgepawnKindDef");
        //    Scribe_References.Look(ref faction, "MFI_HuntersLodgeFaction");
        //    Scribe_Deep.Look<IncidentParms>(ref this.parmesan, "MFI_HuntersLodgeincidentParms", new object[0]);
        //}
    }

    //class WorldObjectCompProperties_MigratoryHerd : WorldObjectCompProperties
    //{
    //    public WorldObjectCompProperties_MigratoryHerd()
    //    {
    //        this.compClass = typeof(SitePartWorker_MigratoryHerd);
    //    }
    //}
}
