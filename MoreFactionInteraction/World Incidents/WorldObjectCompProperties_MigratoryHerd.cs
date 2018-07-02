using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;

namespace MoreFactionInteraction.World_Incidents
{
    class WorldObjectCompProperties_MigratoryHerd : WorldObjectCompProperties
    {
        public WorldObjectCompProperties_MigratoryHerd()
        {
            this.compClass = typeof(WorldObjectComp_MigratoryHerdComp);
        }
    }
}
