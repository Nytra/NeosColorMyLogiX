using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.UIX;
using HarmonyLib;
using NeosModLoader;
using BaseX;
using System;
using FrooxEngine.LogiX.Math;

namespace ColorMyLogixNodes
{
    public class ColorMyLogixNodes : NeosMod
    {
        public override string Name => "ColorMyLogiXNodes";
        public override string Author => "Nytra";
        public override string Version => "1.0.0-alpha5";
        public override string Link => "https://github.com/Nytra/NeosColorMyLogiXNodes";

        private const string COLOR_SET_TAG = "ColorMyLogiXNodes.ColorSet";

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

        private static void TrySetValueFieldValue(ValueField<BaseX.color> valueField, BaseX.color color)
        {
            try
            {
                valueField.Value.Value = color;
            }
            catch (Exception e)
            {
                Error($"Error occurred while trying to set ValueField Value Value.\nError message: {e.Message}");
            }
        }

        [HarmonyPatch(typeof(LogixNode))]
        [HarmonyPatch("GenerateUI")]
        class Patch_LogixNode_GenerateUI
        {
            static void Postfix(Slot root)
            {
                if (Config.GetValue(MOD_ENABLED) == true && root != null && root.ReferenceID.User == root.LocalUser.AllocationID) // check logix node visual is allocated to the local user
                {
                    if (root.Tag != COLOR_SET_TAG)
                    {
                        var imageSlot = root.FindChild((Slot c) => c.Name == "Image");
                        if (imageSlot != null)
                        {
                            if (root.Tag == "Disabled")
                            {
                                var image = imageSlot.GetComponent<Image>();
                                if (image != null)
                                {
                                    TrySetImageTint(image, Config.GetValue(NODE_ERROR_COLOR));
                                }
                            }
                            else
                            {
                                //var valueField = root.Parent.GetComponent<ValueField<BaseX.color>>();
                                var image = imageSlot.GetComponent<Image>();
                                //if (valueField == null)
                                //{
                                if (image != null)
                                {
                                    //valueField = root.Parent.AttachComponent<ValueField<BaseX.color>>();
                                    //valueField.Persistent = false;

                                    if (Config.GetValue(USE_RANDOM_COLORS) == true)
                                    {
                                        //TrySetValueFieldValue(valueField, new BaseX.color(rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, 0.8f));
                                        TrySetImageTint(image, new BaseX.color(rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, rng.Next(101) / 100.0f, 0.8f));
                                    }
                                    else
                                    {
                                        //TrySetValueFieldValue(valueField, Config.GetValue(NODE_COLOR));
                                        TrySetImageTint(image, Config.GetValue(NODE_COLOR));
                                    }

                                    TrySetTag(imageSlot, COLOR_SET_TAG);
                                }
                                //}
                                //else
                                //{
                                    //if (image != null && image.Tint.Value != valueField.Value.Value)
                                    //{
                                        //TrySetImageTint(image, valueField.Value.Value);
                                    //}
                                //}
                            }
                        }
                    }
                }
            }
        }
    }
}