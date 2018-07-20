using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace MoreFactionInteraction.MoreFactionWar
{
    using General;
    using JetBrains.Annotations;

    [UsedImplicitly]
    public class IncidentWorker_SettlementBaseAttack : IncidentWorker
    {
        public override float AdjustedChance => base.AdjustedChance;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return base.CanFireNowSub(parms: parms) && Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarIsOngoing;
        }

        //warning: logic ahead.
        //GOAL:
        //1. find random player tile
        //2. find warring faction settlment near it.
        //3. find closest allied (for now: faction = faction) near it
        //4. find closest enemy faction near it.
        //5. If enemy is closer than ally, it's a win for enemy. 
        //6? If enemy is twice as close, base in question becomes enemy base? maaaybe.
        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            int FindTile(int root)
            {
                if (TileFinder.TryFindPassableTileWithTraversalDistance(rootTile: root, minDist: 7, maxDist: 66, result: out int num))
                    return num;

                return -1;
            }
            
            if (!TileFinder.TryFindRandomPlayerTile(tile: out int randomPlayerTile, allowCaravans: true, validator: x => FindTile(root: x) != -1)) return false;

            SettlementBase someRandomPreferablyNearbySettlement = RandomPreferablyNearbySettlementOfFactionInvolvedInWar(originTile: randomPlayerTile);

            if (someRandomPreferablyNearbySettlement == null)
            {
                Find.World.GetComponent<WorldComponent_MFI_FactionWar>().AllOuttaFactionSettlements();
                return false;
            }

            bool HasNearbyAlliedFaction(int x) => Find.WorldObjects.AnySettlementBaseAt(tile: x) &&
                                                    Find.WorldObjects.SettlementBaseAt(tile: x).Faction ==
                                                    someRandomPreferablyNearbySettlement.Faction;

            bool HasNearbyEnemyFaction(int x) => Find.WorldObjects.AnySettlementBaseAt(tile: x) &&
                                                    Find.WorldObjects.SettlementBaseAt(tile: x).Faction ==
                                                    someRandomPreferablyNearbySettlement.Faction.EnemyInFactionWar();

            TileFinder.TryFindPassableTileWithTraversalDistance(rootTile: someRandomPreferablyNearbySettlement.Tile,
                                                                minDist: 0, maxDist: 66,
                                                                result: out int tileContainingAlly,
                                                                validator: HasNearbyAlliedFaction);

            TileFinder.TryFindPassableTileWithTraversalDistance(rootTile: someRandomPreferablyNearbySettlement.Tile,
                                                                minDist: 0, maxDist: 66,
                                                                result: out int tileContainingEnemy,
                                                                validator: HasNearbyEnemyFaction);

            Faction winner = null;
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(value: "MFI_FactionWarBaseAttacked".Translate(someRandomPreferablyNearbySettlement.Faction, someRandomPreferablyNearbySettlement.Faction.EnemyInFactionWar()));
            stringBuilder.AppendLine();
            stringBuilder.AppendLine();

            if (tileContainingEnemy == -1)
            {
                winner = someRandomPreferablyNearbySettlement.Faction;
                stringBuilder.Append(value: "MFI_FactionWarBaseSuccessfullyDefended".Translate(someRandomPreferablyNearbySettlement.Faction, someRandomPreferablyNearbySettlement.Faction.EnemyInFactionWar()));
            }

            //winner is whoever is faster in sending in reinforcements.
            if (tileContainingAlly != -1 && tileContainingEnemy != -1)
            {
                winner =
                    CalcuteTravelTimeForReinforcements(originTile: someRandomPreferablyNearbySettlement.Tile, destinationTile: tileContainingAlly) <
                    CalcuteTravelTimeForReinforcements(originTile: someRandomPreferablyNearbySettlement.Tile, destinationTile: tileContainingEnemy)
                        ? someRandomPreferablyNearbySettlement.Faction
                        : someRandomPreferablyNearbySettlement.Faction.EnemyInFactionWar();

                string flavourText = (winner == someRandomPreferablyNearbySettlement.Faction)
                                         ? "MFI_FactionWarBaseSuccessfullyDefended".Translate(someRandomPreferablyNearbySettlement.Faction, someRandomPreferablyNearbySettlement.Faction.EnemyInFactionWar())
                                         : "MFI_FactionWarBaseDefeated".Translate(someRandomPreferablyNearbySettlement.Faction, someRandomPreferablyNearbySettlement.Faction.EnemyInFactionWar());

                stringBuilder.Append(value: flavourText);
            }

            Find.World.GetComponent<WorldComponent_MFI_FactionWar>().NotifyBattleWon(faction: winner);

            if (Rand.Bool && winner != someRandomPreferablyNearbySettlement.Faction)
            {
                stringBuilder.Append(value: " ");
                stringBuilder.Append(value: DestroyOldOutpostAndCreateNewAtSpot(someRandomPreferablyNearbySettlement: someRandomPreferablyNearbySettlement));
            }

            Find.LetterStack.ReceiveLetter(label: "MFI_FactionWarBaseBattleTookPlaceLabel".Translate(), text: stringBuilder.ToString(), textLetterDef: LetterDefOf.NeutralEvent, lookTargets: new GlobalTargetInfo(tile: someRandomPreferablyNearbySettlement.Tile), relatedFaction: someRandomPreferablyNearbySettlement.Faction);

            return true;
        }

        private static string DestroyOldOutpostAndCreateNewAtSpot(SettlementBase someRandomPreferablyNearbySettlement)
        {
            if (Rand.ChanceSeeded(chance: 0.5f, specialSeed: someRandomPreferablyNearbySettlement.ID))
            {
                Settlement factionBase = (Settlement)WorldObjectMaker.MakeWorldObject(def: WorldObjectDefOf.Settlement);
                factionBase.SetFaction(newFaction: someRandomPreferablyNearbySettlement.Faction.EnemyInFactionWar());
                factionBase.Tile = someRandomPreferablyNearbySettlement.Tile;
                factionBase.Name = SettlementNameGenerator.GenerateSettlementName(factionBase: factionBase);
                Find.WorldObjects.Remove(o: someRandomPreferablyNearbySettlement);
                Find.WorldObjects.Add(o: factionBase);
                return "MFI_FactionWarBaseTakenOver".Translate(someRandomPreferablyNearbySettlement.Faction, someRandomPreferablyNearbySettlement.Faction.EnemyInFactionWar());
            }
            DestroyedSettlement destroyedSettlement = (DestroyedSettlement)WorldObjectMaker.MakeWorldObject(def: WorldObjectDefOf.DestroyedSettlement);
            destroyedSettlement.Tile = someRandomPreferablyNearbySettlement.Tile;
            Find.WorldObjects.Add(o: destroyedSettlement);
            Find.WorldObjects.Remove(o: someRandomPreferablyNearbySettlement);
            return "MFI_FactionWarBaseDestroyed".Translate(someRandomPreferablyNearbySettlement.Faction, someRandomPreferablyNearbySettlement.Faction.EnemyInFactionWar());
        }

        private static SettlementBase RandomPreferablyNearbySettlementOfFactionInvolvedInWar(int originTile)
        {
            Faction factionOne = Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionOne;
            Faction factionTwo = Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionTwo;

            return (from settlement in Find.WorldObjects.SettlementBases
                    where (settlement.Faction == factionOne || settlement.Faction == factionTwo) && Find.WorldGrid.ApproxDistanceInTiles(originTile, settlement.Tile) < 66f
                    select settlement).RandomElementWithFallback(RandomSettlementOfFactionInvolvedInWarThatCanBeABitFurtherAwayIDontParticularlyCare());
        }

        private static SettlementBase RandomSettlementOfFactionInvolvedInWarThatCanBeABitFurtherAwayIDontParticularlyCare()
        {
            Faction factionOne = Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionOne;
            Faction factionTwo = Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionTwo;

            return (from settlement in Find.WorldObjects.SettlementBases
                    where (settlement.Faction == factionOne || settlement.Faction == factionTwo)
                    select settlement).RandomElementWithFallback(null);
        }

        private int CalcuteTravelTimeForReinforcements(int originTile, int destinationTile)
        {
            return CaravanArrivalTimeEstimator.EstimatedTicksToArrive(from: originTile, to: destinationTile, caravan: null);
        }
    }
}
