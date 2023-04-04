//#define DEBUG

using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.UIX;
using HarmonyLib;
using NeosModLoader;
using System;
using System.Reflection;
using BaseX;
using System.Runtime.CompilerServices;
//using Leap;

#if DEBUG
using System.Collections.Generic;
#endif

namespace ColorMyLogixNodes
{
	
	public class ColorMyLogixNodes : NeosMod
	{
		public override string Name => "ColorMyLogiX";
		public override string Author => "Nytra";
		public override string Version => "1.0.0-alpha8";
		public override string Link => "https://github.com/Nytra/NeosColorMyLogiX";

		const string SEP_STRING = "·";

		public static ModConfiguration Config;

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> MOD_ENABLED = new ModConfigurationKey<bool>("MOD_ENABLED", "Mod Enabled:", () => true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_1", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_STATIC_COLOR = new ModConfigurationKey<bool>("USE_STATIC_COLOR", "Use Static Node Color:", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<BaseX.color> NODE_COLOR = new ModConfigurationKey<BaseX.color>("NODE_COLOR", "Static Node Color:", () => new BaseX.color(1.0f, 1.0f, 1.0f, 0.8f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<BaseX.color> NODE_ERROR_COLOR = new ModConfigurationKey<BaseX.color>("NODE_ERROR_COLOR", "Node Error Color:", () => new BaseX.color(3.0f, 0.5f, 0.5f, 0.8f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_1_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_1_1", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<NodeColorModeEnum> NODE_COLOR_MODE = new ModConfigurationKey<NodeColorModeEnum>("NODE_COLOR_MODE", "Which Node Factor Determines Color:", () => NodeColorModeEnum.NodeCategory);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_2 = new ModConfigurationKey<dummy>("DUMMY_SEP_2", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<ColorModelEnum> COLOR_MODEL = new ModConfigurationKey<ColorModelEnum>("COLOR_MODEL", "Color Model:", () => ColorModelEnum.HSV);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_2_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_2_1", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_HUE_FROM_STATIC_NODE_COLOR = new ModConfigurationKey<bool>("USE_HUE_FROM_STATIC_NODE_COLOR", "Use Static Hue from Static Node Color:", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float> RANDOM_RANGE_AROUND_STATIC_HUE = new ModConfigurationKey<float>("RANDOM_RANGE_AROUND_STATIC_HUE", "Random Range around Static Hue [0-1]:", () => 0.1f);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_3 = new ModConfigurationKey<dummy>("DUMMY_SEP_3", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<int> RANDOM_SEED_OFFSET = new ModConfigurationKey<int>("RANDOM_SEED_OFFSET", "Random Seed:", () => 0);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_4 = new ModConfigurationKey<dummy>("DUMMY_SEP_4", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_4_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_4_1", "These control the upper and lower bounds for the color channels: Hue, Saturation and Value/Lightness", () => new dummy());

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> COLOR_CHANNELS_MAX = new ModConfigurationKey<float3>("COLOR_CHANNELS_MAX", "Color Channels Max [0-1]:", () => new float3(1f, 0.5f, 1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> COLOR_CHANNELS_MIN = new ModConfigurationKey<float3>("COLOR_CHANNELS_MIN", "Color Channels Min [0-1]:", () => new float3(0f, 0.5f, 1f));

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_5 = new ModConfigurationKey<dummy>("DUMMY_SEP_5", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> MULTIPLY_OUTPUT_BY_RGB = new ModConfigurationKey<bool>("MULTIPLY_OUTPUT_BY_RGB", "Use Output RGB Channel Multiplier:", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> RGB_CHANNEL_MULTIPLIER = new ModConfigurationKey<float3>("RGB_CHANNEL_MULTIPLIER", "Output RGB Channel Multiplier:", () => new float3(1f, 1f, 1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_5_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_1", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> COLOR_NULL_REFERENCE_NODES = new ModConfigurationKey<bool>("COLOR_NULL_REFERENCE_NODES", "Should Null Reference Nodes use Node Error Color:", () => true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> COLOR_NULL_DRIVER_NODES = new ModConfigurationKey<bool>("COLOR_NULL_DRIVER_NODES", "Should Null Driver Nodes use Node Error Color:", () => true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ENABLE_NON_RANDOM_REFID = new ModConfigurationKey<bool>("ENABLE_NON_RANDOM_REFID", "Convert RefID to Color without randomness (Hue-shift effect, selected Node Factor must be RefID):", () => false);

		// INTERNAL ACCESS CONFIG KEYS
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<int> REFID_MOD_DIVISOR = new ModConfigurationKey<int>("REFID_MOD_DIVISOR", "Modulo divisor for RefID to Color conversion:", () => 100000, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ALTERNATE_CATEGORY_STRING = new ModConfigurationKey<bool>("ALTERNATE_CATEGORY_STRING", "Use alternate node category string (only uses the part after the final '/'):", () => false, internalAccessOnly: true);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<bool> USE_SYSTEM_TIME_RNG = new ModConfigurationKey<bool>("USE_SYSTEM_TIME_RNG", "Use system time as the seed for RNG (This is bad):", () => false, internalAccessOnly: true);

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
		private const string DELEGATE_ADDED_TAG = "ColorMyLogiXNodes.DelegateAdded";

        public class SyncRefChangedHandler
        {
            public string targetField;
            public LogixNode node;
			//private IWorldElement _lastWorldElement;
            public void OnSyncRefChanged(IWorldElement worldElement)
            {
				//this._lastWorldElement = worldElement;
				node.RunInUpdates(0, () =>
				{
                    var targetSyncRef = node.TryGetField(targetField) as ISyncRef;
                    //Msg($"Node {node.Name} {node.ReferenceID.ToString()} SyncRef Target changed.");
                    //if (targetSyncRef == null) return;
                    //worldElement.
                    if (targetSyncRef.Target == null)
                    {
                        //Msg("Null syncref target found!");
                        var imageSlot1 = node.ActiveVisual.FindChild((Slot c) => c.Name == "Image");
                        if (imageSlot1 != null)
                        {
                            var image1 = imageSlot1.GetComponent<Image>();
                            if (image1 != null)
                            {
                                TrySetImageTint(image1, Config.GetValue(NODE_ERROR_COLOR));
                                //TrySetTag(visualSlot, COLOR_SET_TAG);
                                var imageSlot2 = imageSlot1.FindChild((Slot c) => c.Name == "Image");
                                if (imageSlot2 != null)
                                {
                                    var image2 = imageSlot2.GetComponent<Image>();
                                    if (image2 != null)
                                    {
                                        TrySetImageTint(image2, Config.GetValue(NODE_ERROR_COLOR));
                                        //TrySetTag(visualSlot, COLOR_SET_TAG);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        //Msg($"SyncRef Target not null: {targetSyncRef.Target.ToString()}");
                        var imageSlot1 = node.ActiveVisual.FindChild((Slot c) => c.Name == "Image");
                        if (imageSlot1 != null)
                        {
                            var image1 = imageSlot1.GetComponent<Image>();
                            if (image1 != null)
                            {
                                var defaultColor = LogixHelper.GetColor(node.GetType().GetGenericArguments()[0]);
                                defaultColor = defaultColor.SetA(0.8f);
                                TrySetImageTint(image1, defaultColor);
                                //TrySetTag(visualSlot, COLOR_SET_TAG);
                                var imageSlot2 = imageSlot1.FindChild((Slot c) => c.Name == "Image");
                                if (imageSlot2 != null)
                                {
                                    var image2 = imageSlot2.GetComponent<Image>();
                                    if (image2 != null)
                                    {
                                        TrySetImageTint(image2, defaultColor);
                                        //TrySetTag(visualSlot, COLOR_SET_TAG);
                                    }
                                }
                            }
                        }
                    }
                });
                
            }
        };

        //static List<SyncRefChangedHandler> handlers = new();

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

		private static string GetNodeCategoryString(Type logixType, bool onlyTopmost = false)
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

		private static float GetRandomColorChannelValue(int index, Random rand)
		{
			float3 mins = Config.GetValue(COLOR_CHANNELS_MIN);
			float3 maxs = Config.GetValue(COLOR_CHANNELS_MAX);
			float3 random_strength = MathX.Abs(maxs - mins);
			float3 random_offset = mins;
			return (rand.Next(101) / 100.0f) * random_strength[index] + random_offset[index];
		}

		private static BaseX.color GetHueColorFromUlong(ulong val, ulong divisor)
		{
			float hue = 0.0f;
			if (Config.GetValue(USE_HUE_FROM_STATIC_NODE_COLOR))
			{
				ColorHSV colorHSV = new ColorHSV(Config.GetValue(NODE_COLOR));
				hue = colorHSV.h;
				int coinflip = rngTimeSeeded.Next(2) == 0 ? -1 : 1;
				hue = hue + (rngTimeSeeded.Next(101) / 100.0f) * Config.GetValue(RANDOM_RANGE_AROUND_STATIC_HUE) * coinflip / 2;
			}
			else
			{
				hue = (val % divisor) / (float)divisor;
			}
			BaseX.color c = Config.GetValue(NODE_COLOR);
			switch (Config.GetValue(COLOR_MODEL))
			{
				case ColorModelEnum.HSV:
					c = new ColorHSV(hue, GetRandomColorChannelValue(1, rngTimeSeeded), GetRandomColorChannelValue(2, rngTimeSeeded), 0.8f).ToRGB();
					break;
				case ColorModelEnum.HSL:
					c = new ColorHSL(hue, GetRandomColorChannelValue(1, rngTimeSeeded), GetRandomColorChannelValue(2, rngTimeSeeded), 0.8f).ToRGB();
					break;
				default:
					break;
			}
			return c;
		}

		[HarmonyPatch(typeof(LogixNode))]
		[HarmonyPatch("GenerateVisual")]
		class Patch_LogixNode_GenerateVisual
		{
			static void Postfix(LogixNode __instance)
			{
				if (Config.GetValue(MOD_ENABLED) == true && __instance.ActiveVisual != null && __instance.ActiveVisual.ReferenceID.User == __instance.LocalUser.AllocationID)
				{
					string targetField = null;
					if (Config.GetValue(COLOR_NULL_REFERENCE_NODES) == true && __instance.Name.Contains("ReferenceNode"))
					{
						targetField = "RefTarget";
					}
					else if (Config.GetValue(COLOR_NULL_DRIVER_NODES) == true && __instance.Name.Contains("DriverNode"))
					{
						targetField = "DriveTarget";
					}
					if (targetField != null && __instance.ActiveVisual.Tag != DELEGATE_ADDED_TAG)
					{
						__instance.RunInUpdates(0, () =>
						{
							if (__instance.ActiveVisual != null)
							{
								var targetSyncRef = __instance.TryGetField(targetField) as ISyncRef;
								if (targetSyncRef == null) return;

                                SyncRefChangedHandler handler = new();
								handler.node = __instance;
								handler.targetField = targetField;

								//Msg($"Creating handler for node {__instance.Name} {__instance.ReferenceID.ToString()}");

								targetSyncRef.Changed += (worldElement) =>
								{
									handler.OnSyncRefChanged(worldElement);
								};

								//handlers.Add(handler);

								//foreach (var elem in handlers)
								//{
								//	Msg($"Handler for node: {elem.node.Name.ToString()} {elem.node.ReferenceID.ToString()}");
								//}

								//__instance.Destroyed += (worker) =>
								//{
								//	handlers.Remove(handler);
								//};

								TrySetTag(__instance.ActiveVisual, DELEGATE_ADDED_TAG);

								//if (targetSyncRef.Target == null)
								//{
								//	//Msg("Null syncref target found!");
								//	var imageSlot1 = __instance.ActiveVisual.FindChild((Slot c) => c.Name == "Image");
								//	if (imageSlot1 != null)
								//	{
								//		var image1 = imageSlot1.GetComponent<Image>();
								//		if (image1 != null)
								//		{
								//			TrySetImageTint(image1, Config.GetValue(NODE_ERROR_COLOR));
								//			//TrySetTag(visualSlot, COLOR_SET_TAG);
								//			var imageSlot2 = imageSlot1.FindChild((Slot c) => c.Name == "Image");
								//			if (imageSlot2 != null)
								//			{
								//				var image2 = imageSlot2.GetComponent<Image>();
								//				if (image2 != null)
								//				{
								//					TrySetImageTint(image2, Config.GetValue(NODE_ERROR_COLOR));
								//					//TrySetTag(visualSlot, COLOR_SET_TAG);
								//				}
								//			}
								//		}
								//	}
								//}
								//else
								//{
								//	var imageSlot1 = __instance.ActiveVisual.FindChild((Slot c) => c.Name == "Image");
								//	if (imageSlot1 != null)
								//	{
								//		var image1 = imageSlot1.GetComponent<Image>();
								//		if (image1 != null)
								//		{
								//			var defaultColor = LogixHelper.GetColor(__instance.GetType().GetGenericArguments()[0]);
								//			defaultColor = defaultColor.SetA(0.8f);
								//			TrySetImageTint(image1, defaultColor);
								//			//TrySetTag(visualSlot, COLOR_SET_TAG);
								//			var imageSlot2 = imageSlot1.FindChild((Slot c) => c.Name == "Image");
								//			if (imageSlot2 != null)
								//			{
								//				var image2 = imageSlot2.GetComponent<Image>();
								//				if (image2 != null)
								//				{
								//					TrySetImageTint(image2, defaultColor);
								//					//TrySetTag(visualSlot, COLOR_SET_TAG);
								//				}
								//			}
								//		}
								//	}
								//}
							}
						});
					}
				}
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
									//else if (Config.GetValue(USE_SYSTEM_TIME_RNG) == true)
									//{
									//	rng = rngTimeSeeded;
									//}
									else
									{
										switch (Config.GetValue(NODE_COLOR_MODE))
										{
											case NodeColorModeEnum.NodeName:
												rng = new System.Random(LogixHelper.GetNodeName(__instance.GetType()).GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
												break;
											case NodeColorModeEnum.NodeCategory:
												nodeCategoryString = GetNodeCategoryString(__instance.GetType());
												rng = new System.Random(nodeCategoryString.GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
												break;
											case NodeColorModeEnum.TopmostNodeCategory:
												nodeCategoryString = GetNodeCategoryString(__instance.GetType(), onlyTopmost: true);
												rng = new System.Random(nodeCategoryString.GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
												break;
											case NodeColorModeEnum.FullTypeName:
												rng = new System.Random(__instance.GetType().FullName.GetHashCode() + Config.GetValue(RANDOM_SEED_OFFSET));
												break;
											case NodeColorModeEnum.RefID:
												if (Config.GetValue(ENABLE_NON_RANDOM_REFID) == false)
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
										float hue;
										switch (Config.GetValue(COLOR_MODEL))
										{
											case ColorModelEnum.HSV:

												if (Config.GetValue(USE_HUE_FROM_STATIC_NODE_COLOR))
												{
													ColorHSV colorHSV = new ColorHSV(Config.GetValue(NODE_COLOR));
													hue = colorHSV.h;
													int coinflip = rngTimeSeeded.Next(2) == 0 ? -1 : 1;
													hue = hue + (rngTimeSeeded.Next(101) / 100.0f) * Config.GetValue(RANDOM_RANGE_AROUND_STATIC_HUE) * coinflip / 2;
												}
												else
												{
													hue = GetRandomColorChannelValue(0, rng);
												}
												colorToSet = new ColorHSV(hue, GetRandomColorChannelValue(1, rng), GetRandomColorChannelValue(2, rng), 0.8f).ToRGB();
												//var cHsv = new ColorHSV(colorToSet);
												//Msg($"{cHsv.h.ToString()} {cHsv.s.ToString()} {cHsv.v.ToString()}");
												break;
											case ColorModelEnum.HSL:

												if (Config.GetValue(USE_HUE_FROM_STATIC_NODE_COLOR))
												{
													ColorHSL colorHSL = new ColorHSL(Config.GetValue(NODE_COLOR));
													hue = colorHSL.h;
													int coinflip = rngTimeSeeded.Next(2) == 0 ? -1 : 1;
													hue = hue + (rngTimeSeeded.Next(101) / 100.0f) * Config.GetValue(RANDOM_RANGE_AROUND_STATIC_HUE) * coinflip / 2;
												}
												else
												{
													hue = GetRandomColorChannelValue(0, rng);
												}

												colorToSet = new ColorHSL(hue, GetRandomColorChannelValue(1, rng), GetRandomColorChannelValue(2, rng), 0.8f).ToRGB();
												//var cHsl = new ColorHSL(colorToSet);
												//Msg($"{cHsl.h.ToString()} {cHsl.s.ToString()} {cHsl.l.ToString()}");
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