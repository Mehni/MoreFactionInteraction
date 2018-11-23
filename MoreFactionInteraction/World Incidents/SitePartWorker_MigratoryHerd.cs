using RimWorld;
using RimWorld.Planet;
using Verse;
using System.Linq;
using UnityEngine;

namespace MoreFactionInteraction.World_Incidents
{
    public class SitePartWorker_MigratoryHerd : SitePartWorker
    {
        public override string GetPostProcessedThreatLabel(Site site, SiteCoreOrPartBase siteCoreOrPart)
        {
            return string.Concat(base.GetPostProcessedThreatLabel(site, siteCoreOrPart),
                                     " (",
                                     GenLabel.BestKindLabel(siteCoreOrPart.parms.animalKind, Gender.None, true),
                                     ")"
                                 );
        }

        public override void PostMapGenerate(Map map)
        {
            IncidentParms incidentParms = StorytellerUtility.DefaultParmsNow(incCat: IncidentCategoryDefOf.Misc, target: map);
            incidentParms.forced = true;
            //this part is forced to bypass CanFireNowSub, to solve issue with scenario-added incident.
            QueuedIncident queuedIncident = new QueuedIncident(firingInc: new FiringIncident(def: DefDatabase<IncidentDef>.GetNamed(defName: "MFI_HerdMigration_Ambush"), source: null, parms: incidentParms), fireTick: Find.TickManager.TicksGame + Rand.RangeInclusive(min: GenDate.TicksPerDay / 2, max: GenDate.TicksPerDay));
            Find.Storyteller.incidentQueue.Add(qi: queuedIncident);
        }

        public override string GetPostProcessedDescriptionDialogue(Site site, SiteCoreOrPartBase siteCoreOrPart)
        {
            return string.Format(base.GetPostProcessedDescriptionDialogue(site, siteCoreOrPart), GenLabel.BestKindLabel(siteCoreOrPart.parms.animalKind, Gender.None, true));
        }

        private bool TryFindAnimalKind(int tile, out PawnKindDef animalKind)
        {
            PawnKindDef fallback = PawnKindDefOf.Thrumbo;

            animalKind = (from k in DefDatabase<PawnKindDef>.AllDefs
                    where k.RaceProps.CanDoHerdMigration && Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile: tile, animalRace: k.race)
                    select k).RandomElementByWeightWithFallback(weightSelector: (PawnKindDef x) => x.RaceProps.wildness, fallback);

            return animalKind != fallback;
        }

        public override SiteCoreOrPartParams GenerateDefaultParams(Site site, float myThreatPoints)
        {
            SiteCoreOrPartParams siteCoreOrPartParams = base.GenerateDefaultParams(site, myThreatPoints);
            if (TryFindAnimalKind(site.Tile, out siteCoreOrPartParams.animalKind))
            {
                siteCoreOrPartParams.threatPoints = Mathf.Max(siteCoreOrPartParams.threatPoints, siteCoreOrPartParams.animalKind.combatPower);
            }
            return siteCoreOrPartParams;
        }
    }
}
