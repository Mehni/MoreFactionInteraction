using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using RimWorld.Planet;
using MoreFactionInteraction.MoreFactionWar;

namespace MoreFactionInteraction
{
    public class ChoiceLetter_DiplomaticMarriageShotgunEdition : ChoiceLetter
    {
        //private readonly int goodWillGainedFromMarriage;
        public Pawn betrothedInternal;
        public Pawn marriageSeekerInternal;
        public List<ThingCount> dowry;

        public override bool CanShowInLetterStack => base.CanShowInLetterStack 
            && PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.Contains(value: this.marriageSeekerInternal);

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
                    DiaOption accept = new DiaOption(text: "RansomDemand_Accept".Translate())
                    {
                        action = () =>
                        {
                            if (this.marriageSeekerInternal.GetSpouse() == null)
                                this.marriageSeekerInternal.relations.AddDirectRelation(def: PawnRelationDefOf.Fiance, otherPawn: this.betrothedInternal);

                            DetermineAndDoOutcome(marriageSeeker: this.marriageSeekerInternal, betrothed: this.betrothedInternal);

                            Map map = this.marriageSeekerInternal.MapHeld;

                            if (map != null)
                                if (this.TryFindEntryCell(map, out IntVec3 loc))
                                    GenSpawn.Spawn(this.betrothedInternal, loc, map);

                            if (map == null && this.marriageSeekerInternal.GetCaravan() is Caravan caravan)
                                caravan.AddPawn(this.betrothedInternal, true);

                            if (!this.betrothedInternal.Spawned && this.betrothedInternal.GetCaravan() == null)
                                Log.Warning($"MFI: Could not find map entry point or caravan for {this.betrothedInternal.Name}.");

                            foreach (ThingCount item in dowry)
                            {
                                TradeUtility.LaunchThingsOfType(item.Thing.def, item.Count, map, null);
                            }

                            Find.LetterStack.RemoveLetter(this);
                        }
                    };

                    Map map2 = this.marriageSeekerInternal.MapHeld;

                    if (!ColonyCanAffordDowry(map2))
                        accept.Disable("MFI_CantAffordDowry".Translate());

                    DiaNode dialogueNodeAccept = new DiaNode(text: "MFI_AcceptedProposal".Translate(this.betrothedInternal, this.betrothedInternal.Faction).CapitalizeFirst().AdjustedFor(this.betrothedInternal));
                    dialogueNodeAccept.options.Add(item: this.Option_Close);
                    accept.link = dialogueNodeAccept;

                    DiaOption reject = new DiaOption(text: "RansomDemand_Reject".Translate())
                    {
                        action = () =>
                        {
                            //if (Rand.Chance(0.2f))
                            this.betrothedInternal.Faction.TryAffectGoodwillWith(other: Faction.OfPlayer, goodwillChange: DiplomacyTuning.Goodwill_PeaceTalksBackfireRange.RandomInRange, canSendMessage: true, canSendHostilityLetter: true, reason: "LetterLabelRejectedProposal".Translate());
                            Find.LetterStack.RemoveLetter(this);
                        }
                    };
                    DiaNode dialogueNodeReject = new DiaNode(text: "MFI_DejectedProposal".Translate(this.betrothedInternal.Label, this.betrothedInternal.Faction).CapitalizeFirst().AdjustedFor(this.betrothedInternal));
                    dialogueNodeReject.options.Add(item: this.Option_Close);
                    reject.link = dialogueNodeReject;

                    yield return accept;
                    yield return reject;
                    yield return this.Option_Postpone;
                }
            }
        }

        private bool ColonyCanAffordDowry(Map map)
        {
            Log.Message($"map: {map}");
            List<ThingCount> tempList = dowry;
            List<Thing> first = TradeUtility.AllLaunchableThingsForTrade(map).ToList();
            List<Thing> second = TradeUtility.AllSellableColonyPawns(map).Cast<Thing>().ToList();
            first.AddRange(second);

            var uuuh = first.GroupBy(x => x.def, x => x.stackCount, (key, g) => new { ThingDef = key, stackCount = g }).ToList();


            foreach (var item in tempList)
            {
                if (!uuuh.Any(x => x.ThingDef == item.Thing.def && x.stackCount.Sum() >= item.Count))
                    return false;
            }

            return true;
        }

        private static void DetermineAndDoOutcome(Pawn marriageSeeker, Pawn betrothed)
        {
            if (Prefs.LogVerbose)
                Log.Warning(text: "Determine and do outcome after marriage.");

            betrothed.SetFaction(marriageSeeker.Faction);

            //todo: maybe plan visit, deliver dowry, do wedding.
        }

        private bool TryFindEntryCell(Map map, out IntVec3 cell)
        {
            return CellFinder.TryFindRandomEdgeCellWith(c => map.reachability.CanReachColony(c), map, CellFinder.EdgeRoadChance_Neutral, out cell);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(refee: ref this.betrothedInternal, label: "betrothedInternal");
            Scribe_References.Look(refee: ref this.marriageSeekerInternal, label: "marriageSeekerInternal");
        }
    }
}
