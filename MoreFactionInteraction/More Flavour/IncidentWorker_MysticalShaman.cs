using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace MoreFactionInteraction
{
    class IncidentWorker_MysticalShaman : IncidentWorker
    {
        private static List<Map> tmpAvailableMaps = new List<Map>();

        public override float AdjustedChance => base.AdjustedChance;

        private const int MinDistance = 8;
        private const int MaxDistance = 22;
        private const int TimeoutTicks = GenDate.TicksPerDay;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms: parms) && Find.AnyPlayerHomeMap != null 
                                             && !Find.WorldObjects.AllWorldObjects.Any(predicate: o => o.def == MFI_DefOf.MFI_MysticalShaman)
                                             && Find.FactionManager.AllFactionsVisible.Where(predicate: f => f.def.techLevel <= TechLevel.Neolithic 
                                                                                               && !f.HostileTo(other: Faction.OfPlayer)).TryRandomElement(result: out Faction result)
                                             && TryFindTile(tile: out int num)
                                             && TryGetRandomAvailableTargetMap (map: out Map map);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!Find.FactionManager.AllFactionsVisible.Where(predicate: f => f.def.techLevel <= TechLevel.Neolithic
                                                                && !f.HostileTo(other: Faction.OfPlayer)).TryRandomElement(result: out Faction faction))
                return false;

            if (!TryGetRandomAvailableTargetMap(map: out Map map))
                return false;

            if (faction == null)
                return false;

            if (map == null)
                return false;

            if (!TryFindTile(tile: out int tile))
                return false;

            int fee = Rand.RangeInclusive(min: 400, max: 1000);

            ChoiceLetter_MysticalShaman choiceLetterMysticalShaman = (ChoiceLetter_MysticalShaman)LetterMaker.MakeLetter(label: this.def.letterLabel, text: "MFI_MysticalShamanLetter".Translate(faction.Name, fee.ToString()), def: this.def.letterDef);
            choiceLetterMysticalShaman.title = "MFI_MysticalShamanTitle".Translate().CapitalizeFirst();
            choiceLetterMysticalShaman.faction = faction;
            choiceLetterMysticalShaman.tile = tile;
            choiceLetterMysticalShaman.map = map;
            choiceLetterMysticalShaman.fee = fee;
            choiceLetterMysticalShaman.StartTimeout(duration: TimeoutTicks);
            Find.LetterStack.ReceiveLetter(let: choiceLetterMysticalShaman);
            return true;
        }

        private static bool TryFindTile(out int tile)
        {
            return TileFinder.TryFindNewSiteTile(tile: out tile, minDist: MinDistance, maxDist: MaxDistance, allowCaravans: true, preferCloserTiles: false);
        }

        private static bool TryGetRandomAvailableTargetMap(out Map map)
        {
            tmpAvailableMaps.Clear();
            List<Map> maps = Find.Maps;
            foreach (Map potentialTargetMap in maps)
            {
                if (potentialTargetMap.IsPlayerHome && IncidentWorker_QuestTradeRequest.RandomNearbyTradeableSettlement(originTile: potentialTargetMap.Tile) != null)
                {
                    tmpAvailableMaps.Add(item: potentialTargetMap);
                }
            }
            bool result = tmpAvailableMaps.TryRandomElement(result: out map);
            tmpAvailableMaps.Clear();
            return result;
        }
    }
}
