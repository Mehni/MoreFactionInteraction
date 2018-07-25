using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld.BaseGen;
using Verse;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver
{
    class MFI_SymbolResolver_BasePart_Outdoors : SymbolResolver
    {
        public override void Resolve(ResolveParams rp)
        {
            bool doneresolving = rp.rect.Width > 23 || rp.rect.Height > 23 || ((rp.rect.Width >= 11 || rp.rect.Height >= 11) && Rand.Bool);
            ResolveParams resolveParams = rp;
            resolveParams.pathwayFloorDef = (rp.pathwayFloorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction));

            BaseGen.symbolStack.Push(doneresolving ? "MFI_basePart_outdoors_division" : "MFI_basePart_outdoors_leafPossiblyDecorated", resolveParams);
        }
    }
}
