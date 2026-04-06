using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Quality.Harmony
{
    [HarmonyPatch(typeof(XUiM_Trader))]
    [HarmonyPatch(nameof(XUiM_Trader.GetSellPrice))]
    public class XUiMTraderGetSellPrice
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "CustomQualityLevels";
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return codes;

            foreach (var t in codes)
            {
                if (t.opcode != OpCodes.Ldc_R4)
                    continue;
                if ( t.operand is not float value) continue;
                if (!Mathf.Approximately(value, 5.0f)) continue;
                t.operand = (float)QualityUtils.GetMaxQuality();
                break;
            }

            return codes;
        }
    }
}
    
