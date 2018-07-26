using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction.MoreFactionWar
{
    public class CaravanArrivalAction_VisitFactionWarPeaceTalks : CaravanArrivalAction
    {
        private FactionWarPeaceTalks factionWarPeaceTalks;

        public override string Label => "VisitPeaceTalks".Translate( factionWarPeaceTalks.Label );

        public override string ReportString => "CaravanVisiting".Translate( this.factionWarPeaceTalks.Label );

        public override void Arrived(Caravan caravan)
        {
            this.factionWarPeaceTalks.Notify_CaravanArrived(caravan: caravan);
        }

        public CaravanArrivalAction_VisitFactionWarPeaceTalks()
        {
        }

        public CaravanArrivalAction_VisitFactionWarPeaceTalks(FactionWarPeaceTalks factionWarPeaceTalks)
        {
            this.factionWarPeaceTalks = factionWarPeaceTalks;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(refee: ref this.factionWarPeaceTalks, label: "factionWarPeaceTalks");
        }

        public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
        {
            if (base.StillValid(caravan: caravan, destinationTile: destinationTile))
                return base.StillValid(caravan: caravan, destinationTile: destinationTile);

            if (this.factionWarPeaceTalks?.Tile != destinationTile)
                return false;
            
            return CanVisit(caravan: caravan, factionWarPeaceTalks: this.factionWarPeaceTalks);
        }

        public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, FactionWarPeaceTalks factionWarPeaceTalks)
        {
            return factionWarPeaceTalks != null && factionWarPeaceTalks.Spawned;
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, FactionWarPeaceTalks factionWarPeaceTalks)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions(acceptanceReportGetter: 
                                                                   () => CanVisit(caravan: caravan, factionWarPeaceTalks: factionWarPeaceTalks), arrivalActionGetter: 
                                                                   () => new CaravanArrivalAction_VisitFactionWarPeaceTalks(factionWarPeaceTalks: factionWarPeaceTalks), 
                                                                   label: "VisitPeaceTalks".Translate(factionWarPeaceTalks.Label), 
                                                                   caravan: caravan, pathDestination: factionWarPeaceTalks.Tile, 
                                                                   revalidateWorldClickTarget: factionWarPeaceTalks);
        }
    }
}
