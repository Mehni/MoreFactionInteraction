using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver
{
    public class SymbolResolver_BasePart_Indoors_Leaf_Kitchen : SymbolResolver
    {
        public override bool CanResolve(ResolveParams rp)
        {
            return base.CanResolve(rp);
        }

        public override void Resolve(ResolveParams rp)
        {
            BaseGen.symbolStack.Push("kitchen", rp);
        }
    }

    public class SymbolResolver_Interior_Kitchen : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            InteriorSymbolResolverUtility.PushBedroomHeatersCoolersAndLightSourcesSymbols(rp, true);
            BaseGen.symbolStack.Push("fillWithKitchen", rp);
        }
    }

    public class SymbolResolver_FillWithKitchen : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            ThingDef stoveElectric = DefDatabase<ThingDef>.GetNamedSilentFail("ElectricStove");
            ThingDef stoveFueled = DefDatabase<ThingDef>.GetNamedSilentFail("FueledStove");
            ThingDef tableButcher = DefDatabase<ThingDef>.GetNamedSilentFail("TableButcher");
            ThingDef spotButcher = DefDatabase<ThingDef>.GetNamedSilentFail("ButcherSpot");

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
                thingDef = Rand.Element<ThingDef>(stoveFueled, ThingDefOf.Campfire, spotButcher);
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
                if (!GenSpawn.WouldWipeAnythingWith(potentialSpot, rot, thingDef, map, (Thing x) => x.def.category == ThingCategory.Building))
                {
                    if (!BaseGenUtility.AnyDoorAdjacentCardinalTo(GenAdj.OccupiedRect(potentialSpot, rot, thingDef.Size), map))
                    {
                        ResolveParams resolveParams = rp;
                        resolveParams.rect = GenAdj.OccupiedRect(potentialSpot, rot, thingDef.size);
                        resolveParams.singleThingDef = (Rand.Element(thingDef, tableButcher));
                        resolveParams.thingRot = new Rot4?(rot);
                        bool? skipSingleThingIfHasToWipeBuildingOrDoesntFit = rp.skipSingleThingIfHasToWipeBuildingOrDoesntFit;
                        resolveParams.skipSingleThingIfHasToWipeBuildingOrDoesntFit = new bool?(!skipSingleThingIfHasToWipeBuildingOrDoesntFit.HasValue || skipSingleThingIfHasToWipeBuildingOrDoesntFit.Value);
                        BaseGen.symbolStack.Push("thing", resolveParams);
                    }
                }
            }
        }
    }

}
