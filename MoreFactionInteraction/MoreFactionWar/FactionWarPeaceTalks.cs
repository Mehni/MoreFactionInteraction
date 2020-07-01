using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;
using MoreFactionInteraction.MoreFactionWar;

namespace MoreFactionInteraction
{
    public class FactionWarPeaceTalks : WorldObject
    {
        private Material cachedMat;
        private Texture2D cachedExpandoIco;

        private Faction factionOne;
        private Faction factionInstigator;

        private bool canRemoveWithoutPostRemove;

        public override Material Material
        {
            get
            {
                if (this.cachedMat == null && this.Faction != null)
                    this.cachedMat = MaterialPool.MatFrom(texPath: this.def.texture, shader: ShaderDatabase.WorldOverlayTransparentLit, color: (this.factionOne?.Color ?? Color.white), renderQueue: WorldMaterials.WorldObjectRenderQueue);

                return this.cachedMat;
            }
        }

        public override Color ExpandingIconColor => Color.white;

        public override Texture2D ExpandingIcon
        {
            get
            {
                if (this.cachedExpandoIco == null) this.cachedExpandoIco = MatFrom(texPath: this.def.expandingIconTexture, shader: ShaderDatabase.CutoutComplex, color: this.factionOne.Color, colorTwo: this.factionInstigator.Color, renderQueue: WorldMaterials.WorldObjectRenderQueue).GetMaskTexture();
                return this.cachedExpandoIco;
            }
        }

        public void Notify_CaravanArrived(Caravan caravan)
        {
            Pawn pawn = BestCaravanPawnUtility.FindBestDiplomat(caravan: caravan);
            if (pawn == null)
            {
                Messages.Message(text: "MessagePeaceTalksNoDiplomat".Translate(), lookTargets: caravan, def: MessageTypeDefOf.NegativeEvent, historical: false);
            }
            else
            {
                CameraJumper.TryJumpAndSelect(target: caravan);
                var dialogue = new FactionWarDialogue(pawn: pawn, factionOne: this.factionOne, factionInstigator: this.factionInstigator, incidentTarget: caravan);
                var nodeRoot = dialogue.FactionWarPeaceTalks();
                Find.WindowStack.Add(window: new Dialogue_FactionWarNegotiation(factionOne: this.factionOne, factionInstigator: this.factionInstigator, nodeRoot: nodeRoot));
                this.canRemoveWithoutPostRemove = true;
                Find.WorldObjects.Remove(this);
            }
        }

        private static Material MatFrom(string texPath, Shader shader, Color color, Color colorTwo, int renderQueue)
        {
            MaterialRequest materialRequest = new MaterialRequest(tex: ContentFinder<Texture2D>.Get(itemPath: texPath), shader: shader)
            {
                renderQueue = renderQueue,
                color = colorTwo,
                colorTwo = color,
                maskTex = ContentFinder<Texture2D>.Get(itemPath: texPath + Graphic_Single.MaskSuffix, reportFailure: false),
            };
            return MaterialPool.MatFrom(req: materialRequest);
        }

        public void SetWarringFactions(Faction factionOne, Faction factionInstigator)
        {
            this.factionOne = factionOne;
            this.factionInstigator = factionInstigator;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            foreach (FloatMenuOption o in base.GetFloatMenuOptions(caravan: caravan)) yield return o;
            foreach (FloatMenuOption f in CaravanArrivalAction_VisitFactionWarPeaceTalks.GetFloatMenuOptions(caravan: caravan, factionWarPeaceTalks: this)) yield return f;
        }

        public override void PostRemove()
        {
            base.PostRemove();
            if (!this.canRemoveWithoutPostRemove)
                Find.World.GetComponent<WorldComponent_MFI_FactionWar>().DetermineWarAsIfNoPlayerInteraction(this.factionOne, this.factionInstigator);
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref this.factionInstigator, "MFI_PeaceTalksFactionInstigator");
            Scribe_References.Look(ref this.factionOne, "MFI_PeaceTalksFactionOne");
        }
    }
}
