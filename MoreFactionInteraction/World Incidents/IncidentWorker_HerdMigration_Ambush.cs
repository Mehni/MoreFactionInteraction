using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;
using RimWorld.Planet;
using UnityEngine;

namespace MoreFactionInteraction.World_Incidents
{
    public class IncidentWorker_HerdMigration_Ambush : IncidentWorker_Ambush
    {
        PawnKindDef pawnKindDef;

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return Current.Game.Maps.Any(x=> x.Tile == parms.target.Tile) && base.CanFireNowSub(parms);
        }

        protected override LordJob CreateLordJob(List<Pawn> generatedPawns, IncidentParms parms)
        {
            Map map = parms.target as Map;
            TryFindEndCell(map, generatedPawns, out IntVec3 end);
            if (!end.IsValid && CellFinder.TryFindRandomPawnExitCell(generatedPawns[0], out IntVec3 intVec3)) end = intVec3;
            return new LordJob_ExitMapNear(end, LocomotionUrgency.Walk, 12f, false, false);
        }

        protected override List<Pawn> GeneratePawns(IncidentParms parms)
        {
            Map map = parms.target as Map;

            pawnKindDef = PawnKindDefOf.Thrumbo;
            if (parms.target is Site site)
            {
                SitePartWorker_MigratoryHerd sitePart = (SitePartWorker_MigratoryHerd)site.parts.First(x => x.def == DefDatabase<SitePartDef>.GetNamed("MFI_HuntersLodge")).Def.Worker;
                pawnKindDef = sitePart?.pawnKindDef;
            }
            int num = new IntRange(30, 50).RandomInRange;

            List<Pawn> list = new List<Pawn>();
            for (int i = 0; i < num; i++)
            {
                PawnGenerationRequest request = new PawnGenerationRequest(pawnKindDef, null, PawnGenerationContext.NonPlayer, parms.target.Tile);
                Pawn item = PawnGenerator.GeneratePawn(request);
                list.Add(item);
            }
            return list;
        }

        protected override string GetLetterLabel(Pawn anyPawn, IncidentParms parms)
        {
            return string.Format(this.def.letterLabel, pawnKindDef.GetLabelPlural());
        }

        protected override string GetLetterText(Pawn anyPawn, IncidentParms parms)
        {
            return string.Format(this.def.letterText, pawnKindDef.GetLabelPlural());
        }

        private IntVec3 TryFindEndCell(Map map, List<Pawn> generatedPawns, out IntVec3 end)
        {
            end = IntVec3.Invalid;
            for (int i = 0; i < 8; i++)
            {
                IntVec3 intVec3 = generatedPawns[i].Position;
                if (!CellFinder.TryFindRandomEdgeCellWith((IntVec3 x) => map.reachability.CanReach(intVec3, x, PathEndMode.OnCell, TraverseMode.NoPassClosedDoors, Danger.Deadly), map, CellFinder.EdgeRoadChance_Ignore, out IntVec3 intVec))
                {
                    break;
                }
                if (!end.IsValid || intVec.DistanceToSquared(intVec3) > end.DistanceToSquared(intVec3))
                {
                    end = intVec;
                }
            }
            return end;
        }
    }
}
