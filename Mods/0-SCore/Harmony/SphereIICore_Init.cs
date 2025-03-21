using System.Reflection;
namespace Harmony
{
    public class SphereIICoreInit : IModApi
    {
        public void InitMod(Mod _modInstance)
        {
            Log.Out(" Loading Patch: " + GetType());
            var harmony = new HarmonyLib.Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            // Check Harmony/ModEvents.cs for registration of other events which can be called here.
            SCoreModEvents.Init();
        }
    }
}