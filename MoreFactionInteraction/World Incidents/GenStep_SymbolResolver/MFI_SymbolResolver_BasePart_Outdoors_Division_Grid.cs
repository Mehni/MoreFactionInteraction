using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld.BaseGen;
using RimWorld;

namespace MoreFactionInteraction.World_Incidents.GenStep_SymbolResolver
{
    //magic numbers. Magic numbers everywhere ;-;
    class MFI_SymbolResolver_BasePart_Outdoors_Division_Grid : SymbolResolver
    {
        private class Child
        {
            public CellRect rect;

            public int gridX;

            public int gridY;

            public bool merged;
        }

        private readonly List<Pair<int, int>> optionsX = new List<Pair<int, int>>();

        private readonly List<Pair<int, int>> optionsZ = new List<Pair<int, int>>();

        private readonly List<MFI_SymbolResolver_BasePart_Outdoors_Division_Grid.Child> children =
            new List<MFI_SymbolResolver_BasePart_Outdoors_Division_Grid.Child>();

        private const int MinWidthOrHeight = 13;

        private const int MinRoomsPerRow = 2;

        private const int MaxRoomsPerRow = 4;

        private const int MaxPathwayWidth = 5;

        private const int MinRoomSize = 6;

        private const float AllowNonSquareRoomsInTheFirstStepChance = 0.2f;

        private static readonly List<Pair<Pair<int, int>, Pair<int, int>>> options =
            new List<Pair<Pair<int, int>, Pair<int, int>>>();

        public override bool CanResolve(ResolveParams rp)
        {
            bool result;
            if (!base.CanResolve(rp))
            {
                result = false;
            }
            else if (rp.rect.Width < MinWidthOrHeight && rp.rect.Height < MinWidthOrHeight)
            {
                result = false;
            }
            else
            {
                this.FillOptions(rp.rect);
                result = (this.optionsX.Any<Pair<int, int>>() && this.optionsZ.Any<Pair<int, int>>());
            }

            return result;
        }

        public override void Resolve(ResolveParams rp)
        {
            this.FillOptions(rp.rect);
            if (!Rand.Chance(AllowNonSquareRoomsInTheFirstStepChance))
            {
                if (this.TryResolveRandomOption(0, 0, rp))
                {
                    return;
                }

                if (this.TryResolveRandomOption(0, 1, rp))
                {
                    return;
                }
            }

            if (!this.TryResolveRandomOption(1, 0, rp))
            {
                if (!this.TryResolveRandomOption(2, 0, rp))
                {
                    if (!this.TryResolveRandomOption(2, 1, rp))
                    {
                        if (!this.TryResolveRandomOption(999999, 999999, rp))
                        {
                            Log.Warning("Grid resolver could not resolve any grid size. params=" + rp, false);
                        }
                    }
                }
            }
        }

        private void FillOptions(CellRect rect)
        {
            this.FillOptions(this.optionsX, rect.Width);
            this.FillOptions(this.optionsZ, rect.Height);
            if (this.optionsZ.Any((Pair<int, int> x) => x.First > 1))
            {
                this.optionsX.RemoveAll((Pair<int, int> x) =>
                                            x.First >= 3 && this.GetRoomSize(x.First, x.Second, rect.Width) <= 7);
            }

            if (this.optionsX.Any((Pair<int, int> x) => x.First > 1))
            {
                this.optionsZ.RemoveAll((Pair<int, int> x) =>
                                            x.First >= 3 && this.GetRoomSize(x.First, x.Second, rect.Height) <= 7);
            }
        }

        private void FillOptions(List<Pair<int, int>> outOptions, int length)
        {
            outOptions.Clear();
            for (int i = MinRoomsPerRow; i <= MaxRoomsPerRow; i++)
            {
                for (int j = 1; j <= MaxPathwayWidth; j++)
                {
                    int roomSize = this.GetRoomSize(i, j, length);
                    if (roomSize != -1 && roomSize >= MinRoomSize && roomSize >= MinRoomsPerRow * j - 1)
                    {
                        outOptions.Add(new Pair<int, int>(i, j));
                    }
                }
            }
        }

        private int GetRoomSize(int roomsPerRow, int pathwayWidth, int totalLength)
        {
            int num = totalLength - (roomsPerRow - 1) * pathwayWidth;
            int result;
            if (num % roomsPerRow != 0)
            {
                result = -1;
            }
            else
            {
                result = num / roomsPerRow;
            }

            return result;
        }

        private bool TryResolveRandomOption(int maxWidthHeightDiff, int maxPathwayWidthDiff, ResolveParams rp)
        {
            MFI_SymbolResolver_BasePart_Outdoors_Division_Grid.options.Clear();
            for (int i = 0; i < this.optionsX.Count; i++)
            {
                int first    = this.optionsX[i].First;
                int second   = this.optionsX[i].Second;
                int roomSize = this.GetRoomSize(first, second, rp.rect.Width);
                for (int j = 0; j < this.optionsZ.Count; j++)
                {
                    int first2    = this.optionsZ[j].First;
                    int second2   = this.optionsZ[j].Second;
                    int roomSize2 = this.GetRoomSize(first2, second2, rp.rect.Height);
                    if (Mathf.Abs(roomSize - roomSize2) <= maxWidthHeightDiff &&
                        Mathf.Abs(second   - second2)   <= maxPathwayWidthDiff)
                    {
                        MFI_SymbolResolver_BasePart_Outdoors_Division_Grid
                           .options.Add(new Pair<Pair<int, int>, Pair<int, int>>(this.optionsX[i], this.optionsZ[j]));
                    }
                }
            }

            bool result;
            if (MFI_SymbolResolver_BasePart_Outdoors_Division_Grid.options.Any<Pair<Pair<int, int>, Pair<int, int>>>())
            {
                Pair<Pair<int, int>, Pair<int, int>> pair = MFI_SymbolResolver_BasePart_Outdoors_Division_Grid
                                                           .options
                                                           .RandomElement<Pair<Pair<int, int>, Pair<int, int>>>();
                this.ResolveOption(pair.First.First, pair.First.Second, pair.Second.First, pair.Second.Second, rp);
                result = true;
            }
            else
            {
                result = false;
            }

            return result;
        }

        private void ResolveOption(int           roomsPerRowX, int pathwayWidthX, int roomsPerRowZ, int pathwayWidthZ,
                                   ResolveParams rp)
        {
            Map      map       = BaseGen.globalSettings.map;
            int      roomSize  = this.GetRoomSize(roomsPerRowX, pathwayWidthX, rp.rect.Width);
            int      roomSize2 = this.GetRoomSize(roomsPerRowZ, pathwayWidthZ, rp.rect.Height);
            ThingDef thingDef  = null;
            if (pathwayWidthX >= 3)
            {
                if (rp.faction == null || rp.faction.def.techLevel >= TechLevel.Industrial)
                {
                    thingDef = ThingDefOf.StandingLamp;
                }
                else
                {
                    thingDef = ThingDefOf.TorchLamp;
                }
            }

            TerrainDef floorDef = rp.pathwayFloorDef ?? BaseGenUtility.RandomBasicFloorDef(rp.faction, false);
            int        num      = roomSize;
            for (int i = 0; i < roomsPerRowX - 1; i++)
            {
                CellRect rect =
                    new CellRect(rp.rect.minX + num, rp.rect.minZ, pathwayWidthX, rp.rect.Height);
                ResolveParams resolveParams = rp;
                resolveParams.rect             = rect;
                resolveParams.floorDef         = floorDef;
                resolveParams.streetHorizontal = new bool?(false);
                BaseGen.symbolStack.Push("street", resolveParams);
                num += roomSize + pathwayWidthX;
            }

            int num2 = roomSize2;
            for (int j = 0; j < roomsPerRowZ - 1; j++)
            {
                CellRect rect2 =
                    new CellRect(rp.rect.minX, rp.rect.minZ + num2, rp.rect.Width, pathwayWidthZ);
                ResolveParams resolveParams2 = rp;
                resolveParams2.rect             = rect2;
                resolveParams2.floorDef         = floorDef;
                resolveParams2.streetHorizontal = new bool?(true);
                BaseGen.symbolStack.Push("street", resolveParams2);
                num2 += roomSize2 + pathwayWidthZ;
            }

            num  = 0;
            num2 = 0;
            this.children.Clear();
            for (int k = 0; k < roomsPerRowX; k++)
            {
                for (int l = 0; l < roomsPerRowZ; l++)
                {
                    MFI_SymbolResolver_BasePart_Outdoors_Division_Grid.Child child =
                        new MFI_SymbolResolver_BasePart_Outdoors_Division_Grid.Child
                        {
                            rect = new CellRect(rp.rect.minX + num,
                                                rp.rect.minZ +
                                                num2,
                                                roomSize,
                                                roomSize2),
                            gridX = k,
                            gridY = l
                        };
                    this.children.Add(child);
                    num2 += roomSize2 + pathwayWidthZ;
                }

                num  += roomSize + pathwayWidthX;
                num2 =  0;
            }

            this.MergeRandomChildren();
            this.children.Shuffle<MFI_SymbolResolver_BasePart_Outdoors_Division_Grid.Child>();
            for (int m = 0; m < this.children.Count; m++)
            {
                if (thingDef != null)
                {
                    IntVec3 c = new IntVec3(this.children[m].rect.maxX + 1, 0, this.children[m].rect.maxZ);
                    if (rp.rect.Contains(c) && c.Standable(map))
                    {
                        ResolveParams resolveParams3 = rp;
                        resolveParams3.rect           = CellRect.SingleCell(c);
                        resolveParams3.singleThingDef = thingDef;
                        BaseGen.symbolStack.Push("thing", resolveParams3);
                    }
                }

                ResolveParams resolveParams4 = rp;
                resolveParams4.rect = this.children[m].rect;
                BaseGen.symbolStack.Push("MFI_basePart_outdoors", resolveParams4);
            }
        }

        private void MergeRandomChildren()
        {
            if (this.children.Count >= 4)
            {
                int num = GenMath.RoundRandom((float) this.children.Count / 6f);
                for (int i = 0; i < num; i++)
                {
                    MFI_SymbolResolver_BasePart_Outdoors_Division_Grid.Child child =
                        this.children.Find((MFI_SymbolResolver_BasePart_Outdoors_Division_Grid.Child x) => !x.merged);
                    if (child == null)
                    {
                        break;
                    }

                    MFI_SymbolResolver_BasePart_Outdoors_Division_Grid.Child child3 =
                        this.children.Find((MFI_SymbolResolver_BasePart_Outdoors_Division_Grid.Child x) =>
                                               x != child &&
                                               ((Mathf.Abs(x.gridX - child.gridX) == 1 && x.gridY == child.gridY) ||
                                                (Mathf.Abs(x.gridY - child.gridY) == 1 && x.gridX == child.gridX)));
                    if (child3 != null)
                    {
                        this.children.Remove(child);
                        this.children.Remove(child3);
                        MFI_SymbolResolver_BasePart_Outdoors_Division_Grid.Child child2 =
                            new MFI_SymbolResolver_BasePart_Outdoors_Division_Grid.Child
                            {
                                gridX = Mathf.Min(child.gridX, child3.gridX),
                                gridY = Mathf.Min(child.gridY, child3.gridY),
                                merged = true,
                                rect = CellRect.FromLimits(Mathf.Min(child.rect.minX, child3.rect.minX),
                                                           Mathf.Min(child.rect.minZ, child3.rect.minZ),
                                                           Mathf.Max(child.rect.maxX, child3.rect.maxX),
                                                           Mathf.Max(child.rect.maxZ, child3.rect.maxZ))
                            };
                        this.children.Add(child2);
                    }
                }
            }
        }
    }
}