//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Verse;
//using RimWorld;

//namespace MoreFactionInteraction.MoreFactionWar
//{

//    public class ChoiceLetter_SellingWarBonds : ChoiceLetter
//    {
//        public int tile;
//        public Faction faction;
//        public Map map;
//        public int fee;
//        public int steelRequest;

//        public override IEnumerable<DiaOption> Choices
//        {
//            get
//            {
//                if (this.ArchivedOnly)
//                {
//                    yield return this.Option_Close;
//                }
//                else
//                {
//                    DiaOption accept = new DiaOption(text: "RansomDemand_Accept".Translate())
//                    {
//                        action = () =>
//                        {
//                            TradeUtility.LaunchSilver(map: this.map, fee: this.fee);
//                            Find.LetterStack.RemoveLetter(let: this);
//                        },
//                        resolveTree = true
//                    };
//                    if (!TradeUtility.ColonyHasEnoughSilver(map: this.map, fee: this.fee))
//                    {
//                        accept.Disable(newDisabledReason: "NeedSilverLaunchable".Translate(this.fee.ToString()));
//                    }
//                    else if (!ColonyHasEnoughSteel(this.map, this.steelRequest))
//                    {
//                        accept.Disable("MFI_SellingWarBondsNeedSteelLaunchable".Translate(this.steelRequest.ToString()));
//                    }

//                    yield return accept;

//                    DiaOption reject = new DiaOption(text: "RansomDemand_Reject".Translate())
//                    {
//                        action = () => { Find.LetterStack.RemoveLetter(let: this); },
//                        resolveTree = true
//                    };
//                    yield return reject;
//                    yield return this.Option_Postpone;
//                }
//            }
//        }

//        public override bool CanShowInLetterStack => base.CanShowInLetterStack && Find.Maps.Contains(item: this.map);

//        public static bool ColonyHasEnoughSteel(Map map, int steelRequest)
//        {
//            return (from t in TradeUtility.AllLaunchableThingsForTrade(map)
//                    where t.def == ThingDefOf.Steel
//                    select t).Sum((Thing t) => t.stackCount) >= steelRequest;
//        }

//        public override void ExposeData()
//        {
//            base.ExposeData();
//            Scribe_References.Look<Map>(refee: ref this.map, label: "MFI_Shaman_Map");
//            Scribe_References.Look<Faction>(refee: ref this.faction, label: "MFI_Shaman_Faction");
//            Scribe_Values.Look(ref this.tile, "MFI_ShamanTile");
//            Scribe_Values.Look<int>(value: ref this.fee, label: "MFI_ShamanFee");
//        }
//    }
//}
