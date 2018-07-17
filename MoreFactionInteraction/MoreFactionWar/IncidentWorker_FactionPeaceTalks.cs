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
        private static readonly IntRange TimeoutDaysRange = new IntRange(min: 21, max: 23);

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms: parms) && this.FoundTwoFactions() && TryFindTile(tile: out int tile) && !Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarIsOngoing;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!this.FoundTwoFactions())
                return false;

            if (!TryFindTile(tile: out int tile))
                return false;

            Faction faction = TryFindFactions(instigatingFaction: out Faction instigatingFaction);

            if (faction != null)
            {
                FactionWarPeaceTalks factionWarPeaceTalks = (FactionWarPeaceTalks)WorldObjectMaker.MakeWorldObject(def: MFI_DefOf.MFI_FactionWarPeaceTalks);
                factionWarPeaceTalks.Tile = tile;
                factionWarPeaceTalks.SetFaction(newFaction: faction);
                factionWarPeaceTalks.SetWarringFactions(factionOne: faction, factionInstigator: instigatingFaction);
                int randomInRange = TimeoutDaysRange.RandomInRange;
                factionWarPeaceTalks.GetComponent<TimeoutComp>().StartTimeout(ticks: randomInRange * GenDate.TicksPerDay);
                Find.WorldObjects.Add(o: factionWarPeaceTalks);
                string text = string.Format(format: this.def.letterText.AdjustedFor(p: faction.leader), faction.def.leaderTitle, faction.Name, instigatingFaction.Name, randomInRange).CapitalizeFirst();
                Find.LetterStack.ReceiveLetter(label: this.def.letterLabel, text: text, textLetterDef: this.def.letterDef, lookTargets: factionWarPeaceTalks, relatedFaction: faction);
                Find.World.GetComponent<WorldComponent_MFI_FactionWar>().StartUnrest(faction, instigatingFaction);
                return true;
            }
            return false;
        }

        private static bool TryFindTile(out int tile)
        {
            return TileFinder.TryFindNewSiteTile(tile: out tile, minDist: 5, maxDist: 13, allowCaravans: false, preferCloserTiles: false);
        }

        private bool FoundTwoFactions()
        {
            return TryFindFactions(instigatingFaction: out Faction instigatingFaction) != null;
        }

        private static Faction TryFindFactions(out Faction instigatingFaction)
        {
            IEnumerable<Faction> factions = Find.FactionManager.AllFactions.Where(predicate: x => !x.def.hidden && !x.defeated && !x.IsPlayer && !x.def.permanentEnemy);
            Faction alliedFaction = factions.RandomElement();

            IEnumerable<Faction> factionsPartTwo = Find.FactionManager.AllFactions.Where(predicate: x => !x.def.hidden && !x.defeated && !x.IsPlayer && !x.def.permanentEnemy && x != alliedFaction);

            return factionsPartTwo.TryRandomElement(result: out instigatingFaction) ? alliedFaction : null;
        }
    }
}
