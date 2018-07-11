using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction
{
    public class IncidentWorker_DiplomaticMarriage : IncidentWorker
    {
        private Pawn marriageSeeker;
        private Pawn betrothed;
        private const int TimeoutTicks = GenDate.TicksPerDay;

        public override float AdjustedChance => base.AdjustedChance - Find.Storyteller.intenderPopulation.PopulationIntent;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && TryFindMarriageSeeker(out Pawn marriageSeeker) && TryFindBetrothed(out Pawn betrothed);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!TryFindMarriageSeeker(out marriageSeeker))
            {
                if (Prefs.LogVerbose) Log.Warning("no marriageseeker");
                return false;
            }
            if (!TryFindBetrothed(out betrothed))
            {
                if (Prefs.LogVerbose) Log.Warning ("no betrothed");
                return false;
            }

            ChoiceLetter_DiplomaticMarriage choiceLetter_DiplomaticMarriage = (ChoiceLetter_DiplomaticMarriage)LetterMaker.MakeLetter(this.def.letterLabel, "MFI_DiplomaticMarriage".Translate(new object[]
            {
                    marriageSeeker.LabelShort,
                    betrothed.LabelShort
            }).AdjustedFor(marriageSeeker), this.def.letterDef);

            choiceLetter_DiplomaticMarriage.title = "MFI_DiplomaticMarriageLabel".Translate(new object[]
            {
                    betrothed.LabelShort
            }).CapitalizeFirst();
            choiceLetter_DiplomaticMarriage.radioMode = true;
            choiceLetter_DiplomaticMarriage.marriageSeeker = marriageSeeker;
            choiceLetter_DiplomaticMarriage.betrothed = betrothed;
            choiceLetter_DiplomaticMarriage.StartTimeout(TimeoutTicks);
            Find.LetterStack.ReceiveLetter(choiceLetter_DiplomaticMarriage);
            return true;
        }

        private bool TryFindBetrothed(out Pawn betrothed) => (from potentialPartners in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep
                                                              select potentialPartners).TryRandomElementByWeight((Pawn marriageSeeker2) => this.marriageSeeker.relations.SecondaryLovinChanceFactor(marriageSeeker2), out betrothed);

        private static bool TryFindMarriageSeeker(out Pawn marriageSeeker) => (from x in Find.WorldPawns.AllPawnsAlive
                                                                        where x.Faction != null && !x.Faction.def.hidden && !x.Faction.def.permanentEnemy && !x.Faction.IsPlayer && !x.Faction.defeated
                                                                        && !SettlementUtility.IsPlayerAttackingAnySettlementOf(x.Faction) && !PeaceTalksExist(x.Faction)
                                                                        && x.Faction.leader != null && !x.Faction.leader.IsPrisoner && !x.Faction.leader.Spawned
                                                                        && (x.Faction.def.techLevel <= TechLevel.Medieval) /*|| x.Faction.def.techLevel == TechLevel.Archotech*/ // not today space kitties
                                                                        && !x.IsPrisoner && !x.Spawned
                                                                        && (!LovePartnerRelationUtility.HasAnyLovePartner(x) || ((LovePartnerRelationUtility.ExistingMostLikedLovePartner(x, false) is Pawn pawn && pawn.Faction is Faction faction && faction == Faction.OfPlayer))) // HOW I NULL COALESCE ??
                                                                        select x).TryRandomElement(out marriageSeeker); //todo: make more likely to select hostile.

        private static bool PeaceTalksExist(Faction faction)
        {
            List<PeaceTalks> peaceTalks = Find.WorldObjects.PeaceTalks;
            foreach (PeaceTalks peaceTalk in peaceTalks)
            {
                if (peaceTalk.Faction == faction)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
