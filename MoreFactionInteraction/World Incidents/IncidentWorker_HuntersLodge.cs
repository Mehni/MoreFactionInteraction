using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

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
                                                    && Find.FactionManager.RandomAlliedFaction(allowHidden: false, allowDefeated: false, allowNonHumanlike: false) != null 
                                                    && TryFindTile(tile: out int num);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Faction faction = parms.faction ?? Find.FactionManager.RandomAlliedFaction(allowHidden: false, allowDefeated: false, allowNonHumanlike: false);

            if (faction == null)
            {
                faction = Find.FactionManager.RandomNonHostileFaction(allowNonHumanlike: false);
                Log.ErrorOnce("MFI: No allied faction found, but event was forced. Using random faction.", 40830425);
            }

            if (!TryFindTile(tile: out int tile))
                return false;

            Site site = SiteMaker.MakeSite(core: MFI_DefOf.MFI_HuntersLodgeCore, 
                                           sitePart: MFI_DefOf.MFI_HuntersLodgePart, 
                                           tile: tile, faction: faction, ifHostileThenMustRemainHostile: false);

            if (site == null)
                return false;

            int randomInRange = TimeoutDaysRange.RandomInRange;

            site.Tile = tile;
            site.GetComponent<TimeoutComp>().StartTimeout(ticks: randomInRange * GenDate.TicksPerDay);
            site.SetFaction(newFaction: faction);
            site.customLabel = site.def.LabelCap + site.parts.First(predicate: x => x.def == MFI_DefOf.MFI_HuntersLodgePart).def.Worker.GetPostProcessedThreatLabel(site, site.parts.FirstOrDefault());

            Find.WorldObjects.Add(o: site);

            string text = string.Format(format: def.letterText, 
                                        faction, 
                                        faction.def.leaderTitle, 
                                        SitePartUtility.GetDescriptionDialogue(site, site.parts.FirstOrDefault()), 
                                        randomInRange)
                                .CapitalizeFirst();

            Find.LetterStack.ReceiveLetter(label: def.letterLabel, text: text, textLetterDef: def.letterDef, lookTargets: site);
            return true;
        }

        private static bool TryFindTile(out int tile)
        {
            return TileFinder.TryFindNewSiteTile(tile: out tile, minDist: MinDistance, maxDist: MaxDistance, allowCaravans: true, preferCloserTiles: false);
        }
    }
}
