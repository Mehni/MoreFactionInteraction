using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;

namespace MoreFactionInteraction
{
    public class WorldComponent_MFI_FactionWar : WorldComponent
    {
        public WorldComponent_MFI_FactionWar(World world) : base (world: world)
        {
            this.world = world;
        }

        public void StartWar()
        {
            this.WarIsOngoing = true;
        }

        public void ResolveWar()
        {
            this.WarIsOngoing = false;
        }

        public bool WarIsOngoing { get; private set; } = false;
    }
}
