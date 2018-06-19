using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                if (base.ArchivedOnly)
                {
                    yield return base.OK;
                }
                else
                {
                    int traveltime = CalcuteTravelTimeForTrader(this.tile);
                    DiaOption accept = new DiaOption("RansomDemand_Accept".Translate())
                    {
                        action = () =>
                        {
                        //spawn a trader with a stock gen that accepts our goods, has decent-ish money and nothing else.
                        //first attempt had a newly created trader for each, but the game can't save that. Had to define in XML.
                        incidentParms.faction = this.faction;
                            TraderKindDef traderKind = DefDatabase<TraderKindDef>.GetNamed("MFI_EmptyTrader_" + this.thingCategoryDef);

                            traderKind.stockGenerators.Where(x => x.HandlesThingDef(ThingDefOf.Silver)).First().countRange.max += fee;
                            traderKind.stockGenerators.Where(x => x.HandlesThingDef(ThingDefOf.Silver)).First().countRange.min += fee;

                            traderKind.label = this.thingCategoryDef.label + " " + "MFI_Trader".Translate();
                            incidentParms.traderKind = traderKind;
                            incidentParms.forced = true;

                            Find.Storyteller.incidentQueue.Add(IncidentDefOf.TraderCaravanArrival, Find.TickManager.TicksGame + traveltime, incidentParms);
                            TradeUtility.LaunchSilver(this.map, this.fee);
                        },
                    };
                    DiaNode diaNode = new DiaNode("MFI_TraderSent".Translate(new object[]
                    {
                    faction.leader.LabelShort,
                    traveltime.ToStringTicksToPeriodVague(false, true)
                    }).CapitalizeFirst());
                    diaNode.options.Add(base.OK);
                    accept.link = diaNode;

                    if (!TradeUtility.ColonyHasEnoughSilver(this.map, this.fee))
                    {
                        accept.Disable("NeedSilverLaunchable".Translate(new object[]
                        {
                    this.fee.ToString()
                        }));
                    }
                    yield return accept;

                    DiaOption reject = new DiaOption("RansomDemand_Reject".Translate())
                    {
                        action = () =>
                        {
                            Find.LetterStack.RemoveLetter(this);
                        },
                        resolveTree = true
                    };
                    yield return reject;
                    yield return base.Postpone;
                }
            }
        }

        private int CalcuteTravelTimeForTrader(int originTile)
        {
            int travelTime = CaravanArrivalTimeEstimator.EstimatedTicksToArrive(originTile, this.map.Tile, null);
            return Math.Min(travelTime, GenDate.TicksPerDay * 4);
        }

        public override bool CanShowInLetterStack
        {
            get
            {
                return base.CanShowInLetterStack && Find.Maps.Contains(this.map) && !this.faction.HostileTo(Faction.OfPlayer);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<ThingCategoryDef>(ref this.thingCategoryDef, "MFI_thingCategoryDef");
            Scribe_Deep.Look<IncidentParms>(ref this.incidentParms, "MFI_incidentParms", new object[0]);
            Scribe_References.Look<Map>(ref this.map, "MFI_map", false);
            Scribe_References.Look<Faction>(ref this.faction, "MFI_faction", false);
            Scribe_Values.Look<int>(ref this.fee, "MFI_fee");
            Scribe_Values.Look<int>(ref this.tile, "MFI_tile");
        }
    }
}

