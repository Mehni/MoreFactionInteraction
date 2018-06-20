using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;

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
                    yield return base.OK;
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
                            int goodWillGainedFromMarriage = (int)(betrothed.MarketValue / InteractionDefOf.RomanceAttempt.Worker.RandomSelectionWeight(marriageSeeker, betrothed));
                            Log.Message(goodWillGainedFromMarriage.ToString() + " = " + betrothed.MarketValue + " / " + InteractionDefOf.RomanceAttempt.Worker.RandomSelectionWeight(marriageSeeker, betrothed).ToString());
                            marriageSeeker.Faction.TryAffectGoodwillWith(Faction.OfPlayer, goodWillGainedFromMarriage, true, false, "They're getting it on now!");
                            betrothed.relations.AddDirectRelation(PawnRelationDefOf.Fiance, marriageSeeker);
                            if (betrothed.GetCaravan() is Caravan caravan)
                            {
                                CaravanInventoryUtility.MoveAllInventoryToSomeoneElse(betrothed, caravan.PawnsListForReading);
                                caravan.RemovePawn(betrothed);
                                HealIfPossible(betrothed);
                            }
                            if (!marriageSeeker.HostileTo(Faction.OfPlayer))
                                betrothed.SetFaction(marriageSeeker.Faction);
                            else
                                betrothed.SetFaction(null);
                        }
                    };
                    DiaNode dialogueNodeAccept = new DiaNode("MFI_TraderSent".Translate().CapitalizeFirst());
                            dialogueNodeAccept.options.Add(base.OK);
                            accept.link = dialogueNodeAccept;

                    DiaOption reject = new DiaOption("RansomDemand_Reject".Translate())
                    {
                        action = () =>
                        {
                            IncidentParms parms = new IncidentParms
                            {
                                target = betrothed.Map,
                                faction = marriageSeeker.Faction,
                                points = 10000f
                            };
                            IncidentDefOf.RaidEnemy.Worker.TryExecute(parms);
                        }
                    };
                    DiaNode dialogueNodeReject = new DiaNode("MFI_TraderSent".Translate().CapitalizeFirst());
                            dialogueNodeReject.options.Add(base.OK);
                            reject.link = dialogueNodeReject;

                    yield return accept;
                    yield return reject;
                    yield return base.Postpone;
                }
            }
        }

        private static void HealIfPossible(Pawn p)
        {
            List<Hediff> tmpHediffs = new List<Hediff>();
            tmpHediffs.AddRange(p.health.hediffSet.hediffs);
            for (int i = 0; i < tmpHediffs.Count; i++)
            {
                if (tmpHediffs[i] is Hediff_Injury hediff_Injury && !hediff_Injury.IsPermanent())
                {
                    p.health.RemoveHediff(hediff_Injury);
                }
                else
                {
                    ImmunityRecord immunityRecord = p.health.immunity.GetImmunityRecord(tmpHediffs[i].def);
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
