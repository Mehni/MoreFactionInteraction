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
            return base.StillValid(caravan, destinationTile);
        }
    }
}
