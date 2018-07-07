using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction.MoreFactionWar
{
    public class CaravanArrivalAction_VisitFactionWarPeaceTalks : CaravanArrivalAction
    {
        private FactionWarPeaceTalks factionWarPeaceTalks;

        public override string Label => "VisitPeaceTalks".Translate(new object[] {this.factionWarPeaceTalks.Label});

        public override string ReportString => "CaravanVisiting".Translate(new object[] { this.factionWarPeaceTalks.Label });

        public override void Arrived(Caravan caravan)
        {
            this.factionWarPeaceTalks.Notify_CaravanArrived(caravan);
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
            Scribe_References.Look(ref this.factionWarPeaceTalks, "factionWarPeaceTalks");
        }

        public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
        {
            if (base.StillValid(caravan, destinationTile))
                return base.StillValid(caravan, destinationTile);

            else if (this.factionWarPeaceTalks?.Tile != destinationTile)
                return false;

            else
                return CanVisit(caravan, this.factionWarPeaceTalks);
        }

        public static FloatMenuAcceptanceReport CanVisit(Caravan caravan, FactionWarPeaceTalks factionWarPeaceTalks)
        {
            return factionWarPeaceTalks != null && factionWarPeaceTalks.Spawned;
        }

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, FactionWarPeaceTalks factionWarPeaceTalks)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions<CaravanArrivalAction_VisitFactionWarPeaceTalks>(() => CaravanArrivalAction_VisitFactionWarPeaceTalks.CanVisit(caravan, factionWarPeaceTalks), () => new CaravanArrivalAction_VisitFactionWarPeaceTalks(factionWarPeaceTalks), "VisitPeaceTalks".Translate(new object[]
            {
                factionWarPeaceTalks.Label
            }), caravan, factionWarPeaceTalks.Tile, factionWarPeaceTalks);
        }
    }
}
