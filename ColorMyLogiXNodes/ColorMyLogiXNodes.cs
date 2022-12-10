using FrooxEngine;
using FrooxEngine.LogiX;
using HarmonyLib;
using NeosModLoader;
using BaseX;
using System;

namespace ColorMyLogixNodes
{
    public class ColorMyLogixNodes : NeosMod
    {
        public override string Name => "ColorMyLogiXNodes";
        public override string Author => "Nytra";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/Nytra/NeosColorMyLogiXNodes";

        private static bool _first_trigger = false;

        public static ModConfiguration Config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> MOD_ENABLED = new ModConfigurationKey<bool>("Mod enabled.", "", () => true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> USE_RANDOM_COLORS = new ModConfigurationKey<bool>("Use random colors. If false, mod uses the configured color.", "", () => true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<BaseX.color> CONFIGURED_COLOR = new ModConfigurationKey<color>("Configured color.", "", () => new BaseX.color(1.0f, 1.0f, 1.0f, 0.8f));

        private static System.Random rng = new System.Random();

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony($"owo.{Author}.{Name}");

            Config = GetConfiguration()!;
            Config.Save(true);

            harmony.PatchAll();
        }

        [HarmonyPatch(typeof(LogixNode))]
        [HarmonyPatch(nameof(LogixNode.GenerateVisual))]
        class Patch2
        {
            static void Postfix(LogixNode __instance)
            {
                if (!_first_trigger) _first_trigger = true;

                if (Config.GetValue(MOD_ENABLED) == true && __instance.Slot != null)
                {
                    //Msg("---");
                    //Msg($"local user: {__instance.World.LocalUser.ToString()}");
                    //Msg($"logix node refid user: {__instance.ReferenceID.User.ToString()}");
                    //Msg($"logix node local user allocation id: {__instance.LocalUser.AllocationID.ToString()}");
                    //Msg($"logix node refid user == logix node local user allocation id: {__instance.ReferenceID.User == __instance.LocalUser.AllocationID}");

                    var ImageSlot = __instance.Slot.FindChild((Slot c) => c.Name == "Image");

                    //Msg($"image slot refid user: {ImageSlot.ReferenceID.User}");
                    if (ImageSlot != null)
                    {
                        if (ImageSlot.ReferenceID.User == __instance.LocalUser.AllocationID)
                        {
                            var Image = ImageSlot.GetComponent<FrooxEngine.UIX.Image>();
                            if (Image != null)
                            {
                                try
                                {
                                    if (Config.GetValue(USE_RANDOM_COLORS) == true)
                                    {
                                        Image.Tint.Value = new BaseX.color(rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, 0.8f);
                                    }
                                    else
                                    {
                                        Image.Tint.Value = Config.GetValue(CONFIGURED_COLOR);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Error($"Error occurred while trying to set Image tint value.\nError message: {e.Message}");
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
