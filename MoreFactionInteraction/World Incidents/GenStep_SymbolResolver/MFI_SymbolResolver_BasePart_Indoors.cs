using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld.BaseGen;
using UnityEngine;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver
{
    using RimWorld;

    class MFI_SymbolResolver_BasePart_Indoors : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            bool flag = rp.rect.Width > 13 || rp.rect.Height > 13 || ((rp.rect.Width >= 9 || rp.rect.Height >= 9) && Rand.Chance(0.3f));
            if (flag)
            {
                BaseGen.symbolStack.Push("MFI_basePart_indoors_division", rp);
            }
            else
            {
                BaseGen.symbolStack.Push("MFI_basePart_indoors_leaf", rp);
            }
        }
    }

    class MFI_SymbolResolver_BasePart_Indoors_Division_Split : SymbolResolver
    {
        private const int MinLengthAfterSplit = 5;

        private const int MinWidthOrHeight = 9;

        public override bool CanResolve(ResolveParams rp)
        {
            return base.CanResolve(rp) && (rp.rect.Width >= MinWidthOrHeight || rp.rect.Height >= MinWidthOrHeight);
        }

        public override void Resolve(ResolveParams rp)
        {
            if (rp.rect.Width < MinWidthOrHeight && rp.rect.Height < MinWidthOrHeight)
            {
                Log.Warning("Too small rect. params=" + rp, false);
            }
            else
            {
                bool flag = (Rand.Bool && rp.rect.Height >= MinWidthOrHeight) || rp.rect.Width < MinWidthOrHeight;
                if (flag)
                {
                    int           num           = Rand.RangeInclusive(4, rp.rect.Height - MinLengthAfterSplit);
                    ResolveParams resolveParams = rp;
                    resolveParams.rect = new CellRect(rp.rect.minX, rp.rect.minZ, rp.rect.Width, num + 1);
                    BaseGen.symbolStack.Push("MFI_basePart_indoors", resolveParams);
                    ResolveParams resolveParams2 = rp;
                    resolveParams2.rect =
                        new CellRect(rp.rect.minX, rp.rect.minZ + num, rp.rect.Width, rp.rect.Height - num);
                    BaseGen.symbolStack.Push("MFI_basePart_indoors", resolveParams2);
                }
                else
                {
                    int           num2           = Rand.RangeInclusive(4, rp.rect.Width - MinLengthAfterSplit);
                    ResolveParams resolveParams3 = rp;
                    resolveParams3.rect = new CellRect(rp.rect.minX, rp.rect.minZ, num2 + 1, rp.rect.Height);
                    BaseGen.symbolStack.Push("MFI_basePart_indoors", resolveParams3);
                    ResolveParams resolveParams4 = rp;
                    resolveParams4.rect =
                        new CellRect(rp.rect.minX + num2, rp.rect.minZ, rp.rect.Width - num2, rp.rect.Height);
                    BaseGen.symbolStack.Push("MFI_basePart_indoors", resolveParams4);
                }
            }
        }
    }

    class MFI_SymbolResolver_BasePart_Indoors_Leaf_GloriousPotat : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            CellRect rect          = new CellRect(rp.rect.maxX - 3, rp.rect.maxZ - 3, 4, 4);
            ThingDef gloriousPotat = ThingDefOf.RawPotatoes;
            int      num           = Rand.RangeInclusive(2, 3);
            for (int i = 0; i < num; i++)
            {
                ResolveParams resolveParams = rp;
                resolveParams.rect                  = rect.ContractedBy(1);
                resolveParams.singleThingDef        = gloriousPotat;
                resolveParams.singleThingStackCount = Rand.RangeInclusive(Mathf.Min(10, gloriousPotat.stackLimit), Mathf.Min(50, gloriousPotat.stackLimit));
                BaseGen.symbolStack.Push("thing", resolveParams);
            }
            ResolveParams resolveParams2 = rp;
            resolveParams2.rect = rect;
            BaseGen.symbolStack.Push("ensureCanReachMapEdge", resolveParams2);
            ResolveParams resolveParams3 = rp;
            resolveParams3.rect = rect;
            BaseGen.symbolStack.Push("emptyRoom", resolveParams3);
        }
    }
}
