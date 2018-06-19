using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using RimWorld.BaseGen;
using Verse;
//using Kitchen.Sink;
using Verse.AI;
using Verse.AI.Group;

namespace MoreFactionInteraction.World_Incidents
{
    public class IncidentWorker_HuntersLodge : IncidentWorker
    {
        private const float NoSitePartChance = 0.3f;
        private const int MinDistance = 2;
        private const int MaxDistance = 15;

        private static readonly IntRange TimeoutDaysRange = new IntRange(15, 25);
        private Faction faction;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && Find.AnyPlayerHomeMap != null && this.TryFindTile(out int num) && SiteMakerHelper.TryFindRandomFactionFor(MFI_DefOf.HuntersLodge, null, out faction, true, null);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!this.TryFindTile(out int tile))
            {
                return false;
            }
            Site site = SiteMaker.TryMakeSite_SingleSitePart(MFI_DefOf.HuntersLodge, singleSitePartTag: null, faction, false, null);
            if (site == null)
            {
                return false;
            }
            site.Tile = tile;
            if (!TryFindAnimalKind(tile, out PawnKindDef pawnKindDef))
            {
                return false;
            }

            int randomInRange = IncidentWorker_HuntersLodge.TimeoutDaysRange.RandomInRange;
            site.GetComponent<TimeoutComp>().StartTimeout(randomInRange * GenDate.TicksPerDay);
            site.SetFaction(faction);
            site.GetComponent<MigratoryHerdComp>().pawnKindDef = pawnKindDef;
            site.GetComponent<MigratoryHerdComp>().parmesan = parms;

            Find.WorldObjects.Add(site);
            string text = string.Format(this.def.letterText, pawnKindDef.label, randomInRange).CapitalizeFirst();

            Find.LetterStack.ReceiveLetter(this.def.letterLabel, text, this.def.letterDef, site, null);
            return true;
        }

        private bool TryFindAnimalKind(int tile, out PawnKindDef animalKind)
        {
            return (from k in DefDatabase<PawnKindDef>.AllDefs
                    where k.RaceProps.CanDoHerdMigration && Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile, k.race)
                    select k).TryRandomElementByWeight((PawnKindDef x) => x.RaceProps.wildness, out animalKind);
        }

        private bool TryFindTile(out int tile)
        {
            return TileFinder.TryFindNewSiteTile(out tile, MinDistance, MaxDistance, true, false);
        }
    }
}
