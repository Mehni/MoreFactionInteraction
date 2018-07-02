using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction.World_Incidents
{
    public class IncidentWorker_BumperCrop : IncidentWorker
    {
        private static readonly IntRange OfferDurationRange = new IntRange(10, 30);
        private static List<Map> tmpAvailableMaps = new List<Map>();

        public override float AdjustedChance
        {
            get
            {
                return base.AdjustedChance +
                (float)(Find.FactionManager.AllFactionsVisible
                    .Where(faction => !faction.defeated && !faction.IsPlayer && !faction.HostileTo(Faction.OfPlayer))
                        .Average(faction => faction.GoodwillWith(Faction.OfPlayer)) / 100);
            }
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && this.TryGetRandomAvailableTargetMap(out Map map) && IncidentWorker_BumperCrop.RandomNearbyGrowerSettlement(map.Tile) != null
                && VirtualPlantsUtility.EnvironmentAllowsEatingVirtualPlantsNowAt(IncidentWorker_BumperCrop.RandomNearbyGrowerSettlement(map.Tile).Tile);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            this.TryGetRandomAvailableTargetMap(out Map map);
            Settlement settlement = IncidentWorker_BumperCrop.RandomNearbyGrowerSettlement(map.Tile);
            if (settlement == null)
            {
                return false;
            }
            WorldObjectComp_SettlementBumperCropComp component = settlement.GetComponent<WorldObjectComp_SettlementBumperCropComp>();

            if (!this.TryGenerateBumperCrop(component, map))
            {
                return false;
            }
            Find.LetterStack.ReceiveLetter("MFI_LetterLabel_HarvestRequest".Translate(), "MFI_LetterHarvestRequest".Translate(new object[]
            {
                settlement.Label,
                (component.expiration - Find.TickManager.TicksGame).ToStringTicksToDays("F0")
            }), LetterDefOf.PositiveEvent, settlement, null);
            return true;
        }

        private bool TryGenerateBumperCrop(WorldObjectComp_SettlementBumperCropComp target, Map map)
        {
            int num = this.RandomOfferDuration(map.Tile, target.parent.Tile);
            if (num < 1)
            {
                return false;
            }
            target.bumperCrop = RandomRawFood() ?? ThingDefOf.RawPotatoes;
            target.expiration = Find.TickManager.TicksGame + num;
            return true;
        }

        private ThingDef RandomRawFood()
        {
            //a long list of things to excluse stuff like milk and kibble. In retrospect, it may have been easier to get all plants and get their harvestables.
            if (!(from x in ThingSetMakerUtility.allGeneratableItems
                  where x.IsNutritionGivingIngestible && !x.IsCorpse && x.ingestible.HumanEdible && !x.IsMeat 
                    && !x.IsDrug && !x.HasComp(typeof(CompHatcher)) && !x.HasComp(typeof(CompIngredients)) 
                    && x.BaseMarketValue <3 && (x.ingestible.preferability == FoodPreferability.RawBad || x.ingestible.preferability == FoodPreferability.RawTasty)
                  select x).TryRandomElement(out ThingDef thingDef))
            {
                return null;
            }
            return thingDef;
        }

        public static Settlement RandomNearbyGrowerSettlement(int originTile)
        {
            return (from settlement in Find.WorldObjects.Settlements
                    where settlement.Visitable && settlement.GetComponent<TradeRequestComp>() != null && !settlement.GetComponent<TradeRequestComp>().ActiveRequest 
                    && !settlement.GetComponent<WorldObjectComp_SettlementBumperCropComp>().ActiveRequest && Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < 36f 
                    && Find.WorldReachability.CanReach(originTile, settlement.Tile)
                    select settlement).RandomElementWithFallback(null);
        }

        private int RandomOfferDuration(int tileIdFrom, int tileIdTo)
        {
            int offerValidForDays = IncidentWorker_BumperCrop.OfferDurationRange.RandomInRange;
            int travelTimeByCaravan = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(tileIdFrom, tileIdTo, null);
            float daysWorthOfTravel = (float)travelTimeByCaravan / GenDate.TicksPerDay;
            int b = Mathf.CeilToInt(Mathf.Max(daysWorthOfTravel + 1f, daysWorthOfTravel * 1.1f));
            offerValidForDays = Mathf.Max(offerValidForDays, b);
            if (offerValidForDays > IncidentWorker_BumperCrop.OfferDurationRange.max)
            {
                return -1;
            }
            return GenDate.TicksPerDay * offerValidForDays;
        }

        private bool TryGetRandomAvailableTargetMap(out Map map)
        {
            IncidentWorker_BumperCrop.tmpAvailableMaps.Clear();
            List<Map> maps = Find.Maps;
            for (int i = 0; i < maps.Count; i++)
            {
                if (maps[i].IsPlayerHome && this.AtLeast2HealthyColonists(maps[i]) && IncidentWorker_BumperCrop.RandomNearbyGrowerSettlement(maps[i].Tile) != null)
                {
                    IncidentWorker_BumperCrop.tmpAvailableMaps.Add(maps[i]);
                }
            }
            bool result = IncidentWorker_BumperCrop.tmpAvailableMaps.TryRandomElement(out map);
            IncidentWorker_BumperCrop.tmpAvailableMaps.Clear();
            return result;
        }
        
        private bool AtLeast2HealthyColonists(Map map)
        {
            List<Pawn> pawnList = map.mapPawns.SpawnedPawnsInFaction(Faction.OfPlayer);
            int healthyColonists = 0;

            for (int i = 0; i < pawnList.Count; i++)
            {
                if (pawnList[i].IsFreeColonist)
                {
                    if (!HealthAIUtility.ShouldSeekMedicalRest(pawnList[i]))
                    {
                        healthyColonists++;
                        if (healthyColonists >= 2)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
