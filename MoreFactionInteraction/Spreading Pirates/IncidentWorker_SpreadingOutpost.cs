using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace MoreFactionInteraction
{
    using JetBrains.Annotations;

    [UsedImplicitly]
    public class IncidentWorker_SpreadingOutpost : IncidentWorker
    {
        private Faction faction;
        private static List<Map> tmpAvailableMaps = new List<Map>();

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms: parms) && TryFindFaction(enemyFaction: out this.faction) 
                                                    && TileFinder.TryFindNewSiteTile(tile: out int tile, minDist: 8, maxDist: 30) 
                                                    && this.TryGetRandomAvailableTargetMap(out Map map);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!TryFindFaction(enemyFaction: out this.faction)) return false;
            if (!this.TryGetRandomAvailableTargetMap(map: out Map map)) return false;

            int pirateTile = this.RandomNearbyHostileSettlement(map.Tile)?.Tile ?? Tile.Invalid;

            if (pirateTile == Tile.Invalid) return false;

            if (!TileFinder.TryFindNewSiteTile(tile: out int tile, minDist: 2, maxDist: 8, allowCaravans: false, preferCloserTiles: true, nearThisTile: pirateTile)) return false;
            Site site = SiteMaker.MakeSite(core: SiteCoreDefOf.Nothing, sitePart: SitePartDefOf.Outpost, tile: tile, faction: this.faction);
            site.Tile = tile;
            Find.WorldObjects.Add(o: site);
            this.SendStandardLetter(lookTargets: site, relatedFaction: this.faction, textArgs: new string[]
            {
                this.faction.leader.LabelShort, this.faction.def.leaderTitle, this.faction.Name,
            });
            return true;
        }

        private SettlementBase RandomNearbyHostileSettlement(int originTile)
        {
            return (from settlement in Find.WorldObjects.SettlementBases
                    where settlement.Attackable && Find.WorldGrid.ApproxDistanceInTiles(firstTile: originTile, secondTile: settlement.Tile) < 36f 
                    && Find.WorldReachability.CanReach(startTile: originTile, destTile: settlement.Tile) && settlement.Faction == this.faction
                    select settlement).RandomElementWithFallback();
        }

        private static bool TryFindFaction(out Faction enemyFaction)
        {
            if (Find.FactionManager.AllFactions
                    .Where(x => !x.def.hidden && !x.defeated && x.HostileTo(other: Faction.OfPlayer) && x.def.permanentEnemy)
                    .TryRandomElement(result: out enemyFaction))
                return true;

            enemyFaction = null;
            return false;
        }

        private bool TryGetRandomAvailableTargetMap(out Map map)
        {
            tmpAvailableMaps.Clear();
            List<Map> maps = Find.Maps;
            foreach (Map potentialTargetMap in maps)
            {
                if (potentialTargetMap.IsPlayerHome && this.RandomNearbyHostileSettlement(originTile: potentialTargetMap.Tile) != null)
                {
                    tmpAvailableMaps.Add(item: potentialTargetMap);
                }
            }
            bool result = tmpAvailableMaps.TryRandomElement(result: out map);
            tmpAvailableMaps.Clear();
            return result;
        }
    }
}
