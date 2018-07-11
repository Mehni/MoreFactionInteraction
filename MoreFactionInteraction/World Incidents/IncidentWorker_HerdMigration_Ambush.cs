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
            return Current.Game.Maps.Any(predicate: x=> x.Tile == parms.target.Tile) && base.CanFireNowSub(parms: parms);
        }

        protected override LordJob CreateLordJob(List<Pawn> generatedPawns, IncidentParms parms)
        {
            Map map = parms.target as Map;
            TryFindEndCell(map: map, generatedPawns: generatedPawns, end: out IntVec3 end);
            if (!end.IsValid && CellFinder.TryFindRandomPawnExitCell(searcher: generatedPawns[index: 0], result: out IntVec3 intVec3)) end = intVec3;
            return new LordJob_ExitMapNear(near: end, locomotion: LocomotionUrgency.Walk);
        }

        protected override List<Pawn> GeneratePawns(IncidentParms parms)
        {
            Map map = parms.target as Map;

            this.pawnKindDef = PawnKindDefOf.Thrumbo;
            if (parms.target is Site site)
            {
                SitePartWorker_MigratoryHerd sitePart = (SitePartWorker_MigratoryHerd)site.parts.First(predicate: x => x.def == DefDatabase<SitePartDef>.GetNamed(defName: "MFI_HuntersLodge")).Def.Worker;
                this.pawnKindDef = sitePart?.pawnKindDef;
            }
            int num = new IntRange(min: 30, max: 50).RandomInRange;

            List<Pawn> list = new List<Pawn>();
            for (int i = 0; i < num; i++)
            {
                PawnGenerationRequest request = new PawnGenerationRequest(kind: this.pawnKindDef, faction: null, context: PawnGenerationContext.NonPlayer, tile: parms.target.Tile);
                Pawn item = PawnGenerator.GeneratePawn(request: request);
                list.Add(item: item);
            }
            return list;
        }

        protected override string GetLetterLabel(Pawn anyPawn, IncidentParms parms)
        {
            return string.Format(format: this.def.letterLabel, arg0: this.pawnKindDef.GetLabelPlural());
        }

        protected override string GetLetterText(Pawn anyPawn, IncidentParms parms)
        {
            return string.Format(format: this.def.letterText, arg0: this.pawnKindDef.GetLabelPlural());
        }

        private static IntVec3 TryFindEndCell(Map map, List<Pawn> generatedPawns, out IntVec3 end)
        {
            end = IntVec3.Invalid;
            for (int i = 0; i < 8; i++)
            {
                IntVec3 intVec3 = generatedPawns[index: i].Position;
                if (!CellFinder.TryFindRandomEdgeCellWith(validator: (IntVec3 x) => map.reachability.CanReach(start: intVec3, dest: x, peMode: PathEndMode.OnCell, traverseMode: TraverseMode.NoPassClosedDoors, maxDanger: Danger.Deadly), map: map, roadChance: CellFinder.EdgeRoadChance_Ignore, result: out IntVec3 intVec))
                {
                    break;
                }
                if (!end.IsValid || intVec.DistanceToSquared(b: intVec3) > end.DistanceToSquared(b: intVec3))
                {
                    end = intVec;
                }
            }
            return end;
        }
    }
}
