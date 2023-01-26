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

namespace ColorMyLogixNodes
{
    public class ColorMyLogixNodes : NeosMod
    {
        public override string Name => "ColorMyLogiXNodes";
        public override string Author => "Nytra";
        public override string Version => "1.0.0-alpha7.3";
        public override string Link => "https://github.com/Nytra/NeosColorMyLogiXNodes";

        public static ModConfiguration Config;

        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> MOD_ENABLED = new ModConfigurationKey<bool>("Mod enabled", "Controls whether or not the mod is enabled.", () => true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<NodeColorModeEnum> NODE_COLOR_MODE = new ModConfigurationKey<NodeColorModeEnum>("Node color mode", "Controls what factor determines the color of nodes.", () => NodeColorModeEnum.Config);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<BaseX.color> NODE_COLOR = new ModConfigurationKey<BaseX.color>("Node color", "The color of nodes when using Config mode.", () => new BaseX.color(1.0f, 1.0f, 1.0f, 0.8f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<BaseX.color> NODE_ERROR_COLOR = new ModConfigurationKey<BaseX.color>("Node error color", "The color of broken error nodes.", () => new BaseX.color(3.0f, 0.5f, 0.5f, 0.8f));
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> USE_RANDOM = new ModConfigurationKey<bool>("Use random", "Use RNG seeded by the hashcode of the node factor (Only affects RefID mode unless USE_COLOR_FROM_STRING is also enabled)", () => false);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<int> RANDOM_SEED_OFFSET = new ModConfigurationKey<int>("Random seed offset", "Optional integer offset applied to the random seed to get different colors.", () => 0);

        // INTERNAL ACCESS CONFIG KEYS
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<int> REFID_MODULO = new ModConfigurationKey<int>("REFID_MODULO", "Modulo for RefID to Color conversion", () => 100000, internalAccessOnly: true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<int> STRING_MODULO = new ModConfigurationKey<int>("STRING_MODULO", "Modulo for String to Color conversion", () => 1500, internalAccessOnly: true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<float> HUE_COLOR_MULT = new ModConfigurationKey<float>("HUE_COLOR_MULT", "Color multiplier for non-random RefID and Color from string", () => 1f, internalAccessOnly: true);
        [AutoRegisterConfigKey]
        private static ModConfigurationKey<bool> USE_COLOR_FROM_STRING = new ModConfigurationKey<bool>("USE_COLOR_FROM_STRING", "Enable color from string (experimental, requires 'Use RNG' to be disabled)", () => false, internalAccessOnly: true);
        // =====

        private enum NodeColorModeEnum
        {
            Config,
            NodeName,
            NodeCategory,
            TopmostNodeCategory,
            FullTypeName,
            RefID,
            Random
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

        private static BaseX.color GetColorFromUlong(ulong val, ulong modulo)
        {
            float hue = (val % modulo) / (float)modulo;
            return new ColorHSV(hue, 1f, 1f, 0.8f).ToRGB().MulRGB(Config.GetValue(HUE_COLOR_MULT));
        }

        private static BaseX.color GetColorFromInt(int val, int modulo)
        {
            float hue = (val % modulo) / (float)modulo;
            return new ColorHSV(hue, 1f, 1f, 0.8f).ToRGB().MulRGB(Config.GetValue(HUE_COLOR_MULT)); ;
        }

        private static BaseX.color GetColorFromString(string str)
        {
            int val = 0;
            int[] allAscii = Enumerable.Range('\x1', 127).ToArray();
            int charIndex = 0;
            foreach (char c in str)
            {
                charIndex = allAscii.FindIndex((int i) => (char)i == c);
                val += charIndex;
            }
            //Msg($"str: {str}");
            //Msg($"val: {val.ToString()}");

            //allNums.Add(val);
            //Msg($"max: {allNums.Max()}");
            //Msg($"min: {allNums.Min()}");
            //Msg($"average: {allNums.Average()}");
            return GetColorFromInt(val, Config.GetValue(STRING_MODULO));
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
                                    string categoryString;
                                    BaseX.color colorToSet = Config.GetValue(NODE_COLOR);
                                    rng = null;
                                    switch (Config.GetValue(NODE_COLOR_MODE))
                                    {
                                        case NodeColorModeEnum.Config:
                                            colorToSet = Config.GetValue(NODE_COLOR);
                                            break;
                                        case NodeColorModeEnum.NodeName:
                                            if (Config.GetValue(USE_RANDOM) || Config.GetValue(USE_COLOR_FROM_STRING) == false)
                                            {
                                                rng = new System.Random(LogixHelper.GetNodeName(__instance.GetType()).GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
                                            }
                                            else
                                            {
                                                colorToSet = GetColorFromString(LogixHelper.GetNodeName(__instance.GetType()));
                                            }
                                            break;
                                        case NodeColorModeEnum.NodeCategory:
                                            categoryString = GetNodeCategoryString(__instance.GetType());
                                            if (Config.GetValue(USE_RANDOM) || Config.GetValue(USE_COLOR_FROM_STRING) == false)
                                            {
                                                rng = new System.Random(categoryString.GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
                                            }
                                            else
                                            {
                                                colorToSet = GetColorFromString(categoryString);
                                            }
                                            break;
                                        case NodeColorModeEnum.TopmostNodeCategory:
                                            categoryString = GetNodeCategoryString(__instance.GetType(), onlyTopmost: true);
                                            if (Config.GetValue(USE_RANDOM) || Config.GetValue(USE_COLOR_FROM_STRING) == false)
                                            {
                                                rng = new System.Random(categoryString.GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
                                            }
                                            else
                                            {
                                                colorToSet = GetColorFromString(categoryString);
                                            }
                                            break;
                                        case NodeColorModeEnum.FullTypeName:
                                            if (Config.GetValue(USE_RANDOM) || Config.GetValue(USE_COLOR_FROM_STRING) == false)
                                            {
                                                rng = new System.Random(__instance.GetType().FullName.GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
                                            }
                                            else
                                            {
                                                colorToSet = GetColorFromString(__instance.GetType().FullName);
                                            }
                                            break;
                                        case NodeColorModeEnum.RefID:
                                            if (Config.GetValue(USE_RANDOM))
                                            {
                                                // maybe use ReferenceID.Position here instead?
                                                rng = new System.Random(root.Parent.ReferenceID.GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
                                            }
                                            else
                                            {
                                                colorToSet = GetColorFromUlong(root.Parent.ReferenceID.Position, (ulong)Config.GetValue(REFID_MODULO));
                                            }
                                            //Msg($"RefID Position: {root.Parent.ReferenceID.Position.ToString()}");
                                            break;
                                        case NodeColorModeEnum.Random:
                                            rng = rngTimeSeeded;
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