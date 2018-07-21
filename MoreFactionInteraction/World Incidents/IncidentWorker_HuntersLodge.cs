using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;
//using Kitchen.Sink;

namespace MoreFactionInteraction.World_Incidents
{
    public class IncidentWorker_HuntersLodge : IncidentWorker
    {
        private const int MinDistance = 2;
        private const int MaxDistance = 15;

        private static readonly IntRange TimeoutDaysRange = new IntRange(min: 15, max: 25);


        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms: parms) && Find.AnyPlayerHomeMap != null 
                                                    && Find.FactionManager.RandomNonHostileFaction(allowHidden: false, allowDefeated: false, allowNonHumanlike: false) != null 
                                                    && TryFindTile(tile: out int num);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Faction faction = parms.faction ?? Find.FactionManager.RandomNonHostileFaction(allowHidden: false, allowDefeated: false, allowNonHumanlike: false);

            if (!TryFindTile(tile: out int tile))
                return false;

            Site site = SiteMaker.MakeSite(core: MFI_DefOf.MFI_HuntersLodgeCore, sitePart: MFI_DefOf.MFI_HuntersLodgePart, tile: tile, faction: faction, ifHostileThenMustRemainHostile: false);

            if (site == null)
                return false;

            if (!this.TryFindAnimalKind(tile: tile, animalKind: out PawnKindDef pawnKindDef))
                return false;
            
            if (pawnKindDef == null) pawnKindDef = PawnKindDefOf.Thrumbo; //mostly for testing.

            int randomInRange = TimeoutDaysRange.RandomInRange;
            site.Tile = tile;
            site.GetComponent<TimeoutComp>().StartTimeout(ticks: randomInRange * GenDate.TicksPerDay);
            site.SetFaction(newFaction: faction);
            
            if(site.parts.First(predicate: x => x.def == MFI_DefOf.MFI_HuntersLodgePart).Def.Worker is SitePartWorker_MigratoryHerd sitePart)
                sitePart.pawnKindDef = pawnKindDef;

            Find.WorldObjects.Add(o: site);
            string text = string.Format(format: this.def.letterText, faction, faction.def.leaderTitle, pawnKindDef.GetLabelPlural(), randomInRange).CapitalizeFirst();
            Find.LetterStack.ReceiveLetter(label: this.def.letterLabel, text: text, textLetterDef: this.def.letterDef, lookTargets: site);
            return true;
        }

        private bool TryFindAnimalKind(int tile, out PawnKindDef animalKind)
        {
            return (from k in DefDatabase<PawnKindDef>.AllDefs
                    where k.RaceProps.CanDoHerdMigration && Find.World.tileTemperatures.SeasonAndOutdoorTemperatureAcceptableFor(tile: tile, animalRace: k.race)
                    select k).TryRandomElementByWeight(weightSelector: (PawnKindDef x) => x.RaceProps.wildness, result: out animalKind);
        }

        private static bool TryFindTile(out int tile)
        {
            return TileFinder.TryFindNewSiteTile(tile: out tile, minDist: MinDistance, maxDist: MaxDistance, allowCaravans: true, preferCloserTiles: false);
        }
    }
}
