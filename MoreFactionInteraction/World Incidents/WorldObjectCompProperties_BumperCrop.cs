using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using System.Text;
using RimWorld;

namespace MoreFactionInteraction.World_Incidents
{
    class WorldObjectCompProperties_BumperCrop : WorldObjectCompProperties
    {
        public WorldObjectCompProperties_BumperCrop()
        {
            this.compClass = typeof(SettlementBumperCropComponent);
        }
    }
}
