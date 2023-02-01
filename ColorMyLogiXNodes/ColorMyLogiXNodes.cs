//#define DEBUG

using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.UIX;
using HarmonyLib;
using NeosModLoader;
using System;
using System.Reflection;
using BaseX;
using System.Collections.Generic;
using System.Linq;

namespace ColorMyLogixNodes
{
    public class ColorMyLogixNodes : NeosMod
    {
        public override string Name => "ColorMyLogiXNodes";
        public override string Author => "Nytra";
        public override string Version => "1.0.0-alpha7.5.1";
        public override string Link => "https://github.com/Nytra/NeosColorMyLogiXNodes";

        public static ModConfiguration Config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> MOD_ENABLED = new ModConfigurationKey<bool>("MOD_ENABLED", "Mod Enabled:", () => true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> USE_STATIC_COLOR = new ModConfigurationKey<bool>("USE_STATIC_COLOR", "Use static node color:", () => false);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<BaseX.color> NODE_COLOR = new ModConfigurationKey<BaseX.color>("NODE_COLOR", "Static Node Color:", () => new BaseX.color(1.0f, 1.0f, 1.0f, 0.8f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<BaseX.color> NODE_ERROR_COLOR = new ModConfigurationKey<BaseX.color>("NODE_ERROR_COLOR", "Node Error Color:", () => new BaseX.color(3.0f, 0.5f, 0.5f, 0.8f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<NodeColorModeEnum> NODE_COLOR_MODE = new ModConfigurationKey<NodeColorModeEnum>("NODE_COLOR_MODE", "Node Factor:", () => NodeColorModeEnum.NodeCategory);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<ColorModelEnum> COLOR_MODEL = new ModConfigurationKey<ColorModelEnum>("COLOR_MODEL", "Color Model:", () => ColorModelEnum.HSV);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float2> HSV_SATURATION_AND_VALUE = new ModConfigurationKey<float2>("HSV_SATURATION_AND_VALUE", "HSV - Saturation and Value:", () => new float2(1f, 1f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float2> HSL_SATURATION_AND_LIGHTNESS = new ModConfigurationKey<float2>("HSL_SATURATION_AND_LIGHTNESS", "HSL - Saturation and Lightness:", () => new float2(1f, 1f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> USE_RANDOMNESS = new ModConfigurationKey<bool>("USE_RANDOMNESS", "Use Randomness:", () => false);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<RandomSeedEnum> RANDOM_SEED_METHOD = new ModConfigurationKey<RandomSeedEnum>("RANDOM_SEED_METHOD", "Randomness Options:", () => RandomSeedEnum.SeededByNodeFactor);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<int> RANDOM_SEED_OFFSET = new ModConfigurationKey<int>("RANDOM_SEED_OFFSET", "Node Factor Seed Offset [+-]:", () => 0);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> USE_RANDOM_SVL = new ModConfigurationKey<bool>("USE_RANDOM_SVL", "Use Random Saturation / Value / Lightness:", () => false);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<RandomSaturationValueLightnessEnum> RANDOM_SVL = new ModConfigurationKey<RandomSaturationValueLightnessEnum>("RANDOM_SVL", "Random Saturation / Value / Lightness Options:", () => RandomSaturationValueLightnessEnum.Saturation);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float3> RANDOM_STRENGTH_HSV = new ModConfigurationKey<float3>("RANDOM_STRENGTH_HSV", "Randomness Channel Strength [HSV] [*0-1]:", () => new float3(1f, 1f, 1f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float3> RANDOM_OFFSET_HSV = new ModConfigurationKey<float3>("RANDOM_OFFSET_HSV", "Randomness Channel Offset [HSV] [+0-1]:", () => new float3(0f, 0f, 0f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float3> RANDOM_STRENGTH_HSL = new ModConfigurationKey<float3>("RANDOM_STRENGTH_HSL", "Randomness Channel Strength [HSL] [*0-1]:", () => new float3(1f, 1f, 1f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float3> RANDOM_OFFSET_HSL = new ModConfigurationKey<float3>("RANDOM_OFFSET_HSL", "Randomness Channel Offset [HSL] [+0-1]:", () => new float3(0f, 0f, 0f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> MULTIPLY_OUTPUT_BY_RGB = new ModConfigurationKey<bool>("MULTIPLY_OUTPUT_BY_RGB", "Should the RGB Channel Multiplier be used on the output color:", () => false);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float3> RGB_CHANNEL_MULTIPLIER = new ModConfigurationKey<float3>("RGB_CHANNEL_MULTIPLIER", "RGB Channel Multiplier:", () => new float3(1f, 1f, 1f));

        // INTERNAL ACCESS CONFIG KEYS
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<int> REFID_MOD_DIVISOR = new ModConfigurationKey<int>("REFID_MOD_DIVISOR", "Modulo divisor for RefID to Color conversion:", () => 100000, internalAccessOnly: true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<int> STRING_MOD_DIVISOR = new ModConfigurationKey<int>("STRING_MOD_DIVISOR", "Modulo divisor for String to Color conversion:", () => 837, internalAccessOnly: true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> ALTERNATE_CATEGORY_STRING = new ModConfigurationKey<bool>("ALTERNATE_CATEGORY_STRING", "Use alternate node category string (only uses the part after the final '/'):", () => false, internalAccessOnly: true);
        // =====

        private enum RandomSaturationValueLightnessEnum
        {
            Saturation,
            Value,
            Lightness,
            SaturationValue,
            SaturationLightness,
            ValueLightness,
            SaturationValueLightness
        }

        private enum RandomSeedEnum
        {
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
            NodeName,
            NodeCategory,
            TopmostNodeCategory,
            FullTypeName,
            RefID
        }

        private static System.Random rng;
        private static readonly System.Random rngTimeSeeded = new System.Random();

        private const string COLOR_SET_TAG = "ColorMyLogiXNodes.ColorSet";

#if DEBUG
        private static List<int> debugAllNums = new List<int>();
        private static List<int> debugAllNumsNoRelay = new List<int>();
#endif

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
                            if (Config.GetValue(ALTERNATE_CATEGORY_STRING))
                            {
                                return parts[1];
                            }
                            else
                            {
                                return parts[0] + "/" + parts[1];
                            }
                        }
                        else
                        {
                            return parts[0];
                        }
                    }
                    else
                    {
                        if (Config.GetValue(ALTERNATE_CATEGORY_STRING))
                        {
                            string[] parts = categoryPaths[0].Split('/');
                            return parts[parts.Length - 1];
                        }
                        else
                        {
                            return categoryPaths[0];
                        }
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
            int val = 0;
            foreach (char c in str)
            {
                val += (int)c;
            }
#if DEBUG
            Msg($"str: {str}");
            Msg($"val: {val.ToString()}");
            debugAllNums.Add(val);
            if (str != "Relay") debugAllNumsNoRelay.Add(val);
            Msg($"max: {debugAllNums.Max()}");
            Msg($"min: {debugAllNums.Min()}");
            Msg($"average: {debugAllNums.Average()}");
            Msg($"maxNR: {debugAllNumsNoRelay.Max()}");
            Msg($"minNR: {debugAllNumsNoRelay.Min()}");
            Msg($"averageNR: {debugAllNumsNoRelay.Average()}");
#endif

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
                                    if (Config.GetValue(USE_STATIC_COLOR) == true)
                                    {
                                        colorToSet = Config.GetValue(NODE_COLOR);
                                    }
                                    else if (Config.GetValue(USE_RANDOMNESS) == true && Config.GetValue(RANDOM_SEED_METHOD) == RandomSeedEnum.SeededBySystemTime)
                                    {
                                        rng = rngTimeSeeded;
                                    }
                                    else
                                    {
                                        switch (Config.GetValue(NODE_COLOR_MODE))
                                        {
                                            case NodeColorModeEnum.NodeName:
                                                if (Config.GetValue(USE_RANDOMNESS) == true && Config.GetValue(RANDOM_SEED_METHOD) == RandomSeedEnum.SeededByNodeFactor)
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
                                                if (Config.GetValue(USE_RANDOMNESS) == true && Config.GetValue(RANDOM_SEED_METHOD) == RandomSeedEnum.SeededByNodeFactor)
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
                                                if (Config.GetValue(USE_RANDOMNESS) == true && Config.GetValue(RANDOM_SEED_METHOD) == RandomSeedEnum.SeededByNodeFactor)
                                                {
                                                    rng = new System.Random(nodeCategoryString.GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
                                                }
                                                else
                                                {
                                                    colorToSet = GetColorFromString(nodeCategoryString);
                                                }
                                                break;
                                            case NodeColorModeEnum.FullTypeName:
                                                if (Config.GetValue(USE_RANDOMNESS) == true && Config.GetValue(RANDOM_SEED_METHOD) == RandomSeedEnum.SeededByNodeFactor)
                                                {
                                                    rng = new System.Random(__instance.GetType().FullName.GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
                                                }
                                                else
                                                {
                                                    colorToSet = GetColorFromString(__instance.GetType().FullName);
                                                }
                                                break;
                                            case NodeColorModeEnum.RefID:
                                                if (Config.GetValue(USE_RANDOMNESS) == true && Config.GetValue(RANDOM_SEED_METHOD) == RandomSeedEnum.SeededByNodeFactor)
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
                                        bool useRandomSVL = Config.GetValue(USE_RANDOM_SVL);
                                        RandomSaturationValueLightnessEnum randomSVL = Config.GetValue(RANDOM_SVL);
                                        float2 hsv_values = Config.GetValue(HSV_SATURATION_AND_VALUE);
                                        float2 hsl_values = Config.GetValue(HSL_SATURATION_AND_LIGHTNESS);
                                        float3 random_strength_hsv = Config.GetValue(RANDOM_STRENGTH_HSV);
                                        float3 random_offset_hsv = Config.GetValue(RANDOM_OFFSET_HSV);
                                        float3 random_strength_hsl = Config.GetValue(RANDOM_STRENGTH_HSL);
                                        float3 random_offset_hsl = Config.GetValue(RANDOM_OFFSET_HSL);
                                        float hue;
                                        float saturation;
                                        float value;
                                        float lightness;
                                        switch (Config.GetValue(COLOR_MODEL))
                                        {
                                            case ColorModelEnum.HSV:

                                                hue = (rng.Next(101) / 100.0f) * random_strength_hsv.x + random_offset_hsv.x;

                                                switch (useRandomSVL)
                                                {
                                                    case false:
                                                        colorToSet = new ColorHSV(hue, hsv_values.x, hsv_values.y, 0.8f).ToRGB();
                                                        break;
                                                    case true:
                                                        switch (randomSVL)
                                                        {
                                                            case RandomSaturationValueLightnessEnum.Saturation:
                                                                saturation = (rng.Next(101) / 100.0f) * random_strength_hsv.y + random_offset_hsv.y;
                                                                colorToSet = new ColorHSV(hue, saturation, hsv_values.y, 0.8f).ToRGB();
                                                                break;
                                                            case RandomSaturationValueLightnessEnum.Value:
                                                                value = (rng.Next(101) / 100.0f) * random_strength_hsv.z + random_offset_hsv.z;
                                                                colorToSet = new ColorHSV(hue, hsv_values.x, value, 0.8f).ToRGB();
                                                                break;
                                                            case RandomSaturationValueLightnessEnum.SaturationValue:
                                                                saturation = (rng.Next(101) / 100.0f) * random_strength_hsv.y + random_offset_hsv.y;
                                                                value = (rng.Next(101) / 100.0f) * random_strength_hsv.z + random_offset_hsv.z;
                                                                colorToSet = new ColorHSV(hue, saturation, value, 0.8f).ToRGB();
                                                                break;
                                                            case RandomSaturationValueLightnessEnum.SaturationValueLightness:
                                                                saturation = (rng.Next(101) / 100.0f) * random_strength_hsv.y + random_offset_hsv.y;
                                                                value = (rng.Next(101) / 100.0f) * random_strength_hsv.z + random_offset_hsv.z;
                                                                colorToSet = new ColorHSV(hue, saturation, value, 0.8f).ToRGB();
                                                                break;
                                                        }
                                                        break;
                                                }
                                                break;
                                            case ColorModelEnum.HSL:

                                                hue = (rng.Next(101) / 100.0f) * random_strength_hsl.x + random_offset_hsl.x;

                                                switch (useRandomSVL)
                                                {
                                                    case false:
                                                        colorToSet = new ColorHSL(hue, hsl_values.x, hsl_values.y, 0.8f).ToRGB();
                                                        break;
                                                    case true:
                                                        switch (randomSVL)
                                                        {
                                                            case RandomSaturationValueLightnessEnum.Saturation:
                                                                saturation = (rng.Next(101) / 100.0f) * random_strength_hsl.y + random_offset_hsl.y;
                                                                colorToSet = new ColorHSL(hue, saturation, hsl_values.y, 0.8f).ToRGB();
                                                                break;
                                                            case RandomSaturationValueLightnessEnum.Lightness:
                                                                lightness = (rng.Next(101) / 100.0f) * random_strength_hsl.z + random_offset_hsl.z;
                                                                colorToSet = new ColorHSL(hue, hsl_values.x, lightness, 0.8f).ToRGB();
                                                                break;
                                                            case RandomSaturationValueLightnessEnum.SaturationLightness:
                                                                saturation = (rng.Next(101) / 100.0f) * random_strength_hsl.y + random_offset_hsl.y;
                                                                lightness = (rng.Next(101) / 100.0f) * random_strength_hsl.z + random_offset_hsl.z;
                                                                colorToSet = new ColorHSL(hue, saturation, lightness, 0.8f).ToRGB();
                                                                break;
                                                            case RandomSaturationValueLightnessEnum.SaturationValueLightness:
                                                                saturation = (rng.Next(101) / 100.0f) * random_strength_hsl.y + random_offset_hsl.y;
                                                                lightness = (rng.Next(101) / 100.0f) * random_strength_hsl.z + random_offset_hsl.z;
                                                                colorToSet = new ColorHSL(hue, saturation, lightness, 0.8f).ToRGB();
                                                                break;
                                                        }
                                                        break;
                                                }
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