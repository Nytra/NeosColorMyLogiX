using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.UIX;
using HarmonyLib;
using NeosModLoader;
using System;
using System.Reflection;
using BaseX;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ColorMyLogixNodes
{
    public class ColorMyLogixNodes : NeosMod
    {
        public override string Name => "ColorMyLogiXNodes";
        public override string Author => "Nytra";
        public override string Version => "1.0.0-alpha7.3.7";
        public override string Link => "https://github.com/Nytra/NeosColorMyLogiXNodes";

        public static ModConfiguration Config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> MOD_ENABLED = new ModConfigurationKey<bool>("MOD_ENABLED", "Controls whether or not the mod is enabled", () => true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<NodeColorModeEnum> NODE_COLOR_MODE = new ModConfigurationKey<NodeColorModeEnum>("NODE_COLOR_MODE", "Controls which factor determines the color of nodes", () => NodeColorModeEnum.Config);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<BaseX.color> NODE_COLOR = new ModConfigurationKey<BaseX.color>("NODE_COLOR", "The color of nodes when Config mode is selected", () => new BaseX.color(1.0f, 1.0f, 1.0f, 0.8f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<BaseX.color> NODE_ERROR_COLOR = new ModConfigurationKey<BaseX.color>("NODE_ERROR_COLOR", "The color of 'broken' error nodes", () => new BaseX.color(3.0f, 0.5f, 0.5f, 0.8f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<ColorModelEnum> COLOR_MODEL = new ModConfigurationKey<ColorModelEnum>("COLOR_MODEL", "Color model", () => ColorModelEnum.RGB);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float3> RGB_CHANNEL_MULTIPLIER = new ModConfigurationKey<float3>("RGB_CHANNEL_MULTIPLIER", "RGB Channel Multiplier", () => new float3(1f, 1f, 1f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float2> HSV_SATURATION_AND_VALUE = new ModConfigurationKey<float2>("HSV_SATURATION_AND_VALUE", "HSV Saturation and Value", () => new float2(1f, 1f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float2> HSL_SATURATION_AND_LIGHTNESS = new ModConfigurationKey<float2>("HSL_SATURATION_AND_LIGHTNESS", "HSL Saturation and Lightness", () => new float2(1f, 1f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> USE_RNG = new ModConfigurationKey<bool>("USE_RNG", "Controls if the mod should use randomness to generate the colors", () => false);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> MULTIPLY_RANDOM_OUTPUT_BY_RGB = new ModConfigurationKey<bool>("MULTIPLY_RANDOM_OUTPUT_BY_RGB", "Controls if the mod should use the RGB multiplier on the random color output", () => false);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<RandomSeedEnum> RANDOM_SEED_METHOD = new ModConfigurationKey<RandomSeedEnum>("RANDOM_SEED_METHOD", "Controls how the RNG is seeded", () => RandomSeedEnum.NodeFactorHashcode);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<int> RANDOM_SEED_OFFSET = new ModConfigurationKey<int>("RANDOM_SEED_OFFSET", "Optional integer offset added to the node factor hashcode seed to get different colors", () => 0);

        // INTERNAL ACCESS CONFIG KEYS
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<int> REFID_MOD_DIVISOR = new ModConfigurationKey<int>("REFID_MOD_DIVISOR", "Modulo divisor for RefID to Color conversion", () => 100000, internalAccessOnly: true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<int> STRING_MOD_DIVISOR = new ModConfigurationKey<int>("STRING_MOD_DIVISOR", "Modulo divisor for String to Color conversion", () => 700, internalAccessOnly: true);
        // =====

        private enum RandomSeedEnum
        {
            NodeFactorHashcode,
            SystemTime
        }

        private enum ColorModelEnum
        {
            RGB,
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

        private static BaseX.color GetHueColorFromNumber(ulong val, ulong modulo)
        {
            float hue = (val % modulo) / (float)modulo;
            BaseX.color c = Config.GetValue(NODE_COLOR);
            switch (Config.GetValue(COLOR_MODEL))
            {
                case ColorModelEnum.RGB:
                    float3 multiplier = Config.GetValue(RGB_CHANNEL_MULTIPLIER);
                    c = new ColorHSV(hue, 1f, 1f, 0.8f).ToRGB();
                    c = c.MulR(multiplier.x);
                    c = c.MulG(multiplier.y);
                    c = c.MulB(multiplier.z);
                    break;
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

        // maybe remove this and just use the ulong version (can convert ints to ulong as long as they are not negative)
        private static BaseX.color GetHueColorFromNumber(int val, int modulo)
        {
            float hue = (val % modulo) / (float)modulo;
            BaseX.color c = Config.GetValue(NODE_COLOR);
            switch (Config.GetValue(COLOR_MODEL))
            {
                case ColorModelEnum.RGB:
                    float3 multiplier = Config.GetValue(RGB_CHANNEL_MULTIPLIER);
                    c = new ColorHSV(hue, 1f, 1f, 0.8f).ToRGB();
                    c = c.MulR(multiplier.x);
                    c = c.MulG(multiplier.y);
                    c = c.MulB(multiplier.z);
                    break;
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
            int val = 0;
            int[] allAscii = Enumerable.Range('\x1', 127).ToArray();
            int charIndex = 0;
            foreach (char c in str)
            {
                charIndex = allAscii.FindIndex((int i) => (char)i == c);
                if (charIndex == -1) charIndex = 0;
                val += charIndex;
            }
            //Msg($"str: {str}");
            //Msg($"val: {val.ToString()}");
            //string joined = string.Join(",", allAscii.Select(x => x.ToString()).ToArray());
            //Msg($"allAscii: {joined}");

            //allNums.Add(val);
            //Msg($"max: {allNums.Max()}");
            //Msg($"min: {allNums.Min()}");
            //Msg($"average: {allNums.Average()}");
            return GetHueColorFromNumber(val, Config.GetValue(STRING_MOD_DIVISOR));
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

                                    if (Config.GetValue(RANDOM_SEED_METHOD) == RandomSeedEnum.SystemTime && Config.GetValue(USE_RNG) == true)
                                    {
                                        rng = rngTimeSeeded;
                                    }
                                    else
                                    {
                                        switch (Config.GetValue(NODE_COLOR_MODE))
                                        {
                                            case NodeColorModeEnum.Config:
                                                colorToSet = Config.GetValue(NODE_COLOR);
                                                break;
                                            case NodeColorModeEnum.NodeName:
                                                if (Config.GetValue(USE_RNG) == true)
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
                                                if (Config.GetValue(USE_RNG))
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
                                                if (Config.GetValue(USE_RNG))
                                                {
                                                    rng = new System.Random(nodeCategoryString.GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
                                                }
                                                else
                                                {
                                                    colorToSet = GetColorFromString(nodeCategoryString);
                                                }
                                                break;
                                            case NodeColorModeEnum.FullTypeName:
                                                if (Config.GetValue(USE_RNG))
                                                {
                                                    rng = new System.Random(__instance.GetType().FullName.GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
                                                }
                                                else
                                                {
                                                    colorToSet = GetColorFromString(__instance.GetType().FullName);
                                                }
                                                break;
                                            case NodeColorModeEnum.RefID:
                                                if (Config.GetValue(USE_RNG))
                                                {
                                                    // maybe use ReferenceID.Position here instead?
                                                    rng = new System.Random(root.Parent.ReferenceID.GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
                                                }
                                                else
                                                {
                                                    colorToSet = GetHueColorFromNumber(root.Parent.ReferenceID.Position, (ulong)Config.GetValue(REFID_MOD_DIVISOR));
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
                                            case ColorModelEnum.RGB:
                                                colorToSet = new BaseX.color(rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, 0.8f);
                                                break;
                                            case ColorModelEnum.HSV:
                                                colorToSet = new ColorHSV(rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, 0.8f).ToRGB();
                                                break;
                                            case ColorModelEnum.HSL:
                                                colorToSet = new ColorHSL(rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, 0.8f).ToRGB();
                                                break;
                                            default:
                                                break;
                                        }
                                        if (Config.GetValue(MULTIPLY_RANDOM_OUTPUT_BY_RGB))
                                        {
                                            float3 multiplier = Config.GetValue(RGB_CHANNEL_MULTIPLIER);
                                            colorToSet = colorToSet.MulR(multiplier.x);
                                            colorToSet = colorToSet.MulG(multiplier.y);
                                            colorToSet = colorToSet.MulB(multiplier.z);
                                        }
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