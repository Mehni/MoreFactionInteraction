using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using Verse;
using System.Linq;

namespace MoreFactionInteraction
{
    public class IncidentWorker_ReverseTradeRequest : IncidentWorker
    {
        private const int TimeoutTicks = GenDate.TicksPerDay;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms: parms) && TryGetRandomAvailableTargetMap(map: out Map map)
                                                     && RandomNearbyTradeableSettlement(originTile: map.Tile) != null
                                                     && CommsConsoleUtility.PlayerHasPoweredCommsConsole(map);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!TryGetRandomAvailableTargetMap(map: out Map map))
                return false;

            SettlementBase settlement = RandomNearbyTradeableSettlement(originTile: map.Tile);

            if (settlement == null)
                return false;

            //TODO: look into making the below dynamic based on requester's biome, faction, pirate outpost vicinity and other stuff.
            ThingCategoryDef thingCategoryDef = DetermineThingCategoryDef();

            string letterToSend = DetermineLetterToSend(thingCategoryDef: thingCategoryDef);
            int feeRequest = Math.Max(val1: Rand.Range(min: 150, max: 300), val2: (int)parms.points);
            string categorylabel = (thingCategoryDef == ThingCategoryDefOf.PlantFoodRaw) ? thingCategoryDef.label + " items" : thingCategoryDef.label;
            DiaNode diaNode = new DiaNode(letterToSend.Translate(
                                                                 settlement.Faction.leader.LabelShort,
                                                                 settlement.Faction.def.leaderTitle,
                                                                 settlement.Faction.Name,
                                                                 settlement.Label,
                                                                 categorylabel,
                                                                 feeRequest
                                                                ).AdjustedFor(p: settlement.Faction.leader));

            int traveltime = this.CalcuteTravelTimeForTrader(originTile: settlement.Tile, map);
            DiaOption accept = new DiaOption(text: "RansomDemand_Accept".Translate())
            {
                action = () =>
                {
                    //spawn a trader with a stock gen that accepts our goods, has decent-ish money and nothing else.
                    //first attempt had a newly created trader for each, but the game can't save that. Had to define in XML.
                    parms.faction = settlement.Faction;
                    TraderKindDef traderKind = DefDatabase<TraderKindDef>.GetNamed(defName: "MFI_EmptyTrader_" + thingCategoryDef);

                    traderKind.stockGenerators.First(predicate: x => x.HandlesThingDef(thingDef: ThingDefOf.Silver)).countRange.max += feeRequest;
                    traderKind.stockGenerators.First(predicate: x => x.HandlesThingDef(thingDef: ThingDefOf.Silver)).countRange.min += feeRequest;

                    traderKind.label = thingCategoryDef.label + " " + "MFI_Trader".Translate();
                    parms.traderKind = traderKind;
                    parms.forced = true;
                    parms.target = map;

                    Find.Storyteller.incidentQueue.Add(def: IncidentDefOf.TraderCaravanArrival, fireTick: Find.TickManager.TicksGame + traveltime, parms: parms);
                    TradeUtility.LaunchSilver(map: map, fee: feeRequest);
                },
            };
            DiaNode acceptLink = new DiaNode(text: "MFI_TraderSent".Translate(
                settlement.Faction.leader?.LabelShort,
                traveltime.ToStringTicksToPeriodVague(vagueMin: false)
            ).CapitalizeFirst());
            acceptLink.options.Add(DiaOption.DefaultOK);
            accept.link = acceptLink;

            if (!TradeUtility.ColonyHasEnoughSilver(map: map, fee: feeRequest))
            {
                accept.Disable(newDisabledReason: "NeedSilverLaunchable".Translate(feeRequest.ToString()));
            }

            DiaOption reject = new DiaOption(text: "RansomDemand_Reject".Translate())
            {
                action = () =>
                {
                },
                resolveTree = true
            };

            diaNode.options = new List<DiaOption> { accept, reject };

            Find.WindowStack.Add(new Dialog_NodeTreeWithFactionInfo(diaNode, settlement.Faction, title: "MFI_ReverseTradeRequestTitle".Translate(map.info.parent.Label).CapitalizeFirst()));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, "MFI_ReverseTradeRequestTitle".Translate(map.info.parent.Label).CapitalizeFirst(), settlement.Faction));

            return true;
        }

        private static ThingCategoryDef DetermineThingCategoryDef()
        {
            ThingCategoryDef thingCategoryDef;

            int rand = Rand.RangeInclusive(min: 0, max: 100);
            if (rand < 33) thingCategoryDef = ThingCategoryDefOf.Apparel;
            else if (rand > 33 && rand < 66) thingCategoryDef = ThingCategoryDefOf.PlantFoodRaw;
            else if (rand > 66 && rand < 90) thingCategoryDef = ThingCategoryDefOf.Weapons;
            else thingCategoryDef = ThingCategoryDefOf.Medicine;
            return thingCategoryDef;
        }

        private static string DetermineLetterToSend(ThingCategoryDef thingCategoryDef)
        {

            if (thingCategoryDef == ThingCategoryDefOf.PlantFoodRaw)
                return "MFI_ReverseTradeRequest_Blight";

            switch (Rand.RangeInclusive(min: 0, max: 4))
            {
                case 0:
                    return "MFI_ReverseTradeRequest_Pyro";
                case 1:
                    return "MFI_ReverseTradeRequest_Mechs";
                case 2:
                    return "MFI_ReverseTradeRequest_Caravan";
                case 3:
                    return "MFI_ReverseTradeRequest_Pirates";
                case 4:
                    return "MFI_ReverseTradeRequest_Hardship";

                default:
                    return "MFI_ReverseTradeRequest_Pyro";
            }
        }

        public static SettlementBase RandomNearbyTradeableSettlement(int originTile)
        {
            return (from settlement in Find.WorldObjects.SettlementBases
                    where settlement.Visitable && settlement.Faction.leader != null
                            && settlement.GetComponent<TradeRequestComp>() != null
                            && !settlement.GetComponent<TradeRequestComp>().ActiveRequest
                            && Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < 36f
                            && Find.WorldReachability.CanReach(originTile, settlement.Tile)
                    select settlement).RandomElementWithFallback(null);
        }

        private static bool TryGetRandomAvailableTargetMap(out Map map)
        {
            return Find.Maps.Where(x => x.IsPlayerHome && RandomNearbyTradeableSettlement(x.Tile) != null)
                .TryRandomElement(out map);
        }

        private int CalcuteTravelTimeForTrader(int originTile, Map map)
        {
            int travelTime = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(@from: originTile, to: map.Tile, caravan: null);
            return Math.Min(val1: travelTime, val2: GenDate.TicksPerDay * 4);
        }
    }
}