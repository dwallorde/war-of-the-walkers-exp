// using HarmonyLib;
// using UnityEngine;
//
// namespace SCore.Harmony.NPCFeatures
// {
//     [HarmonyPatch(typeof(EntityAlive))]
//     [HarmonyPatch(nameof(EntityAlive.AwardKill))]
//     public class EntityAliveAwardKill
//     {
//         public static void Postfix(EntityAlive __instance, EntityAlive killer)
//         {
//             Debug.Log("AwardKill:: AwardKill XP");
//             // 1. Safety Checks
//             // Ensure this is the Server (XP is authoritative). 
//             // Ensure valid killer and victim.
//           //  if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer) return;
//             if (killer == null || __instance == null) return;
//
//             // 2. Identify NPC
//             var entityAliveSdx = killer as EntityAliveSDX;
//             if (entityAliveSdx == null) return;
//
//             // 3. Give XP to the NPC (The Killer)
//             // Since the base AddKillXP might be skipped or broken for NPCs, we add it directly.
//             if (entityAliveSdx.Progression != null)
//             {
//                 int npcXP = EntityClass.list[__instance.entityClass].ExperienceValue;
//                 entityAliveSdx.Progression.AddLevelExp(npcXP, "_xpFromKill");
//             }
//
//             // 4. Share with Leader & Party
//             var leader = EntityUtilities.GetLeaderOrOwner(killer.entityId) as EntityPlayer;
//             if (leader == null) return;
//
//             // Calling AddKillXP on the LEADER handles everything else:
//             // - Applies Leader's XP buffs (optional benefit)
//             // - Calculates Party Tax
//             // - Distributes to Party Members
//             // - Updates "Kill Quest" counters for Leader + Party
//             Debug.Log("AwardKill:: AddKill XP");
//             leader.AddKillXP(__instance);
//         }
//     }
// }