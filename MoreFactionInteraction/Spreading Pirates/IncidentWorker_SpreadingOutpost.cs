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
        private readonly int minDist = 8;
        private readonly int maxDist = 30;
        private readonly int maxSites = 20;

        public override float AdjustedChance => base.AdjustedChance * MoreFactionInteraction_Settings.pirateBaseUpgraderModifier;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms: parms) && TryFindFaction(enemyFaction: out faction)
                                                    && TileFinder.TryFindNewSiteTile(tile: out int tile, minDist, maxDist)
                                                    && TryGetRandomAvailableTargetMap(out Map map)
                                                    && Find.World.worldObjects.Sites.Count() <= maxSites;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!TryFindFaction(enemyFaction: out faction))
                return false;
            if (!TryGetRandomAvailableTargetMap(map: out Map map))
                return false;
            if (faction.leader == null)
                return false;

            int pirateTile = RandomNearbyHostileSettlement(map.Tile)?.Tile ?? Tile.Invalid;

            if (pirateTile == Tile.Invalid)
                return false;

            if (!TileFinder.TryFindNewSiteTile(out int tile, minDist: 2, maxDist: 8, allowCaravans: false, preferCloserTiles: true, nearThisTile: pirateTile))
                return false;

            Site site = SiteMaker.MakeSite(core: SiteCoreDefOf.Nothing, sitePart: SitePartDefOf.Outpost, tile: tile, faction: faction);
            site.Tile = tile;
            site.sitePartsKnown = true;
            Find.WorldObjects.Add(o: site);
            SendStandardLetter(lookTargets: site, relatedFaction: faction, textArgs: new string[]
            {
                (faction.leader?.LabelShort ?? "MFI_Representative".Translate()), faction.def.leaderTitle, faction.Name,
            });
            return true;
        }

        private SettlementBase RandomNearbyHostileSettlement(int originTile)
            => Find.WorldObjects.SettlementBases
                .Where(settlement => settlement.Attackable
                        && Find.WorldGrid.ApproxDistanceInTiles(firstTile: originTile, secondTile: settlement.Tile) < 36f
                        && Find.WorldReachability.CanReach(startTile: originTile, destTile: settlement.Tile)
                        && settlement.Faction == faction)
                .RandomElementWithFallback();

        private static bool TryFindFaction(out Faction enemyFaction)
            => Find.FactionManager.AllFactions
                    .Where(x => !x.def.hidden && !x.defeated && x.HostileTo(other: Faction.OfPlayer) && x.def.permanentEnemy)
                    .TryRandomElement(result: out enemyFaction);

        private bool TryGetRandomAvailableTargetMap(out Map map)
            => Find.Maps
                .Where(x => x.IsPlayerHome && RandomNearbyHostileSettlement(x.Tile) != null)
                .TryRandomElement(out map);
    }
}
