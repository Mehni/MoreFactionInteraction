using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;
using UnityEngine;
using Verse.AI;
using Verse.AI.Group;

namespace MoreFactionInteraction
{
    public class ChoiceLetter_DiplomaticMarriage : ChoiceLetter
    {
        public Pawn betrothed;
        public Pawn marriageSeeker;

        public override bool CanShowInLetterStack
        {
            get
            {
                return base.CanShowInLetterStack && PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonists.Contains(this.betrothed);
            }
        }

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (base.ArchivedOnly)
                {
                    yield return base.Option_Close;
                }
                else
                {
                    //possible outcomes: 
                    //dowry
                    //goodwill based on pawn value
                    //wedding (doable)
                    //bring us the betrothed? (complicated.)
                    //betrothed picks a transport pod (meh)
                    DiaOption accept = new DiaOption("RansomDemand_Accept".Translate())
                    {
                        action = () =>
                        {
                            int goodWillGainedFromMarriage = (int)Mathf.Clamp(((betrothed.MarketValue / 20) * marriageSeeker.relations.SecondaryLovinChanceFactor(betrothed)), DiplomacyTuning.Goodwill_MemberExitedMapHealthy_LeaderBonus, DiplomacyTuning.Goodwill_PeaceTalksSuccessRange.RandomInRange);
                            marriageSeeker.Faction.TryAffectGoodwillWith(Faction.OfPlayer, goodWillGainedFromMarriage, true, true, "LetterLabelAcceptedProposal".Translate());
                            betrothed.relations.AddDirectRelation(PawnRelationDefOf.Fiance, marriageSeeker);

                            if (betrothed.GetCaravan() is Caravan caravan)
                            {
                                CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(betrothed, caravan.PawnsListForReading);
                                HealIfPossible(betrothed);
                                caravan.RemovePawn(betrothed);
                            }

                            DetermineAndDoOutcome(marriageSeeker, betrothed);
                        }
                    };
                    DiaNode dialogueNodeAccept = new DiaNode("MFI_AcceptedProposal".Translate().CapitalizeFirst());
                            dialogueNodeAccept.options.Add(base.Option_Close);
                            accept.link = dialogueNodeAccept;

                    DiaOption reject = new DiaOption("RansomDemand_Reject".Translate())
                    {
                        action = () =>
                        {
                            //if (Rand.Chance(0.2f))
                            marriageSeeker.Faction.TryAffectGoodwillWith(Faction.OfPlayer, DiplomacyTuning.Goodwill_PeaceTalksBackfireRange.RandomInRange, true, true, "LetterLabelRejectedProposal".Translate());
                        }
                    };
                    DiaNode dialogueNodeReject = new DiaNode("MFI_DejectedProposal".Translate().CapitalizeFirst());
                            dialogueNodeReject.options.Add(base.Option_Close);
                            reject.link = dialogueNodeReject;

                    yield return accept;
                    yield return reject;
                    yield return base.Option_Postpone;
                }
            }
        }

        private static void DetermineAndDoOutcome(Pawn marriageSeeker, Pawn betrothed)
        {
            if (Prefs.LogVerbose) Log.Warning(" Determine and do outcome after marriage.");

            if (!marriageSeeker.HostileTo(Faction.OfPlayer))
                betrothed.SetFaction(marriageSeeker.Faction);
            else
                betrothed.SetFaction(null);

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
            tmpHediffs.AddRange(p.health.hediffSet.hediffs);
            foreach (Hediff hediffTemp in tmpHediffs)
            {
                if (hediffTemp is Hediff_Injury hediff_Injury && !hediff_Injury.IsPermanent())
                {
                    p.health.RemoveHediff(hediff_Injury);
                }
                else
                {
                    ImmunityRecord immunityRecord = p.health.immunity.GetImmunityRecord(hediffTemp.def);
                    if (immunityRecord != null)
                    {
                        immunityRecord.immunity = 1f;
                    }
                }
            }
            tmpHediffs.Clear();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look<Pawn>(ref this.betrothed, "betrothed");
            Scribe_References.Look<Pawn>(ref this.marriageSeeker, "marriageSeeker");
        }
    }
}
