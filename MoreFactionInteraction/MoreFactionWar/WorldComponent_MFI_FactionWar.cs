using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace MoreFactionInteraction
{
    public class WorldComponent_MFI_FactionWar : WorldComponent
    {
        private Faction factionOne;
        private Faction factionTwo;
        private bool    warIsOngoing;
        private bool    unrestIsBrewing;
        private int     factionOneBattlesWon = 10;
        private int     factionTwoBattlesWon = 10;


        public Faction WarringFactionOne
        {
            get
            {
                return this.factionOne;
            }
        }

        public Faction WarringFactionTwo
        {
            get
            {
                return this.factionTwo;
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

        public void StartWar(Faction factionOne, Faction factionTwo = null)
        {
            this.warIsOngoing = true;
            this.unrestIsBrewing = false;
            this.SetFirstWarringFaction(factionOne);
            this.SetSecondWarringFaction(factionTwo);
            factionOne.TrySetRelationKind(factionTwo, FactionRelationKind.Hostile, false);
            factionTwo?.TrySetRelationKind(factionOne, FactionRelationKind.Hostile, false);
        }

        public void StartUnrest(Faction factionOne, Faction factionTwo = null)
        {
            this.unrestIsBrewing = true;
            this.SetFirstWarringFaction(factionOne);
            this.SetSecondWarringFaction(factionTwo);
        }
        
        public void ResolveWar()
        {
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


        public override void ExposeData()
        {
            Scribe_References.Look(ref this.factionOne, "MFI_WarringFactionOne");
            Scribe_References.Look(ref this.factionTwo, "MFI_WarringFactionTwo");

            Scribe_Values.Look(ref this.warIsOngoing,    "MFI_warIsOngoing");
            Scribe_Values.Look(ref this.unrestIsBrewing, "MFI_UnrestIsBrewing");

            Scribe_Values.Look(ref this.factionOneBattlesWon, "MFI_factionOneScore", 10);
            Scribe_Values.Look(ref this.factionTwoBattlesWon, "MFI_factionTwoScore", 10);
        }

        public void NotifyBattleWon(Faction faction)
        {
            if (faction == this.factionOne) this.factionOneBattlesWon++;

            if (faction == this.factionTwo) this.factionTwoBattlesWon++;
        }
    }
}
