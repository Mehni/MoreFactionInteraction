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

        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            return base.CanFireNowSub(target) && this.TryFindFaction(out faction) && TileFinder.TryFindNewSiteTile(out int tile, 8, 30, false, true, -1);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!this.TryFindFaction(out faction)) return false;

            int pirateTile = RandomNearbyHostileSettlement(parms.target.Tile).Tile;

            if (!TileFinder.TryFindNewSiteTile(out int tile, 5, 8, false, true, pirateTile)) return false;
            Site site = SiteMaker.MakeSite(SiteCoreDefOf.Nothing, SitePartDefOf.Outpost, faction);
            site.Tile = tile;
            Find.WorldObjects.Add(site);
            base.SendStandardLetter(site, new string[]
            {
                    faction.leader.LabelShort,
                    faction.def.leaderTitle,
                    faction.Name,
            });
            return true;
        }

        private static Settlement RandomNearbyHostileSettlement(int originTile)
        {
            return (from settlement in Find.WorldObjects.Settlements
                    where settlement.Attackable && Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < 36f && Find.WorldReachability.CanReach(originTile, settlement.Tile)
                    select settlement).RandomElementWithFallback(null);
        }

        private bool TryFindFaction(out Faction enemyFaction)
        {
            if ((from x in Find.FactionManager.AllFactions
                 where !x.def.hidden && !x.defeated && !x.IsPlayer && x.HostileTo(Faction.OfPlayer) && !x.def.appreciative
                 select x).TryRandomElement(out enemyFaction))
            {
                return true;
            };
            enemyFaction = null;
            return false;
        }
    }
}
