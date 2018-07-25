using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using RimWorld.Planet;
using MoreFactionInteraction.MoreFactionWar;

namespace MoreFactionInteraction
{
    public class ChoiceLetter_DiplomaticMarriage : ChoiceLetter
    {
        private int goodWillGainedFromMarriage;
        public Pawn betrothed;
        public Pawn marriageSeeker;

        public override bool CanShowInLetterStack => base.CanShowInLetterStack && PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.Contains(value: this.betrothed);

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
                    //possible outcomes: 
                    //dowry
                    //goodwill based on pawn value
                    //wedding (doable)
                    //bring us the betrothed? (complicated.)
                    //betrothed picks a transport pod (meh)
                    DiaOption accept = new DiaOption(text: "RansomDemand_Accept".Translate())
                    {
                        action = () =>
                            {
                                this.goodWillGainedFromMarriage = (int)FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks.PawnValueInGoodWillAmountOut.Evaluate(x: this.betrothed.MarketValue);
                                    //(int)Mathf.Clamp(value: this.betrothed.MarketValue / 20, min: DiplomacyTuning.Goodwill_MemberExitedMapHealthy_LeaderBonus, max: FactionInteractionDiplomacyTuningsBlatantlyCopiedFromPeaceTalks.GoodWill_FactionWarPeaceTalks_ImpactHuge.RandomInRange);
                                this.marriageSeeker.Faction.TryAffectGoodwillWith(other: Faction.OfPlayer, goodwillChange: this.goodWillGainedFromMarriage, canSendMessage: true, canSendHostilityLetter: true, reason: "LetterLabelAcceptedProposal".Translate());
                                this.betrothed.relations.AddDirectRelation(def: PawnRelationDefOf.Fiance, otherPawn: this.marriageSeeker);

                                if (this.betrothed.GetCaravan() is Caravan caravan)
                                {
                                    CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(from: this.betrothed, candidates: caravan.PawnsListForReading);
                                    HealIfPossible(p: this.betrothed);
                                    caravan.RemovePawn(p: this.betrothed);
                                }

                                DetermineAndDoOutcome(marriageSeeker: this.marriageSeeker, betrothed: this.betrothed);
                        }
                    };
                    DiaNode dialogueNodeAccept = new DiaNode(text: "MFI_AcceptedProposal".Translate(this.betrothed, this.marriageSeeker.Faction).CapitalizeFirst().AdjustedFor(this.marriageSeeker));
                            dialogueNodeAccept.options.Add(item: this.Option_Close);
                            accept.link = dialogueNodeAccept;

                    DiaOption reject = new DiaOption(text: "RansomDemand_Reject".Translate())
                    {
                        action = () =>
                        {
                            //if (Rand.Chance(0.2f))
                            this.marriageSeeker.Faction.TryAffectGoodwillWith(other: Faction.OfPlayer, goodwillChange: DiplomacyTuning.Goodwill_PeaceTalksBackfireRange.RandomInRange, canSendMessage: true, canSendHostilityLetter: true, reason: "LetterLabelRejectedProposal".Translate());
                        }
                    };
                    DiaNode dialogueNodeReject = new DiaNode(text: "MFI_DejectedProposal".Translate(this.marriageSeeker.Name, this.marriageSeeker.Faction).CapitalizeFirst().AdjustedFor(this.marriageSeeker));
                            dialogueNodeReject.options.Add(item: this.Option_Close);
                            reject.link = dialogueNodeReject;

                    yield return accept;
                    yield return reject;
                    yield return this.Option_Postpone;
                }
            }
        }

        private static void DetermineAndDoOutcome(Pawn marriageSeeker, Pawn betrothed)
        {
            if (Prefs.LogVerbose) Log.Warning(text: "Determine and do outcome after marriage.");

            betrothed.SetFaction(newFaction: !marriageSeeker.HostileTo(fac: Faction.OfPlayer)
                                                 ? marriageSeeker.Faction
                                                 : null);

            //GenSpawn.Spawn(marriageSeeker, DropCellFinder.TradeDropSpot(betrothed.Map), betrothed.Map);
            //Lord PARTYHARD = LordMaker.MakeNewLord(betrothed.Faction, new LordJob_NonVoluntaryJoinable_MarriageCeremony(marriageSeeker, betrothed, DropCellFinder.TradeDropSpot(betrothed.Map)), betrothed.Map, null);
            //foreach (Pawn lazybum in betrothed.Map.mapPawns.FreeColonistsSpawned)
            //{
            //    PARTYHARD.AddPawn(lazybum);
            //} 
            ////betrothed.Map.lordsStarter.TryStartMarriageCeremony(betrothed, marriageSeeker);
            //IncidentParms parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.FactionArrival, marriageSeeker.Map);
            //parms.faction = marriageSeeker.Faction;
            //MFI_DefOf.MFI_WeddingGuestsArrival.Worker.TryExecute(parms);
        }

        private static void HealIfPossible(Pawn p)
        {
            List<Hediff> tmpHediffs = new List<Hediff>();
            tmpHediffs.AddRange(collection: p.health.hediffSet.hediffs);
            foreach (Hediff hediffTemp in tmpHediffs)
            {
                if (hediffTemp is Hediff_Injury hediffInjury && !hediffInjury.IsPermanent())
                {
                    p.health.RemoveHediff(hediff: hediffInjury);
                }
                else
                {
                    ImmunityRecord immunityRecord = p.health.immunity.GetImmunityRecord(def: hediffTemp.def);
                    if (immunityRecord != null)
                        immunityRecord.immunity = 1f;
                }
            }
            tmpHediffs.Clear();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Pawn>(refee: ref this.betrothed, label: "betrothed");
            Scribe_References.Look<Pawn>(refee: ref this.marriageSeeker, label: "marriageSeeker");
        }
    }
}
