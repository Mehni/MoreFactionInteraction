using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.BaseGen;
using Verse;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver
{
    class MFI_SymbolResolver_BasePart_Outdoors_LeafPossiblyDecorated : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            if (rp.rect.Width >= 10 && rp.rect.Height >= 10 && Rand.Chance(chance: 0.25f))
            {
                BaseGen.symbolStack.Push(symbol: "MFI_basePart_outdoors_leafDecorated", resolveParams: rp);
            }
            else
            {
                BaseGen.symbolStack.Push(symbol: "MFI_basePart_outdoors_leaf", resolveParams: rp);
            }
        }
    }

    class MFI_SymbolResolver_BasePart_Outdoors_LeafDecorated_EdgeStreet : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            ResolveParams resolveParams = rp;
            resolveParams.floorDef = (rp.pathwayFloorDef ?? BaseGenUtility.RandomBasicFloorDef(faction: rp.faction, allowCarpet: false));
            BaseGen.symbolStack.Push(symbol: "edgeStreet", resolveParams: resolveParams);
            ResolveParams resolveParams2 = rp;
            resolveParams2.rect = rp.rect.ContractedBy(dist: 1);
            BaseGen.symbolStack.Push(symbol: "MFI_basePart_outdoors_leaf", resolveParams: resolveParams2);
        }
    }

    class MFI_SymbolResolver_BasePart_Outdoors_LeafDecorated_RandomInnerRect : SymbolResolver
    {
        private const int MinLength = 5;

        private const int MaxRectSize = 15;

        public override bool CanResolve(ResolveParams rp)
        {
            return base.CanResolve(rp: rp) && rp.rect.Width <= MaxRectSize 
                                       && rp.rect.Height <= MaxRectSize 
                                       && rp.rect.Width > MinLength 
                                       && rp.rect.Height > MinLength;
        }

        public override void Resolve(ResolveParams rp)
        {
            int           num           = Rand.RangeInclusive(min: MinLength, max: rp.rect.Width  - 1);
            int           num2          = Rand.RangeInclusive(min: MinLength, max: rp.rect.Height - 1);
            int           num3          = Rand.RangeInclusive(min: 0, max: rp.rect.Width  - num);
            int           num4          = Rand.RangeInclusive(min: 0, max: rp.rect.Height - num2);
            ResolveParams resolveParams = rp;
            resolveParams.rect = new CellRect(minX: rp.rect.minX + num3, minZ: rp.rect.minZ + num4, width: num, height: num2);
            BaseGen.symbolStack.Push(symbol: "MFI_basePart_outdoors_leaf", resolveParams: resolveParams);
        }
    }
}
