using System;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Quality.Harmony
{
    [HarmonyPatch(typeof(XUiC_WorkstationWindowGroup))]
    [HarmonyPatch("syncTEfromUI")]
    public class XUiCWorkstationWindowGroupSyncTEFromUI
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "CustomQualityLevels";

        public static void Postfix(XUiC_WorkstationWindowGroup __instance)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return;

            // 1. Get the Reference to the TileEntity and the Crafting Queue
            // Note: Accessing private fields via Reflection/Publicized assemblies
            var tileEntity = __instance.WorkstationData.TileEntity;
            var craftingQueue = __instance.craftingQueue;

            if (tileEntity == null || craftingQueue == null) return;

            // 2. Get the list of recipes currently in the UI
            var recipesToCraft = craftingQueue.GetRecipesToCraft();
            if (recipesToCraft == null) return;

            // 3. Get the queue stored in the TileEntity (which currently has the WRONG quality)
            var teQueue = tileEntity.Queue;
            if (teQueue == null) return;

            bool isModified = false;

            // 4. Loop through and correct the data
            // We iterate up to the count of the UI recipes, ensuring we don't go out of bounds of the TE queue
            int count = Math.Min(recipesToCraft.Length, teQueue.Length);
            
            for (int i = 0; i < count; i++)
            {
                var uiRecipe = recipesToCraft[i];
                var teItem = teQueue[i];

                // If the UI recipe exists and is valid
                if (uiRecipe != null && uiRecipe.GetRecipe() != null)
                {
                    // Get the REAL values from the UI
                    int realQuality = uiRecipe.OutputQuality;
                    int playerEntityId = uiRecipe.StartingEntityId;

                    // Only pack if we actually need to (Quality > 255) 
                    // or if you want consistent behavior, just always pack.
                    // Here we check if the TE currently holds a truncated value to be safe.
                    
                    // PACKING LOGIC:
                    int safeId = playerEntityId & 0xFFFF;
                    int safeQuality = realQuality & 0xFFFF;
                    int packedValue = (safeQuality << 16) | safeId;

                    // If the value currently stored is different from our packed value, overwrite it.
                    if (teItem.StartingEntityId != packedValue)
                    {
                        teItem.StartingEntityId = packedValue;
                        isModified = true;

                        if (realQuality > 255)
                        {
                            // Optional: Log only when we are fixing a high-quality item
                           // Log.Out($"[SCore] syncTEfromUI Postfix: Fixed Quality. UI Quality: {realQuality} -> Packed into EntityID: {packedValue}");
                        }
                    }
                }
            }

            // 5. If we changed anything, force the TileEntity to save again
            if (isModified)
            {
                tileEntity.setModified();
            }
        }
    }
}