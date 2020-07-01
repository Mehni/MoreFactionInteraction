﻿using RimWorld;
using Verse;
using RimWorld.Planet;
using System.Linq;
using MoreFactionInteraction.MoreFactionWar;
using System.Collections;
using System.Collections.Generic;

namespace MoreFactionInteraction
{
    public class WorldComponent_MFI_FactionWar : WorldComponent
    {
        private Faction factionOne;
        private Faction factionTwo;
        private bool    warIsOngoing;
        private bool    unrestIsBrewing;
        
        private int    factionOneBattlesWon = 1;
        private int    factionTwoBattlesWon = 1;

        private readonly List<Faction> allFactionsInVolvedInWar = new List<Faction>();

        public List<Faction> AllFactionsInVolvedInWar
        {
            get
            {
                if (this.allFactionsInVolvedInWar.Count == 0)
                {
                    this.allFactionsInVolvedInWar.Add(WarringFactionOne);
                    this.allFactionsInVolvedInWar.Add(WarringFactionTwo);
                }
                return this.allFactionsInVolvedInWar;
            }
        }

        public Faction WarringFactionOne
        {
            get
            {
                return factionOne;
            }
        }

        public Faction WarringFactionTwo
        {
            get
            {
                return factionTwo;
            }
        }

        public bool WarIsOngoing => this.warIsOngoing;
        public bool UnrestIsBrewing => this.unrestIsBrewing;
        public bool StuffIsGoingDown => this.unrestIsBrewing || this.warIsOngoing;

        private void SetFirstWarringFaction(Faction faction)
        {
            this.factionOne = faction;
        }

        private void SetSecondWarringFaction(Faction faction)
        {
            this.factionTwo = faction;
        }

        public WorldComponent_MFI_FactionWar(World world) : base (world: world)
        {
            this.world = world;
        }

        /// <summary>
        /// Starts the war and sets up stuff.
        /// </summary>
        /// <param name="factionOne"></param>
        /// <param name="factionInstigator"></param>
        /// <param name="selfResolved">Used in cases where we don't want to send standard letter. (i.e. DetermineWarAsIfNoPlayerInteraction)</param>
        public void StartWar(Faction factionOne, Faction factionInstigator, bool selfResolved)
        {
            this.warIsOngoing = true;
            this.unrestIsBrewing = false;
            this.SetFirstWarringFaction(factionOne);
            this.SetSecondWarringFaction(factionInstigator);
            this.factionOneBattlesWon = 1;
            this.factionTwoBattlesWon = 1;
            factionOne.TrySetRelationKind(factionInstigator, FactionRelationKind.Hostile, false);
            factionInstigator.TrySetRelationKind(factionOne, FactionRelationKind.Hostile, false);

            if (selfResolved)
                return;

            WorldObject peacetalks =
                (WorldObject) (Find.WorldObjects.AllWorldObjects.FirstOrDefault(x => x.def == MFI_DefOf.MFI_FactionWarPeaceTalks) 
                            ?? GlobalTargetInfo.Invalid);

            Find.LetterStack.ReceiveLetter("MFI_FactionWarStarted".Translate(),
                                           "MFI_FactionWarExplanation".Translate(factionOne.Name, factionInstigator.Name),
                                           LetterDefOf.NegativeEvent, new GlobalTargetInfo(peacetalks));
        }

        public void StartUnrest(Faction factionOne, Faction factionTwo)
        {
            this.unrestIsBrewing = true;
            this.SetFirstWarringFaction(factionOne);
            this.SetSecondWarringFaction(factionTwo);
        }

        private void ResolveWar()
        {
            this.warIsOngoing = false;
            this.SetFirstWarringFaction(null);
            this.SetSecondWarringFaction(null);
            this.allFactionsInVolvedInWar.Clear();
            MainTabWindow_FactionWar.ResetBars();

            Find.LetterStack.ReceiveLetter("MFI_FactionWarOverLabel".Translate(), "MFI_FactionWarOver".Translate(WarringFactionOne, WarringFactionTwo), LetterDefOf.PositiveEvent);
        }

        public void AllOuttaFactionSettlements()
        {
            this.ResolveWar();
        }

        public float ScoreForFaction(Faction faction)
        {
            if (faction == this.factionOne)
                return (float)this.factionOneBattlesWon / (this.factionOneBattlesWon + this.factionTwoBattlesWon);

            if (faction == this.factionTwo)
                return (float)this.factionTwoBattlesWon / (this.factionOneBattlesWon + this.factionTwoBattlesWon);

            return 0f;
        }

        public void NotifyBattleWon(Faction faction)
        {
            if (faction == this.factionOne)
                this.factionOneBattlesWon++;

            if (faction == this.factionTwo)
                this.factionTwoBattlesWon++;

            if ((this.factionOneBattlesWon + this.factionTwoBattlesWon >= 12 && Rand.Chance(0.75f)) ||
                (this.factionOneBattlesWon + this.factionTwoBattlesWon >= 8 && Rand.Chance(0.25f)))
            {
                this.ResolveWar();
            }
        }

        public override void ExposeData()
        {
            Scribe_References.Look(ref this.factionOne, "MFI_WarringFactionOne");
            Scribe_References.Look(ref this.factionTwo, "MFI_WarringFactionTwo");

            Scribe_Values.Look(ref this.warIsOngoing,    "MFI_warIsOngoing");
            Scribe_Values.Look(ref this.unrestIsBrewing, "MFI_UnrestIsBrewing");

            Scribe_Values.Look(ref this.factionOneBattlesWon, "MFI_factionOneScore", 1);
            Scribe_Values.Look(ref this.factionTwoBattlesWon, "MFI_factionTwoScore", 1);
        }

        public void DetermineWarAsIfNoPlayerInteraction(Faction faction, Faction factionInstigator)
        {
            this.unrestIsBrewing = false;

            DesiredOutcome rollForIntendedOutcome = Rand.Bool
                                                    ? DesiredOutcome.CURRY_FAVOUR_FACTION_ONE
                                                    : DesiredOutcome.BROKER_PEACE;

            if (faction.leader?.GetStatValue(StatDefOf.NegotiationAbility) != null)
            {
                var dialogue = new FactionWarDialogue(faction.leader, faction, factionInstigator, null);
                dialogue.DetermineOutcome(rollForIntendedOutcome, out _);

                Find.LetterStack.ReceiveLetter("MFI_FactionWarLeaderDecidedLabel".Translate(),
                                               WarIsOngoing ? "MFI_FactionWarLeaderDecidedOnWar".Translate(faction, factionInstigator)
                                                            : "MFI_FactionWarLeaderDecidedAgainstWar".Translate(faction, factionInstigator),
                                               WarIsOngoing ? LetterDefOf.NegativeEvent : LetterDefOf.NeutralEvent);
                return;
            }
            else if (factionInstigator.leader?.GetStatValue(StatDefOf.NegotiationAbility) != null)
            {
                var dialogue = new FactionWarDialogue(factionInstigator.leader, factionInstigator, faction, null);
                dialogue.DetermineOutcome(rollForIntendedOutcome, out _);

                Find.LetterStack.ReceiveLetter("MFI_FactionWarLeaderDecidedLabel".Translate(),
                                               WarIsOngoing ? "MFI_FactionWarLeaderDecidedOnWar".Translate(factionInstigator, faction)
                                                            : "MFI_FactionWarLeaderDecidedAgainstWar".Translate(factionInstigator, faction),
                                               WarIsOngoing ? LetterDefOf.NegativeEvent : LetterDefOf.NeutralEvent);
                return;
            }
        }
    }
}
