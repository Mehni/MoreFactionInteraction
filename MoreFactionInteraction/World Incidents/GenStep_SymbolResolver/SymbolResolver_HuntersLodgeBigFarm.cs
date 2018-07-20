using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace MoreFactionInteraction.World_Incidents
{
    public class SymbolResolver_HuntersLodgeBigFarm : SymbolResolver
    {
        public override bool CanResolve(ResolveParams rp)
        {
            return base.CanResolve(rp: rp);
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
            farmBottomLeft.rect = new CellRect(minX: rp.rect.minX, minZ: rp.rect.minZ, width: rp.rect.Width / 2 - 1, height: rp.rect.Height / 2 - 1).ContractedBy(dist: num);
            BaseGen.symbolStack.Push(symbol: "farm", resolveParams: farmBottomLeft);

            //bottomRight
            ResolveParams farmBottomRight = rp;
            farmBottomRight.rect = new CellRect(minX: (rp.rect.maxX - rp.rect.Width / 2) - 1, minZ: rp.rect.minZ, width: rp.rect.Width / 2 - 1, height: rp.rect.Height / 2 - 1).ContractedBy(dist: Rand.Range(min: 3, max: 6));
            BaseGen.symbolStack.Push(symbol: "farm", resolveParams: farmBottomRight);

            ResolveParams farmTopLeft = rp;
            farmTopLeft.rect = new CellRect(minX: rp.rect.minX, minZ: (rp.rect.maxZ - rp.rect.Height / 2) - 1, width: rp.rect.Width / 2 - 1, height: rp.rect.Height / 2 - 1).ContractedBy(dist: Rand.Range(min: 3, max: 7));
            BaseGen.symbolStack.Push(symbol: "farm", resolveParams: farmTopLeft);

            ResolveParams farmTopRight = rp;
            farmTopRight.rect = new CellRect(minX: (rp.rect.maxX - rp.rect.Width / 2) - 1, minZ: (rp.rect.maxZ - rp.rect.Height / 2) - 1, width: rp.rect.Width / 2 - 1, height: rp.rect.Height / 2 - 1).ContractedBy(dist: Rand.Range(min: 1, max: 4));
            BaseGen.symbolStack.Push(symbol: "farm", resolveParams: farmTopRight);
        }
    }
}
