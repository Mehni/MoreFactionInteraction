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
        private Faction faction;
        private Pawn pawn;
        private const int TimeoutTicks = GenDate.TicksPerDay;

        public override float AdjustedChance => base.AdjustedChance - Find.Storyteller.intenderPopulation.PopulationIntent;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return true; // base.CanFireNowSub(parms) && this.TryFindFaction(out Faction faction) && TryFindBetrothed(out Pawn pawn);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!TryFindFaction(out faction)) { Log.Warning("no faction"); return false; }
            if (!TryFindBetrothed(out pawn)) { Log.Warning("no betrothed"); return false; }

            Log.Message(pawn.LabelShort);
            Log.Message(faction.Name);
            Log.Message("sending letter");
            ChoiceLetter_DiplomaticMarriage choiceLetter_DiplomaticMarriage = (ChoiceLetter_DiplomaticMarriage)LetterMaker.MakeLetter(this.def.letterLabel, "MFI_DiplomaticMarriage".Translate(new object[]
            {
                    faction.leader.LabelShort,
                    pawn.LabelShort
            }).AdjustedFor(faction.leader), this.def.letterDef);
            Log.Message("3");
            choiceLetter_DiplomaticMarriage.title = "MFI_DiplomaticMarriageLabel".Translate(new object[]
            {
                    pawn.LabelShort
            }).CapitalizeFirst();
            choiceLetter_DiplomaticMarriage.radioMode = true;
            choiceLetter_DiplomaticMarriage.faction = faction;
            choiceLetter_DiplomaticMarriage.betrothed = pawn;
            choiceLetter_DiplomaticMarriage.StartTimeout(TimeoutTicks);
            Find.LetterStack.ReceiveLetter(choiceLetter_DiplomaticMarriage);
            return true;
        }

        private bool TryFindBetrothed(out Pawn pawn)
        {
            Log.Message("2");
            return (from potentialPartners in PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners_NoCryptosleep
                    select potentialPartners).TryRandomElement(out pawn);
        }

        private bool TryFindFaction(out Faction faction)
        {
            Log.Message("1");
            return (from x in Find.FactionManager.AllFactions
                    where !x.def.hidden && !x.def.permanentEnemy && !x.IsPlayer && !x.defeated
                    && !SettlementUtility.IsPlayerAttackingAnySettlementOf(x) && !this.PeaceTalksExist(x)
                    && x.leader != null && !x.leader.IsPrisoner && !x.leader.Spawned
                    && (x.def.techLevel <= TechLevel.Medieval /*|| x.def.techLevel == TechLevel.Archotech*/) // not today space kitties
                    select x).TryRandomElement/*ByWeight(x => -x.PlayerGoodwill,*/ (out faction); //more likely to select hostile.
        }

        private bool PeaceTalksExist(Faction faction)
        {
            List<PeaceTalks> peaceTalks = Find.WorldObjects.PeaceTalks;
            for (int i = 0; i < peaceTalks.Count; i++)
            {
                if (peaceTalks[i].Faction == faction)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
