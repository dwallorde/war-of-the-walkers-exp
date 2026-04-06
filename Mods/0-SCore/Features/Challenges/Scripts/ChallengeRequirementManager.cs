using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using Challenges;

public static class ChallengeRequirementManager
{
   

    public static bool IsValid(string id, ChallengeClass challengeClass = null, MinEventParams minEventContext = null)
    {
    
        if (challengeClass == null)
        {
            challengeClass = ChallengeClass.GetChallenge(id);
            if (challengeClass == null)
            {
                return true;
            }
        }

        var primaryPlayer = GameManager.Instance.World.GetPrimaryPlayer();
        if (minEventContext == null)
            minEventContext = primaryPlayer.MinEventContext;

        minEventContext.Biome = primaryPlayer.biomeStandingOn;

        if (challengeClass.Effects?.EffectGroups == null) return true;
        foreach (var group in challengeClass.Effects.EffectGroups)
        {
            if (!group.canRun(minEventContext))
            {
                return false;
            }
                
        }
      
        return true;
    }

  
}