using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.BaseGen;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver
{

    class MFI_SymbolResolver_BasePart_Outdoors_Leaf_Farm : SymbolResolver
    {
        private const float MaxCoverage = 0.75f;

        public override bool CanResolve(ResolveParams rp)
        {
            return base.CanResolve(rp) && BaseGen.globalSettings.basePart_buildingsResolved >= BaseGen.globalSettings.minBuildings 
                                       && BaseGen.globalSettings.basePart_emptyNodesResolved >= BaseGen.globalSettings.minEmptyNodes 
                                       && BaseGen.globalSettings.basePart_farmsCoverage + (float)rp.rect.Area / (float)BaseGen.globalSettings.mainRect.Area < MaxCoverage
                                       && (rp.rect.Width <= 15 && rp.rect.Height <= 15) 
                                       && (rp.cultivatedPlantDef != null || SymbolResolver_CultivatedPlants.DeterminePlantDef(rp.rect) != null);
        }

        public override void Resolve(ResolveParams rp)
        {
            BaseGen.symbolStack.PushMany(rp, "farm", "farm", "farm");
            BaseGen.globalSettings.basePart_farmsCoverage += (float)rp.rect.Area / (float)BaseGen.globalSettings.mainRect.Area;
        }
    }

    class MFI_SymbolResolver_BasePart_Outdoors_Leaf_Building : SymbolResolver
    {
        public override bool CanResolve(ResolveParams rp)
        {
            return base.CanResolve(rp) && (BaseGen.globalSettings.basePart_emptyNodesResolved >= BaseGen.globalSettings.minEmptyNodes 
                                        || BaseGen.globalSettings.basePart_buildingsResolved < BaseGen.globalSettings.minBuildings);
        }

        public override void Resolve(ResolveParams rp)
        {
            ResolveParams resolveParams = rp;
            resolveParams.wallStuff = (rp.wallStuff ?? BaseGenUtility.RandomCheapWallStuff(rp.faction, false));
            resolveParams.floorDef  = (rp.floorDef  ?? BaseGenUtility.RandomBasicFloorDef(rp.faction, true));
            BaseGen.symbolStack.Push("MFI_basePart_indoors", resolveParams);
            BaseGen.globalSettings.basePart_buildingsResolved++;
        }
    }
}
