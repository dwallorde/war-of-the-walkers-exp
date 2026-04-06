using System;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Quality.Harmony
{
    [HarmonyPatch(typeof(XUiC_RecipeStack))]
    [HarmonyPatch(nameof(XUiC_RecipeStack.SetRecipe))]
    public class XUiCRecipeStackSetRecipe
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "CustomQualityLevels";

        public static bool Prefix( XUiC_RecipeStack __instance,  int _outputQuality = -1)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;
            if (__instance.recipe == null ) return true;
            
            if (Configuration.CheckFeatureStatus(AdvFeatureClass, "Logging"))
                Log.Out($"SetRecipe(): Recipe: {__instance.recipe.ToString()} Quality: {__instance.recipe.craftingTier} OutputQuality: {__instance.outputQuality} : Passed in Output: {_outputQuality}");
            return true;
        }
    }
}