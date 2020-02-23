using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.BaseGen;
using UnityEngine;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver
{
    // yeah this is mostly taken from vanilla.
    class MFI_SymbolResolver_BasePart_Outdoors_Division_Split : SymbolResolver
    {
        private const int MinLengthAfterSplit = 5;

        private static readonly IntRange SpaceBetweenRange = new IntRange(1, 2);

        public override bool CanResolve(ResolveParams rp)
        {
            return base.CanResolve(rp) && (this.TryFindSplitPoint(false, rp.rect, out _, out _) 
                                        || this.TryFindSplitPoint(true, rp.rect, out _, out _));
        }

        public override void Resolve(ResolveParams rp)
        {
            bool coinFlip = Rand.Bool;
            bool flipACoin;
            if (this.TryFindSplitPoint(coinFlip, rp.rect, out int splitPoint, out int spaceBetween))
            {
                flipACoin = coinFlip;
            }
            else
            {
                if (!this.TryFindSplitPoint(!coinFlip, rp.rect, out splitPoint, out spaceBetween))
                {
                    Log.Warning("Could not find split point.", false);
                    return;
                }
                flipACoin = !coinFlip;
            }
            TerrainDef floorDef = rp.pathwayFloorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction, false);
            ResolveParams resolveVariantA;
            ResolveParams resolveVariantB;
            if (flipACoin)
            {
                ResolveParams resolveParams = rp;
                resolveParams.rect = new CellRect(rp.rect.minX, rp.rect.minZ + splitPoint, rp.rect.Width, spaceBetween);
                resolveParams.floorDef = floorDef;
                resolveParams.streetHorizontal = new bool?(true);
                BaseGen.symbolStack.Push("street", resolveParams);
                ResolveParams resolveParams2 = rp;
                resolveParams2.rect = new CellRect(rp.rect.minX, rp.rect.minZ, rp.rect.Width, splitPoint);
                resolveVariantA = resolveParams2;
                ResolveParams resolveParams4 = rp;
                resolveParams4.rect = new CellRect(rp.rect.minX, rp.rect.minZ + splitPoint + spaceBetween, rp.rect.Width, rp.rect.Height - splitPoint - spaceBetween);
                resolveVariantB = resolveParams4;
            }
            else
            {
                ResolveParams resolveParams6 = rp;
                resolveParams6.rect = new CellRect(rp.rect.minX + splitPoint, rp.rect.minZ, spaceBetween, rp.rect.Height);
                resolveParams6.floorDef = floorDef;
                resolveParams6.streetHorizontal = new bool?(false);
                BaseGen.symbolStack.Push("street", resolveParams6);
                ResolveParams resolveParams7 = rp;
                resolveParams7.rect = new CellRect(rp.rect.minX, rp.rect.minZ, splitPoint, rp.rect.Height);
                resolveVariantA = resolveParams7;
                ResolveParams resolveParams8 = rp;
                resolveParams8.rect = new CellRect(rp.rect.minX + splitPoint + spaceBetween, rp.rect.minZ, rp.rect.Width - splitPoint - spaceBetween, rp.rect.Height);
                resolveVariantB = resolveParams8;
            }
            if (Rand.Bool)
            {
                BaseGen.symbolStack.Push("MFI_basePart_outdoors", resolveVariantA);
                BaseGen.symbolStack.Push("MFI_basePart_outdoors", resolveVariantB);
            }
            else
            {
                BaseGen.symbolStack.Push("MFI_basePart_outdoors", resolveVariantB);
                BaseGen.symbolStack.Push("MFI_basePart_outdoors", resolveVariantA);
            }
        }

        private bool TryFindSplitPoint(bool horizontal, CellRect rect, out int splitPoint, out int spaceBetween)
        {
            int num = (!horizontal) ? rect.Width : rect.Height;
            spaceBetween = MFI_SymbolResolver_BasePart_Outdoors_Division_Split.SpaceBetweenRange.RandomInRange;
            spaceBetween = Mathf.Min(spaceBetween, num - 10);
            if (spaceBetween < MFI_SymbolResolver_BasePart_Outdoors_Division_Split.SpaceBetweenRange.min)
            {
                splitPoint = -1;
                return false;
            }
            splitPoint = Rand.RangeInclusive(MinLengthAfterSplit, num - MinLengthAfterSplit - spaceBetween);
            return true;
        }
    }
}
