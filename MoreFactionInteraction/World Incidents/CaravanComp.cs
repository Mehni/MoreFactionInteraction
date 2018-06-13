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
        public bool caravanIsWorking = false;


        public override string CompInspectStringExtra()
        {
            if (CaravanVisitUtility.SettlementVisitedNow((Caravan)parent)?.GetComponent<SettlementBumperCropComponent>()?.CaravanIsWorking ?? false)
            {
                return "MFI_CaravanWorking".Translate();
            }
            else return string.Empty;
        }

        public override void CompTick()
        {
            if (caravanIsWorking)
            {
                if (Find.TickManager.TicksGame > workWillBeDoneAtTick)
                {
                   CaravanVisitUtility.SettlementVisitedNow((Caravan)parent)?.GetComponent<SettlementBumperCropComponent>().DoOutcome((Caravan)parent);
                }
            }
        }
    }
}
