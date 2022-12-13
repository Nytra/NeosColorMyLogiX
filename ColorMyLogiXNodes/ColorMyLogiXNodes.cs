using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.UIX;
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

        private const string IMAGE_SLOT_TAG = "ColorMyLogiXNodes.ColorSet";

        public static ModConfiguration Config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> MOD_ENABLED = new ModConfigurationKey<bool>("Mod enabled:", "", () => true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> USE_RANDOM_COLORS = new ModConfigurationKey<bool>("Use random colors (Does not affect error'd nodes):", "", () => true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<BaseX.color> NODE_COLOR = new ModConfigurationKey<BaseX.color>("Node color:", "", () => new BaseX.color(1.0f, 1.0f, 1.0f, 0.8f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<BaseX.color> NODE_ERROR_COLOR = new ModConfigurationKey<BaseX.color>("Node error color:", "", () => new BaseX.color(3.0f, 0.5f, 0.5f, 0.8f));

        private static System.Random rng = new System.Random();

        public override void OnEngineInit()
        {
            Harmony harmony = new Harmony($"owo.{Author}.{Name}");

            Config = GetConfiguration()!;
            Config.Save(true);

            harmony.PatchAll();
        }

        private static void TrySetTag(Slot s, string tag)
        {
            try
            {
                s.Tag = tag;
            }
            catch (Exception e)
            {
                Error($"Error occurred while trying to set Image Slot Tag.\nError message: {e.Message}");
            }
        }

        private static void TrySetColor(Image image, BaseX.color color)
        {
            try
            {
                image.Tint.Value = color;
            }
            catch (Exception e)
            {
                Error($"Error occurred while trying to set Image Tint Value.\nError message: {e.Message}");
            }
        }

        [HarmonyPatch(typeof(LogixNode))]
        [HarmonyPatch(nameof(LogixNode.GenerateVisual))]
        class Patch2
        {
            static void Postfix(LogixNode __instance)
            {
                if (Config.GetValue(MOD_ENABLED) == true && __instance.Slot != null)
                {
                    var imageSlot = __instance.Slot.FindChild((Slot c) => c.Name == "Image");
                    if (imageSlot != null)
                    {
                        if (imageSlot.ReferenceID.User == __instance.LocalUser.AllocationID) // check logix node visual is allocated to the local user
                        {
                            if (__instance.Enabled == false)
                            {
                                if (imageSlot.Tag == IMAGE_SLOT_TAG) // idk if this will ever be true, because I think Neos changes the tag to "Disabled" when the node breaks.
                                {
                                    TrySetTag(imageSlot, "");
                                }

                                var image = imageSlot.GetComponent<Image>();
                                if (image != null)
                                {
                                    TrySetColor(image, Config.GetValue(NODE_ERROR_COLOR));
                                }
                            }
                            else
                            {
                                if (imageSlot.Tag != IMAGE_SLOT_TAG)
                                {
                                    TrySetTag(imageSlot, IMAGE_SLOT_TAG);

                                    var image = imageSlot.GetComponent<Image>();
                                    if (image != null)
                                    {
                                        if (Config.GetValue(USE_RANDOM_COLORS) == true)
                                        {
                                            TrySetColor(image, new BaseX.color(rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, 0.8f));
                                        }
                                        else
                                        {
                                            TrySetColor(image, Config.GetValue(NODE_COLOR));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}