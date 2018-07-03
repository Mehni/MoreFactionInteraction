using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace MoreFactionInteraction.World_Incidents
{
    public class SymbolResolver_HuntersLodgeBigFarm : SymbolResolver
    {
        public override bool CanResolve(ResolveParams rp)
        {
            Log.Message("can resolve bigFarmA " + base.CanResolve(rp));

            return base.CanResolve(rp);
        }

        public override void Resolve(ResolveParams rp)
        {
            int num = 0;
            if (rp.rect.Width >= 20 && rp.rect.Height >= 20 && (rp.faction.def.techLevel >= TechLevel.Industrial || Rand.Bool))
            {
                num = ((!Rand.Bool) ? 4 : 2);
            }
            //bottomLeft
            ResolveParams farmBottomLeft = rp;
            farmBottomLeft.rect = new CellRect(rp.rect.minX, rp.rect.minZ, rp.rect.Width / 2 - 1, rp.rect.Height / 2 - 1).ContractedBy(num);
            BaseGen.symbolStack.Push("farm", farmBottomLeft);

            //bottomRight
            ResolveParams farmBottomRight = rp;
            farmBottomRight.rect = new CellRect((rp.rect.maxX - rp.rect.Width / 2) - 1, rp.rect.minZ, rp.rect.Width / 2 - 1, rp.rect.Height / 2 - 1).ContractedBy(Rand.Range(3, 6));
            BaseGen.symbolStack.Push("farm", farmBottomRight);

            ResolveParams farmTopLeft = rp;
            farmTopLeft.rect = new CellRect(rp.rect.minX, (rp.rect.maxZ - rp.rect.Height / 2) - 1, rp.rect.Width / 2 - 1, rp.rect.Height / 2 - 1).ContractedBy(Rand.Range(3, 7));
            BaseGen.symbolStack.Push("farm", farmTopLeft);

            ResolveParams farmTopRight = rp;
            farmTopRight.rect = new CellRect((rp.rect.maxX - rp.rect.Width / 2) - 1, (rp.rect.maxZ - rp.rect.Height / 2) - 1, rp.rect.Width / 2 - 1, rp.rect.Height / 2 - 1).ContractedBy(Rand.Range(1, 4));
            BaseGen.symbolStack.Push("farm", farmTopRight);
        }
    }
}
