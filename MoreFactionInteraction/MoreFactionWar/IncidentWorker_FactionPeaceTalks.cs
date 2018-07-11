using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction.MoreFactionWar
{
    public class IncidentWorker_FactionPeaceTalks : IncidentWorker
    {
        private static readonly IntRange TimeoutDaysRange = new IntRange(21, 23);

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms) && this.FoundTwoFactions() && TryFindTile(out int tile) && !Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarIsOngoing;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!this.FoundTwoFactions())
                return false;

            if (!TryFindTile(out int tile))
                return false;

            Faction faction = TryFindFactions(out Faction instigatingFaction);

            if (faction != null)
            {
                FactionWarPeaceTalks factionWarPeaceTalks = (FactionWarPeaceTalks)WorldObjectMaker.MakeWorldObject(MFI_DefOf.MFI_FactionWarPeaceTalks);
                factionWarPeaceTalks.Tile = tile;
                factionWarPeaceTalks.SetFaction(faction);
                factionWarPeaceTalks.SetWarringFactions(factionOne: faction, factionInstigator: instigatingFaction);
                int randomInRange = IncidentWorker_FactionPeaceTalks.TimeoutDaysRange.RandomInRange;
                factionWarPeaceTalks.GetComponent<TimeoutComp>().StartTimeout(randomInRange * GenDate.TicksPerDay);
                Find.WorldObjects.Add(factionWarPeaceTalks);
                string text = string.Format(this.def.letterText.AdjustedFor(faction.leader, "PAWN"), faction.def.leaderTitle, faction.Name, randomInRange).CapitalizeFirst();
                Find.LetterStack.ReceiveLetter(this.def.letterLabel, text, this.def.letterDef, factionWarPeaceTalks, faction, null);
                return true;
            }
            return false;
        }

        private static bool TryFindTile(out int tile)
        {
            return TileFinder.TryFindNewSiteTile(out tile, 5, 13, false, false, -1);
        }

        private bool FoundTwoFactions()
        {
            return TryFindFactions(out Faction instigatingFaction) != null;
        }

        private static Faction TryFindFactions(out Faction instigatingFaction)
        {
            IEnumerable<Faction> factions = Find.FactionManager.AllFactions.Where(x => !x.def.hidden && !x.defeated && !x.IsPlayer && !x.def.permanentEnemy);
            Faction alliedFaction = factions.RandomElement();

            IEnumerable<Faction> factionsPartTwo = Find.FactionManager.AllFactions.Where(x => !x.def.hidden && !x.defeated && !x.IsPlayer && !x.def.permanentEnemy && x != alliedFaction);

            return factionsPartTwo.TryRandomElement(out instigatingFaction) ? alliedFaction : null;
        }
    }
}
