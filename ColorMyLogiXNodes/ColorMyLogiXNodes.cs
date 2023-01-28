using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.UIX;
using HarmonyLib;
using NeosModLoader;
using System;
using System.Reflection;
using BaseX;
//using System.Linq;
//using System.Collections.Generic;
//using System.Runtime.InteropServices;

namespace ColorMyLogixNodes
{
    public class ColorMyLogixNodes : NeosMod
    {
        public override string Name => "ColorMyLogiXNodes";
        public override string Author => "Nytra";
        public override string Version => "1.0.0-alpha7.3.10";
        public override string Link => "https://github.com/Nytra/NeosColorMyLogiXNodes";

        public static ModConfiguration Config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> MOD_ENABLED = new ModConfigurationKey<bool>("MOD_ENABLED", "Mod Enabled:", () => true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<NodeColorModeEnum> NODE_COLOR_MODE = new ModConfigurationKey<NodeColorModeEnum>("NODE_COLOR_MODE", "Which factor determines the color of nodes:", () => NodeColorModeEnum.Config);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<BaseX.color> NODE_COLOR = new ModConfigurationKey<BaseX.color>("NODE_COLOR", "Config Node Color:", () => new BaseX.color(1.0f, 1.0f, 1.0f, 0.8f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<BaseX.color> NODE_ERROR_COLOR = new ModConfigurationKey<BaseX.color>("NODE_ERROR_COLOR", "Node Error Color:", () => new BaseX.color(3.0f, 0.5f, 0.5f, 0.8f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<RandomSeedEnum> RANDOM_SEED_METHOD = new ModConfigurationKey<RandomSeedEnum>("RANDOM_SEED_METHOD", "Randomness Options:", () => RandomSeedEnum.NoRandomness);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<int> RANDOM_SEED_OFFSET = new ModConfigurationKey<int>("RANDOM_SEED_OFFSET", "Optional integer value added to the Node Factor Seed to get different colors:", () => 0);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<ColorModelEnum> COLOR_MODEL = new ModConfigurationKey<ColorModelEnum>("COLOR_MODEL", "Color Model:", () => ColorModelEnum.HSV);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float2> HSV_SATURATION_AND_VALUE = new ModConfigurationKey<float2>("HSV_SATURATION_AND_VALUE", "HSV Saturation and Value:", () => new float2(1f, 1f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float2> HSL_SATURATION_AND_LIGHTNESS = new ModConfigurationKey<float2>("HSL_SATURATION_AND_LIGHTNESS", "HSL Saturation and Lightness:", () => new float2(1f, 1f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> MULTIPLY_OUTPUT_BY_RGB = new ModConfigurationKey<bool>("MULTIPLY_OUTPUT_BY_RGB", "Should the RGB Channel Multiplier be used on the output color:", () => false);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float3> RGB_CHANNEL_MULTIPLIER = new ModConfigurationKey<float3>("RGB_CHANNEL_MULTIPLIER", "RGB Channel Multiplier:", () => new float3(1f, 1f, 1f));

        // INTERNAL ACCESS CONFIG KEYS
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<int> REFID_MOD_DIVISOR = new ModConfigurationKey<int>("REFID_MOD_DIVISOR", "Modulo divisor for RefID to Color conversion:", () => 100000, internalAccessOnly: true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<int> STRING_MOD_DIVISOR = new ModConfigurationKey<int>("STRING_MOD_DIVISOR", "Modulo divisor for String to Color conversion:", () => 1000, internalAccessOnly: true);
        // =====

        private enum RandomSeedEnum
        {
            NoRandomness,
            SeededByNodeFactor,
            SeededBySystemTime
        }

        private enum ColorModelEnum
        {
            HSV,
            HSL
        }

        private enum NodeColorModeEnum
        {
            Config,
            NodeName,
            NodeCategory,
            TopmostNodeCategory,
            FullTypeName,
            RefID
        }

        private static System.Random rng;
        private static readonly System.Random rngTimeSeeded = new System.Random();

        private const string COLOR_SET_TAG = "ColorMyLogiXNodes.ColorSet";

        //private static List<int> allNums = new List<int>();
        //private static List<int> allNumsNoRelay = new List<int>();

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

        private static string GetNodeCategoryString(Type logixType, bool onlyTopmost=false)
        {
            Category customAttribute = logixType.GetCustomAttribute<Category>();
            if (customAttribute == null)
            {
                return "";
            }
            else
            {
                string[] categoryPaths = customAttribute.Paths;
                if (categoryPaths.Length > 0)
                {
                    if (onlyTopmost)
                    {
                        string[] parts = categoryPaths[0].Split('/');
                        if (parts.Length > 1)
                        {
                            return parts[0] + "/" + parts[1];
                        }
                        else
                        {
                            return parts[0];
                        }
                    }
                    else
                    {
                        return categoryPaths[0];
                    }
                }
                else
                {
                    return "";
                }
            }
        }

        private static BaseX.color GetHueColorFromUlong(ulong val, ulong divisor)
        {
            float hue = (val % divisor) / (float)divisor;
            BaseX.color c = Config.GetValue(NODE_COLOR);
            switch (Config.GetValue(COLOR_MODEL))
            {
                case ColorModelEnum.HSV:
                    float2 hsv_values = Config.GetValue(HSV_SATURATION_AND_VALUE);
                    c = new ColorHSV(hue, hsv_values.x, hsv_values.y, 0.8f).ToRGB();
                    break;
                case ColorModelEnum.HSL:
                    float2 hsl_values = Config.GetValue(HSL_SATURATION_AND_LIGHTNESS);
                    c = new ColorHSL(hue, hsl_values.x, hsl_values.y, 0.8f).ToRGB();
                    break;
                default:
                    break;
            }
            return c;
        }

        private static BaseX.color GetColorFromString(string str)
        {
            Msg($"str: {str}");
            int val = 0;
            foreach (char c in str)
            {
                val += (int)c;
            }
            //Msg($"val: {val.ToString()}");
            //allNums.Add(val);
            //if (str != "Relay") allNumsNoRelay.Add(val);
            //Msg($"max: {allNums.Max()}");
            //Msg($"min: {allNums.Min()}");
            //Msg($"average: {allNums.Average()}");
            //Msg($"maxNR: {allNumsNoRelay.Max()}");
            //Msg($"minNR: {allNumsNoRelay.Min()}");
            //Msg($"averageNR: {allNumsNoRelay.Average()}");
            if (val < 0) val = 0;
            int stringModDivisor = Config.GetValue(STRING_MOD_DIVISOR);
            ulong divisor = (stringModDivisor > 0) ? (ulong)stringModDivisor : 0;
            return GetHueColorFromUlong((ulong)val, divisor);
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
                    if (__instance.Name.Contains("Cast_")) return;

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
                                    string nodeCategoryString;
                                    BaseX.color colorToSet = Config.GetValue(NODE_COLOR);
                                    rng = null;
                                    if (Config.GetValue(NODE_COLOR_MODE) == NodeColorModeEnum.Config)
                                    {
                                        colorToSet = Config.GetValue(NODE_COLOR);
                                    }
                                    else if (Config.GetValue(RANDOM_SEED_METHOD) == RandomSeedEnum.SeededBySystemTime)
                                    {
                                        rng = rngTimeSeeded;
                                    }
                                    else
                                    {
                                        switch (Config.GetValue(NODE_COLOR_MODE))
                                        {
                                            case NodeColorModeEnum.NodeName:
                                                if (Config.GetValue(RANDOM_SEED_METHOD) == RandomSeedEnum.SeededByNodeFactor)
                                                {
                                                    rng = new System.Random(LogixHelper.GetNodeName(__instance.GetType()).GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
                                                }
                                                else
                                                {
                                                    colorToSet = GetColorFromString(LogixHelper.GetNodeName(__instance.GetType()));
                                                }
                                                break;
                                            case NodeColorModeEnum.NodeCategory:
                                                nodeCategoryString = GetNodeCategoryString(__instance.GetType());
                                                if (Config.GetValue(RANDOM_SEED_METHOD) == RandomSeedEnum.SeededByNodeFactor)
                                                {
                                                    rng = new System.Random(nodeCategoryString.GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
                                                }
                                                else
                                                {
                                                    colorToSet = GetColorFromString(nodeCategoryString);
                                                }
                                                break;
                                            case NodeColorModeEnum.TopmostNodeCategory:
                                                nodeCategoryString = GetNodeCategoryString(__instance.GetType(), onlyTopmost: true);
                                                if (Config.GetValue(RANDOM_SEED_METHOD) == RandomSeedEnum.SeededByNodeFactor)
                                                {
                                                    rng = new System.Random(nodeCategoryString.GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
                                                }
                                                else
                                                {
                                                    colorToSet = GetColorFromString(nodeCategoryString);
                                                }
                                                break;
                                            case NodeColorModeEnum.FullTypeName:
                                                if (Config.GetValue(RANDOM_SEED_METHOD) == RandomSeedEnum.SeededByNodeFactor)
                                                {
                                                    rng = new System.Random(__instance.GetType().FullName.GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
                                                }
                                                else
                                                {
                                                    colorToSet = GetColorFromString(__instance.GetType().FullName);
                                                }
                                                break;
                                            case NodeColorModeEnum.RefID:
                                                if (Config.GetValue(RANDOM_SEED_METHOD) == RandomSeedEnum.SeededByNodeFactor)
                                                {
                                                    // maybe use ReferenceID.Position here instead?
                                                    rng = new System.Random(root.Parent.ReferenceID.GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
                                                }
                                                else
                                                {
                                                    int refidModDivisor = Config.GetValue(REFID_MOD_DIVISOR);
                                                    ulong divisor = (refidModDivisor > 0) ? (ulong)refidModDivisor : 0;
                                                    colorToSet = GetHueColorFromUlong(root.Parent.ReferenceID.Position, divisor);
                                                }
                                                //Msg($"RefID Position: {root.Parent.ReferenceID.Position.ToString()}");
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    if (rng != null)
                                    {
                                        // RNG seeded by any constant node factor will always give the same color
                                        switch (Config.GetValue(COLOR_MODEL))
                                        {
                                            case ColorModelEnum.HSV:
                                                colorToSet = new ColorHSV(rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, 0.8f).ToRGB();
                                                break;
                                            case ColorModelEnum.HSL:
                                                colorToSet = new ColorHSL(rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, 0.8f).ToRGB();
                                                break;
                                            default:
                                                break;
                                        }
                                    }
                                    if (Config.GetValue(MULTIPLY_OUTPUT_BY_RGB))
                                    {
                                        float3 multiplier = Config.GetValue(RGB_CHANNEL_MULTIPLIER);
                                        colorToSet = colorToSet.MulR(multiplier.x);
                                        colorToSet = colorToSet.MulG(multiplier.y);
                                        colorToSet = colorToSet.MulB(multiplier.z);
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