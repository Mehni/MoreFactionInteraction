using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        public override Material Material
        {
            get
            {
                if (this.cachedMat == null)
                {
                    if (base.Faction != null)
                    {
                        this.cachedMat = MaterialPool.MatFrom(this.def.texture, ShaderDatabase.WorldOverlayTransparentLit, (factionOne?.Color ?? Color.white), WorldMaterials.WorldObjectRenderQueue);
                    }
                    //works for the small zoomed in icon, but is too ugly. Doesn't work for large icon. (Shader unsupported)
                    //this.cachedMat = MatFrom(this.def.texture, ShaderDatabase.CutoutComplex, factionOne.Color, factionInstigator.Color, WorldMaterials.WorldObjectRenderQueue);
                }
                return this.cachedMat;
            }
        }

        public override Color ExpandingIconColor
        {
            get
            {
                return Color.white;
            }
        }

        public override Texture2D ExpandingIcon
        {
            get
            {
                if (cachedExpandoIco == null)
                {
                    cachedExpandoIco = MatFrom(this.def.expandingIconTexture, ShaderDatabase.CutoutComplex, factionOne.Color, factionInstigator.Color, WorldMaterials.WorldObjectRenderQueue).GetMaskTexture();
                }
                return cachedExpandoIco;
            }
        }

        public void Notify_CaravanArrived(Caravan caravan)
        {
            Pawn pawn = BestCaravanPawnUtility.FindBestDiplomat(caravan);
            if (pawn == null)
            {
                Messages.Message("MessagePeaceTalksNoDiplomat".Translate(), caravan, MessageTypeDefOf.NegativeEvent, false);
            }
            else
            {
                CameraJumper.TryJumpAndSelect(caravan);
                Find.WindowStack.Add(new Dialogue_FactionWarNegotiation(factionOne, factionInstigator, FactionWarDialogue.FactionWarPeaceTalks(pawn, factionOne, factionInstigator)));
            }
        }

        private static Material MatFrom(string texPath, Shader shader, Color color, Color colorTwo, int renderQueue)
        {
            MaterialRequest materialRequest = new MaterialRequest(ContentFinder<Texture2D>.Get(texPath, true), shader)
            {
                renderQueue = renderQueue,
                color = colorTwo,
                colorTwo = color,
                maskTex = ContentFinder<Texture2D>.Get(texPath + Graphic_Single.MaskSuffix, false),
            };
            return MaterialPool.MatFrom(materialRequest);
        }

        public void SetWarringFactions(Faction factionOne, Faction factionInstigator)
        {
            this.factionOne = factionOne;
            this.factionInstigator = factionInstigator;
        }

        public override IEnumerable<FloatMenuOption> GetFloatMenuOptions(Caravan caravan)
        {
            foreach (FloatMenuOption o in base.GetFloatMenuOptions(caravan))
            {
                yield return o;
            }
            foreach (FloatMenuOption f in CaravanArrivalAction_VisitFactionWarPeaceTalks.GetFloatMenuOptions(caravan, this))
            {
                yield return f;
            }
        }
    }
}
