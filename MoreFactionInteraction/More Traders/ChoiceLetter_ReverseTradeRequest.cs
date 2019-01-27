using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction
{
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
                if (this.ArchivedOnly)
                {
                    yield return this.Option_Close;
                }
                else
                {
                    int traveltime = this.CalcuteTravelTimeForTrader(originTile: this.tile);
                    DiaOption accept = new DiaOption(text: "RansomDemand_Accept".Translate())
                    {
                        action = () =>
                        {
                            //spawn a trader with a stock gen that accepts our goods, has decent-ish money and nothing else.
                            //first attempt had a newly created trader for each, but the game can't save that. Had to define in XML.
                            this.incidentParms.faction = this.faction;
                            TraderKindDef traderKind = DefDatabase<TraderKindDef>.GetNamed(defName: "MFI_EmptyTrader_" + this.thingCategoryDef);

                            traderKind.stockGenerators.First(predicate: x => x.HandlesThingDef(thingDef: ThingDefOf.Silver)).countRange.max += this.fee;
                            traderKind.stockGenerators.First(predicate: x => x.HandlesThingDef(thingDef: ThingDefOf.Silver)).countRange.min += this.fee;

                            traderKind.label = this.thingCategoryDef.label + " " + "MFI_Trader".Translate();
                            this.incidentParms.traderKind = traderKind;
                            this.incidentParms.forced = true;

                            Find.Storyteller.incidentQueue.Add(def: IncidentDefOf.TraderCaravanArrival, fireTick: Find.TickManager.TicksGame + traveltime, parms: this.incidentParms);
                            TradeUtility.LaunchSilver(map: this.map, fee: this.fee);
                        },
                    };
                    DiaNode acceptLink = new DiaNode(text: "MFI_TraderSent".Translate(
                        this.faction.leader?.LabelShort,
                        traveltime.ToStringTicksToPeriodVague(vagueMin: false)
                    ).CapitalizeFirst());
                    acceptLink.options.Add(item: this.Option_Close);
                    accept.link = acceptLink;

                    if (!TradeUtility.ColonyHasEnoughSilver(map: this.map, fee: this.fee))
                    {
                        accept.Disable(newDisabledReason: "NeedSilverLaunchable".Translate(this.fee.ToString()));
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
                    yield return this.Option_Postpone;
                }
            }
        }

        private int CalcuteTravelTimeForTrader(int originTile)
        {
            int travelTime = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(@from: originTile, to: this.map.Tile, caravan: null);
            return Math.Min(val1: travelTime, val2: GenDate.TicksPerDay * 4);
        }

        public override bool CanShowInLetterStack => base.CanShowInLetterStack && Find.Maps.Contains(item: this.map) && !this.faction.HostileTo(other: Faction.OfPlayer);

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<ThingCategoryDef>(value: ref this.thingCategoryDef, label: "MFI_thingCategoryDef");
            Scribe_Deep.Look<IncidentParms>(target: ref this.incidentParms, label: "MFI_incidentParms", ctorArgs: new object[0]);
            Scribe_References.Look<Map>(refee: ref this.map, label: "MFI_map");
            Scribe_References.Look<Faction>(refee: ref this.faction, label: "MFI_faction");
            Scribe_Values.Look<int>(value: ref this.fee, label: "MFI_fee");
            Scribe_Values.Look<int>(value: ref this.tile, label: "MFI_tile");
        }
    }
}
