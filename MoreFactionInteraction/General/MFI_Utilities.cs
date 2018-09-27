using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
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

        public static bool IsPartOfFactionWar(this Faction faction) => faction == Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionOne ||
                                                                       faction == Find.World.GetComponent<WorldComponent_MFI_FactionWar>().WarringFactionTwo;

        public static bool TryGetBestArt(Caravan caravan, out Thing thing, out Pawn owner)
        {
            thing = null;
            List<Thing> list = CaravanInventoryUtility.AllInventoryItems(caravan);
            float num = 0f;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].GetInnerIfMinified().GetStatValue(StatDefOf.Beauty) > num)
                    thing = list[i];
            }
            if (thing != null)
            {
                owner = CaravanInventoryUtility.GetOwnerOf(caravan, thing);
                return true;
            }
            owner = null;
            return false;
        }
    }
}
