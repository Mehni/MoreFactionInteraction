using RimWorld;
using RimWorld.Planet;
using System.Collections.Generic;
using System.Linq;
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
            foreach (Thing current in list)
            {
                if (current.GetInnerIfMinified().GetStatValue(StatDefOf.Beauty) > num && (current.GetInnerIfMinified().TryGetComp<CompArt>()?.Props?.canBeEnjoyedAsArt ?? false))
                    thing = current;
            }
            if (thing != null)
            {
                owner = CaravanInventoryUtility.GetOwnerOf(caravan, thing);
                return true;
            }
            owner = null;
            return false;
        }

        public static bool CaravanOrRichestColonyHasAnyOf(ThingDef thingdef, Caravan caravan, out Thing thing)
        {
            if (CaravanInventoryUtility.TryGetThingOfDef(caravan, thingdef, out thing, out Pawn owner))
                return true;

            List<Map> maps = Find.Maps.FindAll(x => x.IsPlayerHome);

            if (maps.NullOrEmpty())
                return false;

            maps.SortBy(x => x.PlayerWealthForStoryteller);
            Map richestMap = maps.First();

            if (thingdef.IsBuildingArtificial)
            {
                return FindBuildingOrMinifiedVersionThereOf(thingdef, richestMap, out thing);
            }
            var thingsOfDef = richestMap.listerThings.ThingsOfDef(thingdef);

            thing = thingsOfDef.FirstOrDefault();
            return thingsOfDef.Any();
        }

        public static bool FindBuildingOrMinifiedVersionThereOf(ThingDef thingdef, Map map, out Thing thing)
        {
            IEnumerable<Building> buildingsOfDef = map.listerBuildings.AllBuildingsColonistOfDef(thingdef);
            if (buildingsOfDef.Any())
            {
                thing = buildingsOfDef.First();
                return true;
            }
            var minifiedBuilds = map.listerThings.ThingsInGroup(ThingRequestGroup.MinifiedThing);
            for (int i = 0; i < minifiedBuilds.Count; i++)
            {
                if (minifiedBuilds[i].GetInnerIfMinified().def == thingdef)
                {
                    thing = minifiedBuilds[i];
                    return true;
                }
            }
            thing = null;
            return false;
        }
    }
}
