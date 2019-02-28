using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction
{
    [Obsolete]
    public class ChoiceLetter_ReverseTradeRequest : ChoiceLetter
    {
        public IncidentParms incidentParms;
        public ThingCategoryDef thingCategoryDef;
        public int fee = 100;
        public Map map;
        public Faction faction;
        public int tile;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly)
                {
                    yield return Option_Close;
                }
                else
                {
                    int traveltime = CalcuteTravelTimeForTrader(originTile: tile);
                    DiaOption accept = new DiaOption(text: "RansomDemand_Accept".Translate())
                    {
                        action = () =>
                        {
                            //spawn a trader with a stock gen that accepts our goods, has decent-ish money and nothing else.
                            //first attempt had a newly created trader for each, but the game can't save that. Had to define in XML.
                            incidentParms.faction = faction;
                            TraderKindDef traderKind = DefDatabase<TraderKindDef>.GetNamed(defName: "MFI_EmptyTrader_" + thingCategoryDef);

                            traderKind.stockGenerators.First(predicate: x => x.HandlesThingDef(thingDef: ThingDefOf.Silver)).countRange.max += fee;
                            traderKind.stockGenerators.First(predicate: x => x.HandlesThingDef(thingDef: ThingDefOf.Silver)).countRange.min += fee;

                            traderKind.label = thingCategoryDef.label + " " + "MFI_Trader".Translate();
                            incidentParms.traderKind = traderKind;
                            incidentParms.forced = true;
                            incidentParms.target = map;

                            Find.Storyteller.incidentQueue.Add(def: IncidentDefOf.TraderCaravanArrival, fireTick: Find.TickManager.TicksGame + traveltime, parms: incidentParms);
                            TradeUtility.LaunchSilver(map: map, fee: fee);
                        },
                    };
                    DiaNode acceptLink = new DiaNode(text: "MFI_TraderSent".Translate(
                        faction.leader?.LabelShort,
                        traveltime.ToStringTicksToPeriodVague(vagueMin: false)
                    ).CapitalizeFirst());
                    acceptLink.options.Add(item: Option_Close);
                    accept.link = acceptLink;

                    if (!TradeUtility.ColonyHasEnoughSilver(map: map, fee: fee))
                    {
                        accept.Disable(newDisabledReason: "NeedSilverLaunchable".Translate(fee.ToString()));
                    }
                    yield return accept;

                    DiaOption reject = new DiaOption(text: "RansomDemand_Reject".Translate())
                    {
                        action = () =>
                        {
                            Find.LetterStack.RemoveLetter(@let: this);
                        },
                        resolveTree = true
                    };
                    yield return reject;
                    yield return Option_Postpone;
                }
            }
        }

        private int CalcuteTravelTimeForTrader(int originTile)
        {
            int travelTime = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(@from: originTile, to: map.Tile, caravan: null);
            return Math.Min(val1: travelTime, val2: GenDate.TicksPerDay * 4);
        }

        public override bool CanShowInLetterStack => base.CanShowInLetterStack && Find.Maps.Contains(item: map) && !faction.HostileTo(other: Faction.OfPlayer);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(value: ref thingCategoryDef, label: "MFI_thingCategoryDef");
            Scribe_Deep.Look(target: ref incidentParms, label: "MFI_incidentParms", ctorArgs: new object[0]);
            Scribe_References.Look(refee: ref map, label: "MFI_map");
            Scribe_References.Look(refee: ref faction, label: "MFI_faction");
            Scribe_Values.Look(value: ref fee, label: "MFI_fee");
            Scribe_Values.Look(value: ref tile, label: "MFI_tile");
        }
    }
}
