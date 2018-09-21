using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace MoreFactionInteraction.More_Flavour
{
    class CaravanArrivalAction_VisitAnnualExpo : CaravanArrivalAction
    {
        private AnnualExpo annualExpo;

        public CaravanArrivalAction_VisitAnnualExpo() { }

        public CaravanArrivalAction_VisitAnnualExpo(AnnualExpo annualExpo)
        {
            this.annualExpo = annualExpo;
        }

        public override string Label => "VisitPeaceTalks".Translate(annualExpo.Label);
        public override string ReportString => "CaravanVisiting".Translate(this.annualExpo.Label);

        public static IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan, AnnualExpo annualExpo)
        {
            return CaravanArrivalActionUtility.GetFloatMenuOptions(acceptanceReportGetter: () => CanVisit(caravan, annualExpo),
                                                                    arrivalActionGetter: () => new CaravanArrivalAction_VisitAnnualExpo(annualExpo),
                                                                    label: "VisitPeaceTalks".Translate(annualExpo.Label),
                                                                    caravan: caravan, pathDestination: annualExpo.Tile,
                                                                    revalidateWorldClickTarget: annualExpo);
        }

        private static FloatMenuAcceptanceReport CanVisit(Caravan caravan, AnnualExpo annualExpo)
        {
            return annualExpo != null && annualExpo.Spawned;
        }

        public override void Arrived(Caravan caravan)
        {
            this.annualExpo.Notify_CaravanArrived(caravan);
        }

        public override FloatMenuAcceptanceReport StillValid(Caravan caravan, int destinationTile)
        {
            return base.StillValid(caravan, destinationTile);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(refee: ref this.annualExpo, label: "MFI_annualExpo");
        }
    }
}
