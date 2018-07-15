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
            return base.CanFireNowSub(parms: parms) && TryFindMarriageSeeker(marriageSeeker: out Pawn marriageSeeker) && this.TryFindBetrothed(betrothed: out Pawn betrothed);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!TryFindMarriageSeeker(marriageSeeker: out this.marriageSeeker))
            {
                if (Prefs.LogVerbose) Log.Warning(text: "no marriageseeker");
                return false;
            }
            if (!this.TryFindBetrothed(betrothed: out this.betrothed))
            {
                if (Prefs.LogVerbose) Log.Warning (text: "no betrothed");
                return false;
            }

            ChoiceLetter_DiplomaticMarriage choiceLetter_DiplomaticMarriage = (ChoiceLetter_DiplomaticMarriage)LetterMaker.MakeLetter(label: this.def.letterLabel, text: "MFI_DiplomaticMarriage".Translate(args: new object[]
            {
                this.marriageSeeker.LabelShort, this.betrothed.LabelShort
            }).AdjustedFor(p: this.marriageSeeker), def: this.def.letterDef);

            choiceLetter_DiplomaticMarriage.title = "MFI_DiplomaticMarriageLabel".Translate(args: new object[]
            {
                this.betrothed.LabelShort
            }).CapitalizeFirst();
            choiceLetter_DiplomaticMarriage.radioMode = true;
            choiceLetter_DiplomaticMarriage.marriageSeeker = this.marriageSeeker;
            choiceLetter_DiplomaticMarriage.betrothed = this.betrothed;
            choiceLetter_DiplomaticMarriage.StartTimeout(duration: TimeoutTicks);
            Find.LetterStack.ReceiveLetter(@let: choiceLetter_DiplomaticMarriage);
            return true;
        }

        private bool TryFindBetrothed(out Pawn betrothed) => (from potentialPartners in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep
                                                              select potentialPartners).TryRandomElementByWeight(weightSelector: (Pawn marriageSeeker2) => this.marriageSeeker.relations.SecondaryLovinChanceFactor(otherPawn: marriageSeeker2), result: out betrothed);

        private static bool TryFindMarriageSeeker(out Pawn marriageSeeker) => (from x in Find.WorldPawns.AllPawnsAlive
                                                                        where x.Faction != null && !x.Faction.def.hidden && !x.Faction.def.permanentEnemy && !x.Faction.IsPlayer && !x.Faction.defeated
                                                                        && !SettlementUtility.IsPlayerAttackingAnySettlementOf(faction: x.Faction) && !PeaceTalksExist(faction: x.Faction)
                                                                        && x.Faction.leader != null && !x.Faction.leader.IsPrisoner && !x.Faction.leader.Spawned
                                                                        && (x.Faction.def.techLevel <= TechLevel.Medieval) /*|| x.Faction.def.techLevel == TechLevel.Archotech*/ // not today space kitties
                                                                        && !x.IsPrisoner && !x.Spawned
                                                                        && (!LovePartnerRelationUtility.HasAnyLovePartner(pawn: x) || LovePartnerRelationUtility.ExistingMostLikedLovePartner(p: x, allowDead: false)?.Faction == Faction.OfPlayer)
                                                                               select x).TryRandomElement(result: out marriageSeeker); //todo: make more likely to select hostile.

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
