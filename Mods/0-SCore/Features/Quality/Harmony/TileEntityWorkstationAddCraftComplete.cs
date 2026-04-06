using System;
using HarmonyLib;
using UnityEngine;

namespace SCore.Features.Quality.Harmony
{
    [HarmonyPatch(typeof(TileEntityWorkstation))]
    [HarmonyPatch(nameof(TileEntityWorkstation.AddCraftComplete))]
    public class TileEntityWorkstationAddCraftComplete
    {
        private static readonly string AdvFeatureClass = "AdvancedItemFeatures";
        private static readonly string Feature = "CustomQualityLevels";

        // We use 'ref int crafterEntityID' so we can modify the ID before the original method runs.
        public static bool Prefix(ref int crafterEntityID, ItemValue itemCrafted)
        {
            if (!Configuration.CheckFeatureStatus(AdvFeatureClass, Feature)) return true;

            // 1. Check if the ID is "Packed" (Top 16 bits are not zero)
            // If the top bits are 0, it's just a normal player ID (e.g., 171), so we do nothing.
            if (((crafterEntityID >> 16) & 0xFFFF) != 0)
            {
                int packedValue = crafterEntityID;

                // 2. Unpack the Quality (Top 16 bits)
                int realQuality = (packedValue >> 16) & 0xFFFF;

                // 3. Unpack the Real Player ID (Bottom 16 bits)
                int realPlayerID = packedValue & 0xFFFF;

                // 4. Fix the data
                if (itemCrafted != null)
                {
                    // Update the item's quality to the real value (e.g., 600)
                    // Since itemCrafted is a reference type, this updates the object directly.
                    itemCrafted.Quality = (ushort)realQuality;
                }

                // Update the argument so the original method sees the correct Player ID (e.g., 171)
                crafterEntityID = realPlayerID;

                // Log it for verification
                // Log.Out($"[SCore] AddCraftComplete Fixed: Unpacked Quality {realQuality} from EntityID. Real PlayerID: {realPlayerID}");
            }

            return true; // Return true to let the original method run with our fixed arguments
        }
    }
}