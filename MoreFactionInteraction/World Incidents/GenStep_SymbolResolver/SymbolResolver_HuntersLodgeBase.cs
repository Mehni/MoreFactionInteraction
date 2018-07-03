using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace MoreFactionInteraction.World_Incidents
{
    public class SymbolResolver_HuntersLodgeBase : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            Map map = BaseGen.globalSettings.map;
            Faction faction = rp.faction ?? Find.FactionManager.RandomAlliedFaction(false, false, true, TechLevel.Undefined);
            int num = 0;

            if (rp.rect.Width >= 20 && rp.rect.Height >= 20 && (faction.def.techLevel >= TechLevel.Industrial || Rand.Bool))
            {
                num = ((!Rand.Bool) ? 4 : 2);
            }

            float num2 = (float)rp.rect.Area / 144f * 0.17f;
            BaseGen.globalSettings.minEmptyNodes = ((num2 >= 1f) ? GenMath.RoundRandom(num2) : 0);

            BaseGen.symbolStack.Push("outdoorLighting", rp);
            if (faction.def.techLevel >= TechLevel.Industrial)
            {
                int num4 = (!Rand.Chance(0.75f)) ? 0 : GenMath.RoundRandom((float)rp.rect.Area / 400f);
                for (int i = 0; i < num4; i++)
                {
                    ResolveParams resolveParams2 = rp;
                    resolveParams2.faction = faction;
                    BaseGen.symbolStack.Push("firefoamPopper", resolveParams2);
                }
            }

            ResolveParams resolveParams4 = rp;
            resolveParams4.rect = rp.rect.ContractedBy(num);
            resolveParams4.faction = faction;
            BaseGen.symbolStack.Push("ensureCanReachMapEdge", resolveParams4);

            ThingDef mealsource = ThingDefOf.Campfire;

            ResolveParams mealSource = rp;
            mealSource.rect = rp.rect;
            mealSource.singleThingDef = mealsource;
            mealSource.skipSingleThingIfHasToWipeBuildingOrDoesntFit = true;
            BaseGen.symbolStack.Push("thing", mealSource);

            ResolveParams tableButcher = rp;
            tableButcher.rect = rp.rect;
            tableButcher.singleThingDef = //DefDatabase<ThingDef>.GetNamed("TableButcher");
            Rand.Element<ThingDef>(DefDatabase<ThingDef>.GetNamedSilentFail("TableButcher"), DefDatabase<ThingDef>.GetNamedSilentFail("ButcherSpot"));
            BaseGen.symbolStack.Push("thing", tableButcher);

            ResolveParams bigFarmA = rp;
            bigFarmA.faction = faction;
            BaseGen.symbolStack.Push("basePart_outdoors_division", bigFarmA);



            //ResolveParams emptyRoom = rp;
            //emptyRoom.rect = rp.rect.ContractedBy(Rand.RangeInclusive(10,14));
            //emptyRoom.faction = faction;
            //BaseGen.symbolStack.Push("emptyRoom", emptyRoom);
        }
    }
}
