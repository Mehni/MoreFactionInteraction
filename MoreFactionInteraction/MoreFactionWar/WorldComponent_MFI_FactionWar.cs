using RimWorld;
using Verse;
using RimWorld.Planet;

namespace MoreFactionInteraction
{
    using System.Linq;
    using MoreFactionWar;

    public class WorldComponent_MFI_FactionWar : WorldComponent
    {
        private Faction factionOne;
        private Faction factionTwo;
        private bool    warIsOngoing;
        private bool    unrestIsBrewing;
        
        private int    factionOneBattlesWon = 1;
        private int    factionTwoBattlesWon = 1;


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
        /// <param name="factionTwo"></param>
        /// <param name="canSendLetter">Used in cases where we don't want to send standard letter. (i.e. DetermineWarAsIfNoPlayerInteraction)</param>
        public void StartWar(Faction factionOne, Faction factionTwo, bool canSendLetter)
        {
            this.warIsOngoing = true;
            this.unrestIsBrewing = false;
            this.SetFirstWarringFaction(factionOne);
            this.SetSecondWarringFaction(factionTwo);
            this.factionOneBattlesWon = 1;
            this.factionTwoBattlesWon = 2;
            factionOne.TrySetRelationKind(factionTwo, FactionRelationKind.Hostile, false);
            factionTwo.TrySetRelationKind(factionOne, FactionRelationKind.Hostile, false);

            if (!canSendLetter) return;

            WorldObject peacetalks =
                (WorldObject) (Find.WorldObjects.AllWorldObjects.FirstOrDefault(x => x.def == MFI_DefOf.MFI_FactionWarPeaceTalks) 
                            ?? GlobalTargetInfo.Invalid);

            Find.LetterStack.ReceiveLetter("MFI_FactionWarStarted".Translate(),
                                           "MFI_FactionWarExplanation".Translate(factionOne.Name, factionTwo.Name),
                                           LetterDefOf.NegativeEvent, new GlobalTargetInfo(peacetalks), factionOne);
        }

        public void StartUnrest(Faction factionOne, Faction factionTwo)
        {
            this.unrestIsBrewing = true;
            this.SetFirstWarringFaction(factionOne);
            this.SetSecondWarringFaction(factionTwo);
        }
        
        public void ResolveWar()
        {
            Find.LetterStack.ReceiveLetter("MFI_FactionWarOverLabel".Translate(), "MFI_FactionWarOver".Translate(WarringFactionOne, WarringFactionTwo), LetterDefOf.PositiveEvent);
            this.warIsOngoing = false;
            this.SetFirstWarringFaction(null);
            this.SetSecondWarringFaction(null);
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
            if (faction == this.factionOne) this.factionOneBattlesWon++;

            if (faction == this.factionTwo) this.factionTwoBattlesWon++;

            if (this.factionOneBattlesWon + this.factionTwoBattlesWon == 10 || Rand.Chance(0.25f))  this.ResolveWar();
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

            if (faction.leader?.GetStatValue(StatDefOf.NegotiationAbility) != null)
            {
                int rollForIntendedOutcome = Rand.Bool ? 1 : 4;
                FactionWarDialogue.DetermineOutcome(faction, factionInstigator, faction.leader, rollForIntendedOutcome, out string blah);
                Find.LetterStack.ReceiveLetter("MFI_FactionWarLeaderDecidedLabel".Translate(), "MFI_FactionWarLeaderDecided".Translate(), LetterDefOf.NeutralEvent);
            }
        }
    }
}
