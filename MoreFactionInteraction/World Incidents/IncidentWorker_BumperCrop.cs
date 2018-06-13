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

        public override float AdjustedChance
        {
            get
            {
                return base.AdjustedChance +
                (Find.FactionManager.AllFactionsVisible
                    .Where(faction => !faction.defeated && !faction.IsPlayer && !faction.HostileTo(Faction.OfPlayer))
                        .Average(faction => faction.GoodwillWith(Faction.OfPlayer)) / 100);
            }
        }

        protected override bool CanFireNowSub(IIncidentTarget target)
        {
            Map map = (Map)target;
            return base.CanFireNowSub(target) && IncidentWorker_BumperCrop.RandomNearbyGrowerSettlement(map.Tile) !=null
                && VirtualPlantsUtility.EnvironmentAllowsEatingVirtualPlantsNowAt(IncidentWorker_BumperCrop.RandomNearbyGrowerSettlement(map.Tile).Tile);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            Settlement settlement = IncidentWorker_BumperCrop.RandomNearbyGrowerSettlement(parms.target.Tile);
            if (settlement == null)
            {
                return false;
            }
            SettlementBumperCropComponent component = settlement.GetComponent<SettlementBumperCropComponent>();

            if (!this.TryGenerateBumperCrop(component, (Map)parms.target))
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

        private bool TryGenerateBumperCrop(SettlementBumperCropComponent target, Map map)
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
            if (!(from x in ItemCollectionGeneratorUtility.allGeneratableItems
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
                    where settlement.Visitable && settlement.GetComponent<CaravanRequestComp>() != null && !settlement.GetComponent<CaravanRequestComp>().ActiveRequest 
                    && !settlement.GetComponent<SettlementBumperCropComponent>().ActiveRequest && Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < 36f 
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
    }
}
