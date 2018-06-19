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

            
            //bottomLeft
            ResolveParams farmBottomLeft = rp;
            farmBottomLeft.rect = new CellRect(rp.rect.minX, rp.rect.minZ, rp.rect.Width / 2 - 1, rp.rect.Height / 2 - 1).ContractedBy(num);
            farmBottomLeft.faction = faction;
            BaseGen.symbolStack.Push("farm", farmBottomLeft);

            //bottomRight
            ResolveParams farmBottomRight = rp;
            farmBottomRight.rect = new CellRect((rp.rect.maxX - rp.rect.Width / 2) - 1, rp.rect.minZ, rp.rect.Width / 2 - 1, rp.rect.Height / 2 - 1).ContractedBy(Rand.Range(3, 6));
            farmBottomRight.faction = faction;
            BaseGen.symbolStack.Push("farm", farmBottomRight);

            ResolveParams farmTopLeft = rp;
            farmTopLeft.rect = new CellRect(rp.rect.minX, (rp.rect.maxZ - rp.rect.Height / 2) - 1, rp.rect.Width / 2 - 1, rp.rect.Height / 2 - 1).ContractedBy(Rand.Range(3,7));
            farmTopLeft.faction = faction;
            BaseGen.symbolStack.Push("farm", farmTopLeft);

            ResolveParams farmTopRight = rp;
            farmTopRight.rect = new CellRect((rp.rect.maxX - rp.rect.Width / 2) - 1, (rp.rect.maxZ - rp.rect.Height / 2) - 1, rp.rect.Width / 2 - 1, rp.rect.Height / 2 - 1).ContractedBy(Rand.Range(1, 4));
            farmTopRight.faction = faction;
            BaseGen.symbolStack.Push("farm", farmTopRight);

            ResolveParams emptyRoom = rp;
            emptyRoom.rect = rp.rect.ContractedBy(Rand.RangeInclusive(10,14));
            emptyRoom.faction = faction;
            BaseGen.symbolStack.Push("emptyRoom", emptyRoom);

        }
    }
}
