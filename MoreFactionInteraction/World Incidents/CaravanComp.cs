using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using RimWorld;
using Verse;

namespace MoreFactionInteraction.World_Incidents
{
    public class CaravanComp : WorldObjectComp
    {
        public int workWillBeDoneAtTick;


        public override string CompInspectStringExtra()
        {
            if (CaravanVisitUtility.SettlementVisitedNow((Caravan)parent)?.GetComponent<SettlementBumperCropComponent>()?.CaravanIsWorking ?? false)
            {
                return "Caravan is working";
            }
            else return string.Empty;
        }

        public override void CompTick()
        {
            if (Find.TickManager.TicksGame > workWillBeDoneAtTick)
            {
                if (CaravanVisitUtility.SettlementVisitedNow((Caravan)parent)?.GetComponent<SettlementBumperCropComponent>()?.CaravanIsWorking ?? false)
                {
                    CaravanVisitUtility.SettlementVisitedNow((Caravan)parent)?.GetComponent<SettlementBumperCropComponent>().DoOutcome((Caravan)parent);
                }
            }
        }
    }
}
