using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;
using Harmony;
using RimWorld.Planet;

namespace MoreFactionInteraction
{
    public class IncidentWorker_SpreadingOutpost : IncidentWorker
    {
        private Faction faction;
        private static List<Map> tmpAvailableMaps = new List<Map>();

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && IncidentWorker_SpreadingOutpost.TryFindFaction(out faction) && TileFinder.TryFindNewSiteTile(out int tile, 8, 30, false, true, -1);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!IncidentWorker_SpreadingOutpost.TryFindFaction(out faction)) return false;
            this.TryGetRandomAvailableTargetMap(out Map map);
            int pirateTile = RandomNearbyHostileSettlement(map.Tile).Tile;

            if (!TileFinder.TryFindNewSiteTile(out int tile, 2, 8, false, true, pirateTile)) return false;
            Site site = SiteMaker.MakeSite(SiteCoreDefOf.Nothing, SitePartDefOf.Outpost, tile, faction);
            site.Tile = tile;
            Find.WorldObjects.Add(site);
            base.SendStandardLetter(site, faction, new string[]
            {
                    faction.leader.LabelShort,
                    faction.def.leaderTitle,
                    faction.Name,
            });
            return true;
        }

        private SettlementBase RandomNearbyHostileSettlement(int originTile)
        {
            return (from settlement in Find.WorldObjects.SettlementBases
                    where settlement.Attackable && Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < 36f 
                    && Find.WorldReachability.CanReach(originTile, settlement.Tile) && settlement.Faction == this.faction
                    select settlement).RandomElementWithFallback(null);
        }

        private static bool TryFindFaction(out Faction enemyFaction)
        {
            if ((from x in Find.FactionManager.AllFactions
                 where !x.def.hidden && !x.defeated && !x.IsPlayer && x.HostileTo(Faction.OfPlayer) && x.def.permanentEnemy
                 select x).TryRandomElement(out enemyFaction))
            {
                return true;
            };
            enemyFaction = null;
            return false;
        }

        private bool TryGetRandomAvailableTargetMap(out Map map)
        {
            IncidentWorker_SpreadingOutpost.tmpAvailableMaps.Clear();
            List<Map> maps = Find.Maps;
            foreach (Map potentialTargetMap in maps)
            {
                if (potentialTargetMap.IsPlayerHome && this.RandomNearbyHostileSettlement(potentialTargetMap.Tile) != null)
                {
                    IncidentWorker_SpreadingOutpost.tmpAvailableMaps.Add(potentialTargetMap);
                }
            }
            bool result = IncidentWorker_SpreadingOutpost.tmpAvailableMaps.TryRandomElement(out map);
            IncidentWorker_SpreadingOutpost.tmpAvailableMaps.Clear();
            return result;
        }
    }
}
