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


        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && Find.AnyPlayerHomeMap != null && (Find.FactionManager.RandomNonHostileFaction(false, false, false, TechLevel.Undefined) != null) && this.TryFindTile(out int num);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Faction faction = parms.faction;
            if (faction == null)
                faction = Find.FactionManager.RandomNonHostileFaction(false, false, false, TechLevel.Undefined);

            if (!this.TryFindTile(out int tile))
                return false;

            Site site = SiteMaker.MakeSite(SiteCoreDefOf.Nothing, MFI_DefOf.MFI_HuntersLodge, tile, faction, false);

            if (site == null)
                return false;

            if (!TryFindAnimalKind(tile, out PawnKindDef pawnKindDef))
                return false;
            
            if (pawnKindDef == null) pawnKindDef = PawnKindDefOf.Thrumbo; //mostly for testing.

            int randomInRange = IncidentWorker_HuntersLodge.TimeoutDaysRange.RandomInRange;
            site.Tile = tile;
            site.GetComponent<TimeoutComp>().StartTimeout(randomInRange * GenDate.TicksPerDay);
            site.SetFaction(faction);
            
            if(site.parts.First(x => x.def == MFI_DefOf.MFI_HuntersLodge).Def.Worker is SitePartWorker_MigratoryHerd sitePart)
                sitePart.pawnKindDef = pawnKindDef;

            Find.WorldObjects.Add(site);
            string text = string.Format(this.def.letterText, faction, faction.def.leaderTitle, pawnKindDef.GetLabelPlural(), randomInRange).CapitalizeFirst();
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
