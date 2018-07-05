using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using UnityEngine;

namespace MoreFactionInteraction
{
    public class FactionWarPeaceTalks : WorldObject
    {
        private Material cachedMat;
        
        private Faction factionOne;
        private Faction factionInstigator;

        public override Material Material
        {
            get
            {
                if (this.cachedMat == null)
                {
                    this.cachedMat = MatFrom(this.def.texture, ShaderDatabase.CutoutComplex, factionOne.Color, factionInstigator.Color, WorldMaterials.WorldObjectRenderQueue);
                }
                return this.cachedMat;
            }
        }

        public void Notify_CaravanArrived(Caravan caravan)
        {
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
            foreach (FloatMenuOption f in CaravanArrivalAction_VisitPeaceTalks.GetFloatMenuOptions(caravan, this))
            {
                yield return f;
            }
        }
    }
}
