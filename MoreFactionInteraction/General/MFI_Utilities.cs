using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace MoreFactionInteraction.General
{
    //I'm fancy, I wrote an extension method.
    public static class MFI_Utilities
    {
        public static Faction EnemyInFactionWar(this Faction faction)
        {
            if (faction == Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionOne)
                return Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionTwo;

            if (faction == Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionTwo)
                return Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionOne;

            return null;

            //Dear reader: Resharper suggests the following:
            //
            //return faction == Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionOne
            //      ? Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionTwo
            //      : (faction == Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionTwo
            //          ? Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionOne
            //          : null);
            //
            // which is a nested ternary and just awful to read. Be happy I spared you.
        }
    }
}
