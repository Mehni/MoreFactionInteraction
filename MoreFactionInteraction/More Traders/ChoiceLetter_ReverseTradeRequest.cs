using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
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

        protected override IEnumerable<DiaOption> Choices
        {
            get
            {
                DiaOption accept = new DiaOption("RansomDemand_Accept".Translate())
                {
                    action = () =>
                    {
                        //create a blank trader with a stock gen that accepts our goods, has decent-ish money and nothing else.
                        incidentParms.faction = this.faction;
                        TraderKindDef traderKind = new TraderKindDef
                        {
                            stockGenerators = faction.def.caravanTraderKinds.RandomElement().stockGenerators.Where(x => x.HandlesThingDef(ThingDefOf.Silver)).ToList()
                        };

                        //TODO: Either in here or in a harmony patch: increase trader silver count based on goodwill.
                        traderKind.stockGenerators.First().countRange.max += fee;
                        traderKind.stockGenerators.First().countRange.min += fee;

                        StockGenerator_BuyCategory stockgen = new StockGenerator_BuyCategory
                        {
                            thingCategoryDef = this.thingCategoryDef
                        };
                        
                        traderKind.stockGenerators.Add(stockgen);
                        traderKind.label = stockgen.thingCategoryDef.label + " "+ "MFI_Trader".Translate();

                        incidentParms.traderKind = traderKind;

                        //TODO: set 600 to a decent estimate between settlement and colony
                        Find.Storyteller.incidentQueue.Add(IncidentDefOf.TraderCaravanArrival, Find.TickManager.TicksGame + 600, incidentParms);



                        TradeUtility.LaunchSilver(this.map, this.fee);
                        Find.LetterStack.RemoveLetter(this);
                    },
                    resolveTree = true
                };
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

        public override bool StillValid
        {
            get
            {
                return base.StillValid && Find.Maps.Contains(this.map);
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<ThingCategoryDef>(ref this.thingCategoryDef, "MFI_thingCategoryDef");
            Scribe_Deep.Look<IncidentParms>(ref this.incidentParms, "MFI_incidentParms", new object[0]);
            Scribe_References.Look<Map>(ref this.map, "MFI_map", false);
            Scribe_References.Look<Faction>(ref this.faction, "MFI_faction", false);
            Scribe_Values.Look<int>(ref this.fee, "MFI_fee", 0, false);
        }
    }
}

