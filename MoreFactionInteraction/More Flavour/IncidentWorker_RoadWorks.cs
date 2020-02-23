﻿using System;
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
        private const float maxRoadCoverage = 0.8f;
        private const float directConnectionChance = 0.7f;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && TryGetRandomAvailableTargetMap(out Map map)
                                             && CommsConsoleUtility.PlayerHasPoweredCommsConsole()
                                             && RandomNearbyTradeableSettlement(map.Tile) != null;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!TryGetRandomAvailableTargetMap(out map))
                return false;

            Settlement Settlement = RandomNearbyTradeableSettlement(map.Tile);

            if (Settlement?.Faction == null)
                return false;

            int destination = Rand.Chance(directConnectionChance) ? map.Tile : AllyOfNearbySettlement(Settlement)?.Tile ?? map.Tile;

            int maxPriority = Settlement.Faction.def.techLevel >= TechLevel.Medieval ? 30 : 20;

            RoadDef roadToBuild = DefDatabase<RoadDef>.AllDefsListForReading.Where(x => x.priority <= maxPriority).RandomElement();

            WorldPath path = WorldPath.NotFound;

            int cost2 = 12000;
            int timeToBuild = 0;
            string letterTitle = "MFI_RoadWorks".Translate();
            List<WorldObject_RoadConstruction> list = new List<WorldObject_RoadConstruction>();
            using (path = Find.WorldPathFinder.FindPath(destination, Settlement.Tile, null))
            {
                if (path == null || path == WorldPath.NotFound)
                    return true;

                float roadCount = path.NodesReversed.Count(x => !Find.WorldGrid[x].Roads.NullOrEmpty()
                                                             && Find.WorldGrid[x].Roads.Any(roadLink => roadLink.road.priority >= roadToBuild.priority)
                                                             || Find.WorldObjects.AnyWorldObjectOfDefAt(MFI_DefOf.MFI_RoadUnderConstruction, x));

                if (roadCount / path.NodesReversed.Count >= maxRoadCoverage)
                    return false;

                //not 0 and - 1
                for (int i = 1; i < path.NodesReversed.Count - 1; i++)
                {
                    cost2 += Caravan_PathFollower.CostToMove(CaravanTicksPerMoveUtility.DefaultTicksPerMove, path.NodesReversed[i], path.NodesReversed[i + 1]);

                    timeToBuild += (int)(2 * GenDate.TicksPerDay
                                           * WorldPathGrid.CalculatedMovementDifficultyAt(path.NodesReversed[i], true)
                                           * Find.WorldGrid.GetRoadMovementDifficultyMultiplier(i, i + 1));

                    if (!Find.WorldGrid[path.NodesReversed[i]].Roads.NullOrEmpty()
                        && Find.WorldGrid[path.NodesReversed[i]].Roads.Any(roadLink => roadLink.road.priority >= roadToBuild.priority))
                    {
                        timeToBuild = timeToBuild / 2;
                    }

                    WorldObject_RoadConstruction roadConstruction = (WorldObject_RoadConstruction)WorldObjectMaker.MakeWorldObject(MFI_DefOf.MFI_RoadUnderConstruction);
                    roadConstruction.Tile = path.NodesReversed[i];
                    roadConstruction.nextTile = path.NodesReversed[i + 1];
                    roadConstruction.road = roadToBuild;
                    roadConstruction.SetFaction(Settlement.Faction);
                    roadConstruction.projectedTimeOfCompletion = Find.TickManager.TicksGame + timeToBuild;
                    list.Add(roadConstruction);
                }
                cost2 = cost2 / 10;
                DiaNode node = new DiaNode("MFI_RoadWorksDialogue".Translate(Settlement, path.NodesReversed.Count, cost2)); // {Settlement} wants {cost2 / 10} to build a road of {path.NodesReversed.Count}");
                DiaOption accept = new DiaOption("OK".Translate())
                {
                    resolveTree = true,
                    action = () =>
                             {
                                 TradeUtility.LaunchSilver(TradeUtility.PlayerHomeMapWithMostLaunchableSilver(), cost2);
                                 foreach (WorldObject_RoadConstruction worldObjectRoadConstruction in list)
                                 {
                                     Find.WorldObjects.Add(worldObjectRoadConstruction);
                                 }
                                 list.Clear();
                             }
                };

                if (!TradeUtility.ColonyHasEnoughSilver(TradeUtility.PlayerHomeMapWithMostLaunchableSilver(), cost2))
                {
                    accept.Disable("NeedSilverLaunchable".Translate(cost2));
                }
                DiaOption reject = new DiaOption("RejectLetter".Translate())
                {
                    resolveTree = true,
                    action = () =>
                             {
                                 for (int i = list.Count - 1; i >= 0; i--)
                                 {
                                     list[i] = null;
                                 }
                                 list.Clear();
                             }
                };

                node.options.Add(accept);
                node.options.Add(reject);

                //Log.Message(stringBuilder.ToString());
                Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(node, Settlement.Faction));
                Find.Archive.Add(new ArchivedDialog(node.text, letterTitle, Settlement.Faction));
            }
            return true;
        }

        public Settlement RandomNearbyTradeableSettlement(int originTile)
            => (from settlement in Find.WorldObjects.Settlements
                where settlement.Visitable && settlement.Faction?.leader != null
                                           && settlement.trader.CanTradeNow
                                           && Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < 36f
                                           && Find.WorldReachability.CanReach(originTile, settlement.Tile)
                select settlement).RandomElementWithFallback(null);

        public Settlement AllyOfNearbySettlement(Settlement origin)
            => (from settlement in Find.WorldObjects.Settlements
                where settlement.Tile != origin.Tile
                   && (settlement.Faction == origin.Faction || settlement.Faction?.GoodwillWith(origin.Faction) >= 0)
                   && settlement.trader.CanTradeNow
                   && Find.WorldGrid.ApproxDistanceInTiles(origin.Tile, settlement.Tile) < 20f
                   && Find.WorldReachability.CanReach(origin.Tile, settlement.Tile)
                select settlement).RandomElement();

        private bool TryGetRandomAvailableTargetMap(out Map map) => Find.Maps.Where(m => m.IsPlayerHome).TryRandomElement(out map);
    }
}
