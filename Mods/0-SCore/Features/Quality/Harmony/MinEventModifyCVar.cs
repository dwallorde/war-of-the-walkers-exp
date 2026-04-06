using HarmonyLib;
using SCore.Features.ItemDegradation.Utils;
using UnityEngine;

namespace SCore.Features.ItemDegradation.Harmony.General
{
   [HarmonyPatch(typeof(MinEventActionModifyCVar))]
[HarmonyPatch(nameof(MinEventActionModifyCVar.Execute))]
public class MinEventActionModifyCVarExecute
{
    private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
    private static readonly string Feature = "CustomQualityLevels";

    public static bool Prefix(MinEventActionModifyCVar __instance, MinEventParams _params)
    {
        // Check if the custom quality feature is enabled in the mod settings
        if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) 
            return true;

        // 1. Calculate the base value based on the Roll Type
        if (__instance.rollType == MinEventActionModifyCVar.RandomRollTypes.tierList)
        {
            // Case A: Items and Modifiers (The 1-600 logic)
            if ((_params.ParentType == MinEffectController.SourceParentType.ItemClass || 
                 _params.ParentType == MinEffectController.SourceParentType.ItemModifierClass) && 
                !_params.ItemValue.IsEmpty())
            {
                // QualityUtils.CalculateTier returns 1-6. 
                // We subtract 1 to get index 0-5 for the valueList array.
                int tierIndex = QualityUtils.CalculateTier(_params.ItemValue.Quality) - 1;

                // Safety clamp: ensures we never go out of bounds of the XML-provided list
                tierIndex = Mathf.Clamp(tierIndex, 0, __instance.valueList.Length - 1);
                __instance.value = __instance.valueList[tierIndex];
            }
            // Case B: Progression/Perks (Player skills that use comma-separated values)
            else if (_params.ParentType == MinEffectController.SourceParentType.ProgressionClass && _params.ProgressionValue != null)
            {
                // Progression levels are usually 0-indexed or 1-indexed depending on the perk;
                // vanilla uses the level directly as the index.
                int progLevel = _params.ProgressionValue.CalculatedLevel(_params.Self);
                int index = Mathf.Clamp(progLevel, 0, __instance.valueList.Length - 1);
                __instance.value = __instance.valueList[index];
            }
        }

        // 2. Apply the calculated value to all targets
        for (int i = 0; i < __instance.targets.Count; i++)
        {
            float finalValue = __instance.value;

            // Handle CVar references (using another CVar's value)
            if (__instance.cvarRef)
            {
                finalValue = __instance.targets[i].Buffs.GetCustomVar(__instance.refCvarName);
            }
            // Handle Random Integer rolls
            else if (__instance.rollType == MinEventActionModifyCVar.RandomRollTypes.randomInt)
            {
                finalValue = Mathf.Clamp((float)_params.Self.rand.RandomRange((int)__instance.minValue, (int)__instance.maxValue + 1), __instance.minValue, __instance.maxValue);
            }
            // Handle Random Float rolls
            else if (__instance.rollType == MinEventActionModifyCVar.RandomRollTypes.randomFloat)
            {
                finalValue = Mathf.Clamp(_params.Self.rand.RandomRange(__instance.minValue, __instance.maxValue + 1f), __instance.minValue, __instance.maxValue);
            }

            // Set the CVar on the specific target (e.g., Self, Other, etc.)
            __instance.targets[i].Buffs.SetCustomVar(
                __instance.cvarName, 
                finalValue, 
                (__instance.targets[i].isEntityRemote && !_params.Self.isEntityRemote) || _params.IsLocal, 
                __instance.operation
            );
        }

        // Skip the vanilla Execute() to prevent out-of-bounds crashes on the quality-to-index calculation
        return false; 
    }
}
}