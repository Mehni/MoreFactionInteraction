using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver
{
    public class SymbolResolver_BasePart_Indoors_Leaf_Kitchen : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            BaseGen.symbolStack.Push(symbol: "kitchen", resolveParams: rp);
        }
    }

    public class SymbolResolver_Interior_Kitchen : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            InteriorSymbolResolverUtility.PushBedroomHeatersCoolersAndLightSourcesSymbols(rp: rp);
            BaseGen.symbolStack.Push(symbol: "fillWithKitchen", resolveParams: rp);
        }
    }

    public class SymbolResolver_FillWithKitchen : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            ThingDef stoveElectric = DefDatabase<ThingDef>.GetNamedSilentFail(defName: "ElectricStove");
            ThingDef stoveFueled = DefDatabase<ThingDef>.GetNamedSilentFail(defName: "FueledStove");
            ThingDef tableButcher = DefDatabase<ThingDef>.GetNamedSilentFail(defName: "TableButcher");
            ThingDef spotButcher = DefDatabase<ThingDef>.GetNamedSilentFail(defName: "ButcherSpot");

            Map map = BaseGen.globalSettings.map;
            ThingDef thingDef;
            if (rp.singleThingDef != null)
            {
                thingDef = rp.singleThingDef;
            }
            else if (rp.faction != null && rp.faction.def.techLevel >= TechLevel.Medieval)
            {
                thingDef = stoveElectric;
            }
            else
            {
                thingDef = Rand.Element(a: stoveFueled, b: ThingDefOf.Campfire, c: spotButcher);
            }

            bool flipACoin = Rand.Bool;
            foreach (IntVec3 potentialSpot in rp.rect)
            {
                if (flipACoin)
                {
                    if (potentialSpot.x % 3 != 0 || potentialSpot.z % 2 != 0)
                    {
                        continue;
                    }
                }
                else if (potentialSpot.x % 2 != 0 || potentialSpot.z % 3 != 0)
                {
                    continue;
                }
                Rot4 rot = (!flipACoin) ? Rot4.North : Rot4.West;
                if (!GenSpawn.WouldWipeAnythingWith(thingPos: potentialSpot, thingRot: rot, thingDef: thingDef, map: map, predicate: x => x.def.category == ThingCategory.Building))
                {
                    IntVec2 dontTouchMe = new IntVec2(thingDef.Size.x + 1, thingDef.Size.z + 1);
                    if (!BaseGenUtility.AnyDoorAdjacentCardinalTo(rect: GenAdj.OccupiedRect(center: potentialSpot, rot: rot, size: dontTouchMe), map: map))
                    {
                        ResolveParams resolveParams = rp;
                        resolveParams.rect = GenAdj.OccupiedRect(center: potentialSpot, rot: rot, size: thingDef.Size);
                        resolveParams.singleThingDef = (Rand.Element(a: thingDef, b: tableButcher));
                        resolveParams.thingRot = rot;
                        bool? skipSingleThingIfHasToWipeBuildingOrDoesntFit = rp.skipSingleThingIfHasToWipeBuildingOrDoesntFit;
                        resolveParams.skipSingleThingIfHasToWipeBuildingOrDoesntFit = !skipSingleThingIfHasToWipeBuildingOrDoesntFit.HasValue || skipSingleThingIfHasToWipeBuildingOrDoesntFit.Value;
                        BaseGen.symbolStack.Push(symbol: "thing", resolveParams: resolveParams);
                    }
                }
            }
        }
    }
}
