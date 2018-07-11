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
            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incCat: IncidentCategoryDefOf.Misc, target: map);
            QueuedIncident queuedIncident = new QueuedIncident(firingInc: new FiringIncident(def: DefDatabase<IncidentDef>.GetNamed(defName: "MFI_HerdMigration_Ambush"), source: null, parms: incidentParms), fireTick: Find.TickManager.TicksGame + Rand.RangeInclusive(min: GenDate.TicksPerDay * 2, max: GenDate.TicksPerDay * 5));
            Find.Storyteller.incidentQueue.Add(qi: queuedIncident);
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
