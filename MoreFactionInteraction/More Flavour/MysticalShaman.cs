using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;

namespace MoreFactionInteraction.More_Flavour
{
    internal class MysticalShaman : WorldObject
    {
        private Material cachedMat;

        public override Material Material
        {
            get
            {
                if (this.cachedMat == null)
                {
                    this.cachedMat = MaterialPool.MatFrom(texPath: this.def.expandingIconTexture, shader: ShaderDatabase.WorldOverlayTransparentLit, color: base.Faction.Color, renderQueue: WorldMaterials.WorldObjectRenderQueue);
                }
                return this.cachedMat;
            }
        }

        public override IEnumerable<Gizmo> GetCaravanGizmos(Caravan caravan)
        {
            return base.GetCaravanGizmos(caravan: caravan);
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            foreach (FloatMenuOption o in base.GetFloatMenuOptions(caravan: caravan)) yield return o;
            foreach (FloatMenuOption f in CaravanArrivalAction_VisitMysticalShaman.GetFloatMenuOptions(caravan: caravan, mysticalShaman: this)) yield return f;
        }

        public void Notify_CaravanArrived(Caravan caravan)
        {
            Pawn pawn = WorstCaravanPawnUtility.FindSickestPawn(caravan: caravan);
            if (pawn == null)
            {
                Find.WindowStack.Add(window: new Dialog_MessageBox(text: "MFI_MysticalShamanFoundNoSickPawn".Translate()));
                //    Dialog_MessageBox.CreateConfirmation(text: "MFI_MysticalShamanFoundNoSickPawn".Translate(),
                //                                                 confirmedAct: () => Find.WorldObjects.Remove(o: this)));
            }
            else
            {
                CameraJumper.TryJumpAndSelect(target: caravan);
                Thing serum = ThingMaker.MakeThing(def: ThingDef.Named(defName: "MechSerumHealer")); //obj ref req, but me lazy.
                serum.TryGetComp<CompUseEffect_FixWorstHealthCondition>().DoEffect(usedBy: pawn);
                serum = null;
                Find.WindowStack.Add(window: new Dialog_MessageBox(text: "MFI_MysticalShamanDoesHisMagic".Translate(pawn.LabelShort))); 
            }
            Find.WorldObjects.Remove(o: this);
        }
    }

    internal class WorstCaravanPawnUtility
    {
        private static readonly Dictionary<Pawn, int> tempPawns = new Dictionary<Pawn, int>();

        public static Pawn FindSickestPawn(Caravan caravan)
        {
            tempPawns.Clear();
            foreach (Pawn pawn in caravan.PawnsListForReading) tempPawns.Add(key: pawn, value: CalcHealthThreatenedScore(usedBy: pawn));
            tempPawns.RemoveAll(predicate: x => x.Value == 0);
            return tempPawns.FirstOrDefault(predicate: x => x.Value.Equals(obj: tempPawns.Values.Max())).Key;
        }

        private static float HandCoverageAbsWithChildren
        {
            get
            {
                return ThingDefOf.Human.race.body.GetPartsWithDef(def: BodyPartDefOf.Hand).First().coverageAbsWithChildren;
            }
        }

        //Taken from CompUseEffect_FixWorstHealthCondition, with a bit of Resharper cleanup to stop my eyes bleeding.
        private static int CalcHealthThreatenedScore(Pawn usedBy)
        {
            Hediff hediff = FindLifeThreateningHediff(pawn: usedBy);
            if (hediff != null) return 8192;

            if (HealthUtility.TicksUntilDeathDueToBloodLoss(pawn: usedBy) < 2500)
            {
                Hediff hediff2 = FindMostBleedingHediff(pawn: usedBy);
                if (hediff2 != null) return 4096;
            }

            if (usedBy.health.hediffSet.GetBrain() != null)
            {
                Hediff_Injury hediffInjury = FindPermanentInjury(pawn: usedBy, allowedBodyParts: Gen.YieldSingle(val: usedBy.health.hediffSet.GetBrain()));
                if (hediffInjury != null) return 2048;
            }

            BodyPartRecord bodyPartRecord = FindBiggestMissingBodyPart(pawn: usedBy, minCoverage: HandCoverageAbsWithChildren);
            if (bodyPartRecord != null) return 1024;

            Hediff_Injury hediffInjury2 = FindPermanentInjury(pawn: usedBy,
                                                              allowedBodyParts: usedBy.health.hediffSet.GetNotMissingParts()
                                                                    .Where(predicate: x => x.def == BodyPartDefOf.Eye));
            if (hediffInjury2 != null) return 512;

            Hediff hediff3 = FindImmunizableHediffWhichCanKill(pawn: usedBy);
            if (hediff3 != null) return 255;

            Hediff hediff4 = FindNonInjuryMiscBadHediff(pawn: usedBy, onlyIfCanKill: true);
            if (hediff4 != null) return 128;

            Hediff hediff5 = FindNonInjuryMiscBadHediff(pawn: usedBy, onlyIfCanKill: false);
            if (hediff5 != null) return 64;

            if (usedBy.health.hediffSet.GetBrain() != null)
            {
                Hediff_Injury hediffInjury3 = FindInjury(pawn: usedBy, allowedBodyParts: Gen.YieldSingle(val: usedBy.health.hediffSet.GetBrain()));
                if (hediffInjury3 != null) return 32;
            }
            BodyPartRecord bodyPartRecord2 = FindBiggestMissingBodyPart(pawn: usedBy);
            if (bodyPartRecord2 != null) return 16;

            Hediff_Addiction hediffAddiction = FindAddiction(pawn: usedBy);
            if (hediffAddiction != null) return 8;

            Hediff_Injury hediffInjury4 = FindPermanentInjury(pawn: usedBy);
            if (hediffInjury4 != null) return 4;

            Hediff_Injury hediffInjury5 = FindInjury(pawn: usedBy);
            if (hediffInjury5 != null) return 2;

            return 0;
        }

        private static Hediff FindLifeThreateningHediff(Pawn pawn)
        {
            Hediff hediff = null;
            float num = -1f;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            foreach (Hediff current in hediffs)
                if (current.Visible && current.def.everCurableByItem)
                    if (!current.FullyImmune())
                    {
                        bool flag  = current.CurStage           != null && current.CurStage.lifeThreatening;
                        bool flag2 = current.def.lethalSeverity >= 0f   && current.Severity / current.def.lethalSeverity >= 0.8f;
                        if (flag || flag2)
                        {
                            float num2 = current.Part?.coverageAbsWithChildren ?? 999f;
                            if (hediff == null || num2 > num)
                            {
                                hediff = current;
                                num    = num2;
                            }
                        }
                    }

            return hediff;
        }

        private static Hediff FindMostBleedingHediff(Pawn pawn)
        {
            float num = 0f;
            Hediff hediff = null;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            foreach (Hediff current in hediffs)
            {
                if (current.Visible && current.def.everCurableByItem)
                {
                    float bleedRate = current.BleedRate;
                    if (bleedRate > 0f && (bleedRate > num || hediff == null))
                    {
                        num    = bleedRate;
                        hediff = current;
                    }
                }
            }
            return hediff;
        }

        private static Hediff FindImmunizableHediffWhichCanKill(Pawn pawn)
        {
            Hediff hediff = null;
            float num = -1f;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            foreach (Hediff current in hediffs)
            {
                if (current.Visible && current.def.everCurableByItem)
                    if (current.TryGetComp<HediffComp_Immunizable>() != null)
                        if (!current.FullyImmune())
                            if (CanEverKill(hediff: current))
                            {
                                float severity = current.Severity;
                                if (hediff == null || severity > num)
                                {
                                    hediff = current;
                                    num    = severity;
                                }
                            }
            }
            return hediff;
        }

        private static Hediff FindNonInjuryMiscBadHediff(Pawn pawn, bool onlyIfCanKill)
        {
            Hediff hediff = null;
            float num = -1f;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            foreach (Hediff current in hediffs)
            {
                if (current.Visible && current.def.isBad && current.def.everCurableByItem)
                    if (!(current is Hediff_Injury) && !(current is Hediff_MissingPart) && !(current is Hediff_Addiction) && !(current is Hediff_AddedPart))
                        if (!onlyIfCanKill || CanEverKill(hediff: current))
                        {
                            float num2 = (current.Part == null) ? 999f : current.Part.coverageAbsWithChildren;
                            if (hediff == null || num2 > num)
                            {
                                hediff = current;
                                num    = num2;
                            }
                        }
            }
            return hediff;
        }

        private static BodyPartRecord FindBiggestMissingBodyPart(Pawn pawn, float minCoverage = 0f)
        {
            BodyPartRecord bodyPartRecord = null;
            foreach (Hediff_MissingPart current in pawn.health.hediffSet.GetMissingPartsCommonAncestors())
                if (current.Part.coverageAbsWithChildren >= minCoverage)
                    if (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(part: current.Part))
                        if (bodyPartRecord == null || current.Part.coverageAbsWithChildren > bodyPartRecord.coverageAbsWithChildren)
                            bodyPartRecord = current.Part;

            return bodyPartRecord;
        }

        private static Hediff_Addiction FindAddiction(Pawn pawn)
        {
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            foreach (Hediff current in hediffs)
            {
                if (current is Hediff_Addiction hediffAddiction && hediffAddiction.Visible && hediffAddiction.def.everCurableByItem)
                {
                    return hediffAddiction;
                }
            }
            return null;
        }

        private static Hediff_Injury FindPermanentInjury(Pawn pawn, IEnumerable<BodyPartRecord> allowedBodyParts = null)
        {
            Hediff_Injury hediffInjury = null;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            for (int i = 0; i < hediffs.Count; i++)
            {
                if (hediffs[index: i] is Hediff_Injury hediffInjury2 && hediffInjury2.Visible && hediffInjury2.IsPermanent() && hediffInjury2.def.everCurableByItem)
                    if (allowedBodyParts == null || allowedBodyParts.Contains(value: hediffInjury2.Part))
                        if (hediffInjury == null || hediffInjury2.Severity > hediffInjury.Severity)
                            hediffInjury = hediffInjury2;
            }
            return hediffInjury;
        }

        private static Hediff_Injury FindInjury(Pawn pawn, IEnumerable<BodyPartRecord> allowedBodyParts = null)
        {
            Hediff_Injury hediffInjury = null;
            List<Hediff> hediffs = pawn.health.hediffSet.hediffs;
            foreach (Hediff hediff in hediffs)
                if (hediff is Hediff_Injury hediffInjury2 && hediffInjury2.Visible && hediffInjury2.def.everCurableByItem)
                    if (allowedBodyParts == null || allowedBodyParts.Contains(value: hediffInjury2.Part))
                        if (hediffInjury == null || hediffInjury2.Severity > hediffInjury.Severity)
                            hediffInjury = hediffInjury2;

            return hediffInjury;
        }

        private static bool CanEverKill(Hediff hediff)
        {
            if (hediff.def.stages != null)
                if (Enumerable.Any(source: hediff.def.stages, predicate: t => t.lifeThreatening))
                
                    return true;
            return (hediff.def.lethalSeverity >= 0f);
        }
    }
}