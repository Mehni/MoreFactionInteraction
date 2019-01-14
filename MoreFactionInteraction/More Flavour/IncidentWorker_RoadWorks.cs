using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;
using UnityEngine;

namespace MoreFactionInteraction
{
    class IncidentWorker_RoadWorks : IncidentWorker
    {
        Map map;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && TryGetRandomAvailableTargetMap(out Map map)
                                             && RandomNearbyTradeableSettlement(map.Tile) != null
                                             && CommsConsoleUtility.PlayerHasPoweredCommsConsole();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!TryGetRandomAvailableTargetMap(out map))
                return false;

            SettlementBase settlementBase = RandomNearbyTradeableSettlement(map.Tile);

            if (settlementBase == null)
                return false;

            int destination = Rand.Chance(0.8f) ? map.Tile : AllyOfNearbySettlement(settlementBase)?.Tile ?? map.Tile;

            WorldPath path = WorldPath.NotFound;
            StringBuilder stringBuilder = new StringBuilder();
            float cost = 0f;
            float cost2;
            using (path = Find.WorldPathFinder.FindPath(destination, settlementBase.Tile, null))
            {
                if (path != null && path != WorldPath.NotFound)
                {
                    stringBuilder.Append($"Path found from {settlementBase.Label} to {map.info.parent.Label}. Length = {path.NodesReversed.Count} ");
                    //not 0 and - 1
                    for (int i = 1; i < path.NodesReversed.Count - 1; i++)
                    {
                        cost2 = Caravan_PathFollower.CostToMove(CaravanTicksPerMoveUtility.DefaultTicksPerMove, path.NodesReversed[i], path.NodesReversed[i + 1]);
                        cost = +WorldPathGrid.CalculatedMovementDifficultyAt(path.NodesReversed[i], true);

                        WorldObject_RoadConstruction roadConstruction = (WorldObject_RoadConstruction)WorldObjectMaker.MakeWorldObject(MFI_DefOf.MFI_RoadUnderConstruction);

                        roadConstruction.Tile = path.NodesReversed[i];
                        roadConstruction.nextTile = path.NodesReversed[i + 1];
                        roadConstruction.road = RoadDefOf.AncientAsphaltHighway;
                        roadConstruction.projectedTimeOfCompletion = Find.TickManager.TicksGame + i * GenDate.TicksPerHour; //(i + 2) * GenDate.TicksPerDay;
                        Find.WorldObjects.Add(roadConstruction);

                        //Find.WorldGrid.OverlayRoad(path.NodesReversed[i], path.NodesReversed[i + 1], RoadDefOf.AncientAsphaltRoad);

                        stringBuilder.Append($"cost1: {cost}, cost2: {cost2}, ");
                    }
                    Log.Message(stringBuilder.ToString());
                    Find.World.renderer.RegenerateAllLayersNow();
                }
            }
            return true;
        }

        public SettlementBase RandomNearbyTradeableSettlement(int originTile)
        {
            return (from settlement in Find.WorldObjects.SettlementBases
                    where settlement.Visitable && settlement.Faction.leader != null
                            && settlement.trader.CanTradeNow
                            && Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < 36f
                            && Find.WorldReachability.CanReach(originTile, settlement.Tile)
                    select settlement).RandomElementWithFallback(null);
        }

        public SettlementBase AllyOfNearbySettlement(SettlementBase origin)
        {
            return (from settlement in Find.WorldObjects.SettlementBases
                    where settlement.Faction.GoodwillWith(origin.Faction) >= 0
                            && settlement.trader.CanTradeNow
                            && Find.WorldGrid.ApproxDistanceInTiles(origin.Tile, settlement.Tile) < 20f
                            && Find.WorldReachability.CanReach(origin.Tile, settlement.Tile)
                    select settlement).RandomElementByWeight(x => x.Faction.GoodwillWith(origin.Faction));
        }

        private bool TryGetRandomAvailableTargetMap(out Map map) => Find.Maps.Where(m => m.IsPlayerHome).TryRandomElement(out map);
    }
}
