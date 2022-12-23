using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.UIX;
using HarmonyLib;
using NeosModLoader;
using BaseX;
using System;
using FrooxEngine.LogiX.Math;
using System.Collections.Generic;

namespace ColorMyLogixNodes
{
    public class ColorMyLogixNodes : NeosMod
    {
        public override string Name => "ColorMyLogiXNodes";
        public override string Author => "Nytra";
        public override string Version => "1.0.0-alpha6";
        public override string Link => "https://github.com/Nytra/NeosColorMyLogiXNodes";

        private const string COLOR_SET_TAG = "ColorMyLogiXNodes.ColorSet";

        private enum NodeColorModeEnum
        {
            UseConfig,
            ByNodeName,
            ByFullTypeName,
            ByRefID,
            TrueRandom
        }

        public static ModConfiguration Config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> MOD_ENABLED = new ModConfigurationKey<bool>("Mod enabled:", "", () => true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<NodeColorModeEnum> NODE_COLOR_MODE = new ModConfigurationKey<NodeColorModeEnum>("Node color mode (Does not affect error nodes):", "", () => NodeColorModeEnum.UseConfig);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<BaseX.color> NODE_COLOR = new ModConfigurationKey<BaseX.color>("Node color (For UseConfig mode):", "", () => new BaseX.color(1.0f, 1.0f, 1.0f, 0.8f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<BaseX.color> NODE_ERROR_COLOR = new ModConfigurationKey<BaseX.color>("Node error color:", "", () => new BaseX.color(3.0f, 0.5f, 0.5f, 0.8f));

        private static System.Random rng;

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
                Error($"Error occurred while trying to set Slot Tag.\nError message: {e.Message}");
            }
        }

        private static void TrySetImageTint(Image image, BaseX.color color)
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
        [HarmonyPatch("GenerateUI")]
        class Patch_LogixNode_GenerateUI
        {
            static void Postfix(LogixNode __instance, Slot root)
            {
                // only run if the logix node visual slot is allocated to the local user
                if (Config.GetValue(MOD_ENABLED) == true && root != null && root.ReferenceID.User == root.LocalUser.AllocationID)
                {
                    // don't apply custom color to cast nodes, because it makes it confusing to read the data types
                    if (__instance.Name.Contains("CastClass")) return;

                    if (root.Tag != COLOR_SET_TAG)
                    {
                        var imageSlot = root.FindChild((Slot c) => c.Name == "Image");
                        if (imageSlot != null)
                        {
                            var image = imageSlot.GetComponent<Image>();
                            if (image != null)
                            {
                                if (root.Tag == "Disabled")
                                {
                                    TrySetImageTint(image, Config.GetValue(NODE_ERROR_COLOR));
                                }
                                else
                                {
                                    BaseX.color colorToSet = Config.GetValue(NODE_COLOR);
                                    rng = null;
                                    switch (Config.GetValue(NODE_COLOR_MODE))
                                    {
                                        case NodeColorModeEnum.UseConfig:
                                            colorToSet = Config.GetValue(NODE_COLOR);
                                            break;
                                        case NodeColorModeEnum.ByNodeName:
                                            rng = new System.Random(LogixHelper.GetNodeName(__instance.GetType()).GetHashCode());
                                            break;
                                        case NodeColorModeEnum.ByFullTypeName:
                                            rng = new System.Random(__instance.GetType().FullName.GetHashCode());
                                            break;
                                        case NodeColorModeEnum.ByRefID:
                                            rng = new System.Random(root.Parent.ReferenceID.GetHashCode());
                                            break;
                                        case NodeColorModeEnum.TrueRandom:
                                            rng = new System.Random();
                                            break;
                                        default:
                                            break;
                                    }
                                    if (rng != null)
                                    {
                                        colorToSet = new BaseX.color(rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, 0.8f);
                                    }
                                    TrySetImageTint(image, colorToSet);
                                    TrySetTag(root, COLOR_SET_TAG);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}