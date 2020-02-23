﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace MoreFactionInteraction
{
    using More_Flavour;

    public class IncidentWorker_MysticalShaman : IncidentWorker
    {

        public override float BaseChanceThisGame => base.BaseChanceThisGame;

        private const int MinDistance = 8;
        private const int MaxDistance = 22;
        private static readonly IntRange TimeoutDaysRange = new IntRange(min: 5, max: 15);

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms: parms) && Find.AnyPlayerHomeMap != null
                                             && !Find.WorldObjects.AllWorldObjects.Any(predicate: o => o.def == MFI_DefOf.MFI_MysticalShaman)
                                             && Find.FactionManager.AllFactionsVisible.Where(predicate: f => f.def.techLevel <= TechLevel.Neolithic
                                                                                               && !f.HostileTo(other: Faction.OfPlayer)).TryRandomElement(result: out Faction result)
                                             && TryFindTile(tile: out int num)
                                             && TryGetRandomAvailableTargetMap(map: out Map map)
                                             && CommsConsoleUtility.PlayerHasPoweredCommsConsole();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!Find.FactionManager.AllFactionsVisible.Where(predicate: f => f.def.techLevel <= TechLevel.Neolithic
                                                                && !f.HostileTo(other: Faction.OfPlayer)).TryRandomElement(result: out Faction faction))
                return false;

            if (faction == null)
                return false;

            if (!TryGetRandomAvailableTargetMap(map: out Map map))
                return false;

            if (map == null)
                return false;

            if (!TryFindTile(tile: out int tile))
                return false;

            int fee = Rand.RangeInclusive(min: 400, max: 1000);

            DiaNode diaNode = new DiaNode("MFI_MysticalShamanLetter".Translate(faction.Name, fee.ToString()));
            DiaOption accept = new DiaOption(text: "RansomDemand_Accept".Translate())
            {
                action = () =>
                         {
                             MysticalShaman mysticalShaman = (MysticalShaman)WorldObjectMaker.MakeWorldObject(def: MFI_DefOf.MFI_MysticalShaman);
                             mysticalShaman.Tile = tile;
                             mysticalShaman.SetFaction(newFaction: faction);
                             int randomInRange = TimeoutDaysRange.RandomInRange;
                             mysticalShaman.GetComponent<TimeoutComp>().StartTimeout(ticks: randomInRange * GenDate.TicksPerDay);
                             Find.WorldObjects.Add(o: mysticalShaman);
                             TradeUtility.LaunchSilver(map: map, fee: fee);
                         },
                resolveTree = true
            };
            if (!TradeUtility.ColonyHasEnoughSilver(map: map, fee: fee))
            {
                accept.Disable(newDisabledReason: "NeedSilverLaunchable".Translate(fee.ToString()));
            }

            DiaOption reject = new DiaOption(text: "RansomDemand_Reject".Translate())
            {
                action = () =>
                         {

                         },
                resolveTree = true
            };
            diaNode.options = new List<DiaOption> {accept, reject};

            Find.WindowStack.Add(new Dialog_NodeTree(diaNode, title: this.def.letterLabel));
            Find.Archive.Add(new ArchivedDialog(diaNode.text, this.def.letterLabel));

            return true;
        }

        private static bool TryFindTile(out int tile)
        {
            return TileFinder.TryFindNewSiteTile(tile: out tile, minDist: MinDistance, maxDist: MaxDistance, allowCaravans: true, preferCloserTiles: false);
        }

        private bool TryGetRandomAvailableTargetMap(out Map map)
        {
            return Find.Maps.Where(target => target.IsPlayerHome && RandomNearbyTradeableSettlement(target.Tile) != null).TryRandomElement(result: out map);
        }

        private Settlement RandomNearbyTradeableSettlement(int tile)
        {
            return Find.WorldObjects.SettlementBases.Where(settlement => settlement.Visitable
                && settlement.GetComponent<TradeRequestComp>() != null
                && !settlement.GetComponent<TradeRequestComp>().ActiveRequest
                && Find.WorldGrid.ApproxDistanceInTiles(tile, settlement.Tile) < MaxDistance && Find.WorldReachability.CanReach(tile, settlement.Tile)
            ).RandomElementWithFallback();
        }
    }
}
