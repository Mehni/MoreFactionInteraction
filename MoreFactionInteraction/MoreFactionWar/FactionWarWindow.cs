using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace MoreFactionInteraction.MoreFactionWar
{
    public class FactionWarWindow : MainTabWindow_Factions
    {
        private Vector2 scrollPosition = Vector2.zero;

        private float scrollViewHeight;

        public override void DoWindowContents(Rect fillRect)
        {
            DrawFactionWarBar(fillRect);
            base.DoWindowContents(fillRect);
        }

        private void DrawFactionWarBar(Rect fillRect)
        {

        }
    }
}
