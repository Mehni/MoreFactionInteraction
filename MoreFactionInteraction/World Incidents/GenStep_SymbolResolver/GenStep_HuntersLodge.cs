using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using RimWorld.Planet;
using RimWorld.BaseGen;

namespace MoreFactionInteraction.World_Incidents
{
    public class GenStep_HuntersLodge : GenStep
    {
        private const int Size = 36;

        private static List<CellRect> possibleRects = new List<CellRect>();

        public override int SeedPart => 735013949;

        public override void Generate(Map map, GenStepParams genStepParams)
        {
            if (!MapGenerator.TryGetVar<CellRect>("RectOfInterest", out CellRect centralPoint))
            {
                centralPoint = CellRect.SingleCell(map.Center);
            }
            Faction faction;
            if (map.ParentFaction == null || map.ParentFaction == Faction.OfPlayer)
            {
                faction = Find.FactionManager.RandomEnemyFaction(false, false, true, TechLevel.Undefined);
            }
            else
            {
                faction = map.ParentFaction;
            }
            ResolveParams resolveParams = default;
            resolveParams.rect = this.GetHuntersLodgeRect(centralPoint, map);
            resolveParams.faction = faction;
            BaseGen.globalSettings.map = map;
            BaseGen.globalSettings.minBuildings = 1;
            BaseGen.globalSettings.minBarracks = 1;
            BaseGen.symbolStack.Push("huntersLodgeBase", resolveParams);
            BaseGen.Generate();
        }

        private CellRect GetHuntersLodgeRect(CellRect centralPoint, Map map)
        {
            GenStep_HuntersLodge.possibleRects.Add(new CellRect(centralPoint.minX - 1 - Size, centralPoint.CenterCell.z - 8, Size, Size));
            GenStep_HuntersLodge.possibleRects.Add(new CellRect(centralPoint.maxX + 1, centralPoint.CenterCell.z - 8, Size, Size));
            GenStep_HuntersLodge.possibleRects.Add(new CellRect(centralPoint.CenterCell.x - 8, centralPoint.minZ - 1 - Size, Size, Size));
            GenStep_HuntersLodge.possibleRects.Add(new CellRect(centralPoint.CenterCell.x - 8, centralPoint.maxZ + 1, Size, Size));
            CellRect mapRect = new CellRect(0, 0, map.Size.x, map.Size.z);
            GenStep_HuntersLodge.possibleRects.RemoveAll((CellRect x) => !x.FullyContainedWithin(mapRect));
            if (GenStep_HuntersLodge.possibleRects.Any<CellRect>())
            {
                return GenStep_HuntersLodge.possibleRects.RandomElement<CellRect>();
            }
            return centralPoint;
        }
    }
}
