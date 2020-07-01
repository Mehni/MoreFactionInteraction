﻿using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction.World_Incidents
{
    public class IncidentWorker_BumperCrop : IncidentWorker
    {
        private static readonly IntRange OfferDurationRange = new IntRange(min: 10, max: 30);

        public override float BaseChanceThisGame => base.BaseChanceThisGame
            + (float)(Find.FactionManager.AllFactionsVisible
                    .Where(predicate: faction => !faction.defeated && !faction.IsPlayer && !faction.HostileTo(other: Faction.OfPlayer))
                        .Average(selector: faction => faction.GoodwillWith(other: Faction.OfPlayer)) / 100);

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms: parms) && TryGetRandomAvailableTargetMap(map: out Map map)
                                                    && RandomNearbyGrowerSettlement(originTile: map.Tile) != null
                                                    && VirtualPlantsUtility.EnvironmentAllowsEatingVirtualPlantsNowAt(tile: RandomNearbyGrowerSettlement(originTile: map.Tile).Tile);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            TryGetRandomAvailableTargetMap(map: out Map map);
            if (map == null)
                return false;

            Settlement settlement = RandomNearbyGrowerSettlement(originTile: map.Tile);

            if (settlement == null)
                return false;

            WorldObjectComp_SettlementBumperCropComp component = settlement.GetComponent<WorldObjectComp_SettlementBumperCropComp>();

            if (!TryGenerateBumperCrop(target: component, map: map))
                return false;

            Find.LetterStack.ReceiveLetter(label: "MFI_LetterLabel_HarvestRequest".Translate(), text: "MFI_LetterHarvestRequest".Translate(
                settlement.Label,
                (component.expiration - Find.TickManager.TicksGame).ToStringTicksToDays(format: "F0")
            ), textLetterDef: LetterDefOf.PositiveEvent, lookTargets: settlement, relatedFaction: settlement.Faction);
            return true;
        }

        private static bool TryGenerateBumperCrop(WorldObjectComp_SettlementBumperCropComp target, Map map)
        {
            int num = RandomOfferDuration(tileIdFrom: map.Tile, tileIdTo: target.parent.Tile);
            if (num < 1)
            {
                return false;
            }
            target.expiration = Find.TickManager.TicksGame + num;
            return true;
        }

        public static Settlement RandomNearbyGrowerSettlement(int originTile)
            => Find.WorldObjects.Settlements
                .Where(settlement => settlement != null
                        && settlement.Visitable
                        && settlement.GetComponent<TradeRequestComp>() != null
                        && !settlement.GetComponent<TradeRequestComp>().ActiveRequest
                        && settlement.GetComponent<WorldObjectComp_SettlementBumperCropComp>() != null
                        && !settlement.GetComponent<WorldObjectComp_SettlementBumperCropComp>().ActiveRequest
                        && Find.WorldGrid.ApproxDistanceInTiles(firstTile: originTile, secondTile: settlement.Tile) < 36f
                        && Find.WorldReachability.CanReach(startTile: originTile, destTile: settlement.Tile))
                .RandomElementWithFallback();

        private static int RandomOfferDuration(int tileIdFrom, int tileIdTo)
        {
            int offerValidForDays = OfferDurationRange.RandomInRange;
            int travelTimeByCaravan = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(@from: tileIdFrom, to: tileIdTo, caravan: null);
            float daysWorthOfTravel = (float)travelTimeByCaravan / GenDate.TicksPerDay;
            int b = Mathf.CeilToInt(f: Mathf.Max(a: daysWorthOfTravel + 1f, b: daysWorthOfTravel * 1.1f));
            offerValidForDays = Mathf.Max(a: offerValidForDays, b: b);
            if (offerValidForDays > OfferDurationRange.max)
            {
                return -1;
            }
            return GenDate.TicksPerDay * offerValidForDays;
        }

        private static bool TryGetRandomAvailableTargetMap(out Map map)
            => Find.Maps
                .Where(maps => maps.IsPlayerHome && AtLeast2HealthyColonists(maps) && RandomNearbyGrowerSettlement(maps.Tile) != null)
                .TryRandomElement(out map);

        private static bool AtLeast2HealthyColonists(Map map)
        {
            List<Pawn> pawnList = map.mapPawns.SpawnedPawnsInFaction(faction: Faction.OfPlayer);
            int healthyColonists = 0;

            foreach (Pawn pawn in pawnList)
            {
                if (pawn.IsFreeColonist && !HealthAIUtility.ShouldSeekMedicalRest(pawn: pawn))
                {
                    healthyColonists++;
                    if (healthyColonists >= 2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
