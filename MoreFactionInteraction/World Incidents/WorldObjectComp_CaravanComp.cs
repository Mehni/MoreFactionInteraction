using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.Planet;
using RimWorld;
using Verse;

namespace MoreFactionInteraction.World_Incidents
{
    public class WorldObjectComp_CaravanComp : WorldObjectComp
    {
        public int workWillBeDoneAtTick;
        public bool caravanIsWorking = false;


        public override string CompInspectStringExtra()
        {
            if (CaravanVisitUtility.SettlementVisitedNow((Caravan)parent)?.GetComponent<WorldObjectComp_SettlementBumperCropComp>()?.CaravanIsWorking ?? false)
            {
                return "MFI_CaravanWorking".Translate();
            }
            else return string.Empty;
        }

        public override void CompTick()
        {
            if (caravanIsWorking && Find.TickManager.TicksGame > workWillBeDoneAtTick)
            {
                CaravanVisitUtility.SettlementVisitedNow((Caravan)parent)?.GetComponent<WorldObjectComp_SettlementBumperCropComp>().DoOutcome((Caravan)parent);
            }
        }
    }
}
