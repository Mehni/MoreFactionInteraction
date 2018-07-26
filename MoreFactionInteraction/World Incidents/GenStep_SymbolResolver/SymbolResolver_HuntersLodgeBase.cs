using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace MoreFactionInteraction.World_Incidents
{
    public class SymbolResolver_HuntersLodgeBase : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            //Map map = BaseGen.globalSettings.map;
            Faction faction = rp.faction ?? Find.FactionManager.RandomAlliedFaction();
            int num = 0;

            if (rp.rect.Width >= 20 && rp.rect.Height >= 20 && (faction.def.techLevel >= TechLevel.Industrial || Rand.Bool))
            {
                num = ((!Rand.Bool) ? 4 : 2);
            }

            float num2 = (float)rp.rect.Area / 144f * 0.17f;
            BaseGen.globalSettings.minEmptyNodes = ((num2 >= 1f) ? GenMath.RoundRandom(f: num2) : 0);

            BaseGen.symbolStack.Push(symbol: "outdoorLighting", resolveParams: rp);
            if (faction.def.techLevel >= TechLevel.Industrial)
            {
                int num4 = (!Rand.Chance(chance: 0.75f)) ? 0 : GenMath.RoundRandom(f: (float)rp.rect.Area / 400f);
                for (int i = 0; i < num4; i++)
                {
                    ResolveParams resolveParams2 = rp;
                    resolveParams2.faction = faction;
                    BaseGen.symbolStack.Push(symbol: "firefoamPopper", resolveParams: resolveParams2);
                }
            }
            ResolveParams resolveParams4 = rp;
            resolveParams4.rect = rp.rect.ContractedBy(dist: num);
            resolveParams4.faction = faction;
            BaseGen.symbolStack.Push(symbol: "ensureCanReachMapEdge", resolveParams: resolveParams4);

            ResolveParams mainBasePart = rp;
            ThingSetMakerParams thingSet = default;
            thingSet.totalMarketValueRange = new FloatRange(300, 1000);
            thingSet.filter = new ThingFilter();
            thingSet.filter.SetAllow(ThingCategoryDefOf.PlantFoodRaw, true);
            rp.thingSetMakerParams = thingSet;
            mainBasePart.faction = faction;
            BaseGen.symbolStack.Push(symbol: "MFI_basePart_outdoors_division", resolveParams: mainBasePart);
        }
    }
}
