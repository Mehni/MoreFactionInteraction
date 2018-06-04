using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Harmony;

namespace MoreFactionInteraction
{
    [StaticConstructorOnStartup]
    public static class HarmonyPatches
    {
        static HarmonyPatches()
        {
            HarmonyInstance harmony = HarmonyInstance.Create("Mehni.RimWorld.MFI.main");

            harmony.Patch(AccessTools.Method(typeof(TraderKindDef), nameof(TraderKindDef.PriceTypeFor)), null,
                new HarmonyMethod(typeof(HarmonyPatches), nameof(PriceTypeSetter_PostFix)), null);



        }


        private static void PriceTypeSetter_PostFix(ref TraderKindDef __instance, ref PriceType __result, TradeAction action)
        {
            //PriceTypeSetter is more finicky than I'd like, part of the reason traders arrive without any sellable inventory.
            // had issues with pricetype undefined, pricetype normal and *all* traders having pricetype expensive for *all* goods. This works.
            PriceType priceType = __result;
            if (priceType == PriceType.Undefined)
            {
                return;
            }
            //if (__instance.stockGenerators[i] is StockGenerator_BuyCategory && action == TradeAction.PlayerSells)
            if (__instance.stockGenerators.Any(x => x is StockGenerator_BuyCategory) && action == TradeAction.PlayerSells)
            {
                __result = PriceType.Expensive;
            }
            else __result = priceType;
        }

    }
}
