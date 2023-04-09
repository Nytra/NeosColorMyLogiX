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

#if DEBUG

#endif

namespace ColorMyLogixNodes
{
	public class ColorMyLogixNodes : NeosMod
	{
		public override string Name => "ColorMyLogiX";
		public override string Author => "Nytra";
		public override string Version => "1.1.0";
		public override string Link => "https://github.com/Nytra/NeosColorMyLogiX";

		const string SEP_STRING = "<size=0></size>";
		const string DETAIL_TEXT_COLOR = "gray";
		const string HEADER_TEXT_COLOR = "green";
		
		public static ModConfiguration Config;

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> MOD_ENABLED = new ModConfigurationKey<bool>("MOD_ENABLED", "Mod Enabled:", () => true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_0 = new ModConfigurationKey<dummy>("DUMMY_SEP_0", SEP_STRING, () => new dummy());
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_0_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_0_1", $"<color={HEADER_TEXT_COLOR}>[COLOR MODEL]</color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<ColorModelEnum> COLOR_MODEL = new ModConfigurationKey<ColorModelEnum>("COLOR_MODEL", "Selected Color Model:", () => ColorModelEnum.HSV);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_0_2 = new ModConfigurationKey<dummy>("DUMMY_SEP_0_2", $"<color={DETAIL_TEXT_COLOR}><i>HSV: Hue, Saturation and Value</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_0_3 = new ModConfigurationKey<dummy>("DUMMY_SEP_0_3", $"<color={DETAIL_TEXT_COLOR}><i>HSL: Hue, Saturation and Lightness</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_0_4 = new ModConfigurationKey<dummy>("DUMMY_SEP_0_4", $"<color={DETAIL_TEXT_COLOR}><i>RGB: Red, Green and Blue</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_1", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_1_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_1_1", $"<color={HEADER_TEXT_COLOR}>[STATIC]</color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_STATIC_COLOR = new ModConfigurationKey<bool>("USE_STATIC_COLOR", "Use Static Node Color (Disables the dynamic section):", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<BaseX.color> NODE_COLOR = new ModConfigurationKey<BaseX.color>("NODE_COLOR", "Static Node Color:", () => new BaseX.color(0.5f, 1.0f, 1.0f, 0.8f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_STATIC_RANGES = new ModConfigurationKey<bool>("USE_STATIC_RANGES", "Use Random Ranges around Static Node Color:", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> RANDOM_RANGES_AROUND_STATIC_VALUES = new ModConfigurationKey<float3>("RANDOM_RANGES_AROUND_STATIC_VALUES", "Random Ranges around Static Node Color [0 to 1]:", () => new float3(0.1f, 0.1f, 0.1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_1_2 = new ModConfigurationKey<dummy>("DUMMY_SEP_1_2", $"<color={DETAIL_TEXT_COLOR}><i>These ranges are for the channels of the Selected Color Model</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_1_3 = new ModConfigurationKey<dummy>("DUMMY_SEP_1_3", $"<color={DETAIL_TEXT_COLOR}><i>Channels with negative ranges will always get their values from the dynamic section</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_2 = new ModConfigurationKey<dummy>("DUMMY_SEP_2", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_2_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_2_1", $"<color={HEADER_TEXT_COLOR}>[DYNAMIC]</color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<NodeColorModeEnum> NODE_COLOR_MODE = new ModConfigurationKey<NodeColorModeEnum>("NODE_COLOR_MODE", "Selected Node Factor:", () => NodeColorModeEnum.NodeCategory);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<int> RANDOM_SEED = new ModConfigurationKey<int>("RANDOM_SEED", "Seed:", () => 0);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> COLOR_CHANNELS_MAX = new ModConfigurationKey<float3>("COLOR_CHANNELS_MAX", "Random Max [0 to 1]:", () => new float3(1f, 0.5f, 1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> COLOR_CHANNELS_MIN = new ModConfigurationKey<float3>("COLOR_CHANNELS_MIN", "Random Min [0 to 1]:", () => new float3(0f, 0.5f, 1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_2_2 = new ModConfigurationKey<dummy>("DUMMY_SEP_2_2", $"<color={DETAIL_TEXT_COLOR}><i>Maximum and minimum bounds for randomness in the channels of the Selected Color Model</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_2_3 = new ModConfigurationKey<dummy>("DUMMY_SEP_2_3", $"<color={DETAIL_TEXT_COLOR}><i>The randomness in this section is affected by the Selected Node Factor plus the Seed</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_4 = new ModConfigurationKey<dummy>("DUMMY_SEP_4", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_4_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_4_1", $"<color={HEADER_TEXT_COLOR}>[EXTRA]</color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> MULTIPLY_OUTPUT_BY_RGB = new ModConfigurationKey<bool>("MULTIPLY_OUTPUT_BY_RGB", "Use Output RGB Channel Multiplier:", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> RGB_CHANNEL_MULTIPLIER = new ModConfigurationKey<float3>("RGB_CHANNEL_MULTIPLIER", "Output RGB Channel Multiplier:", () => new float3(1f, 1f, 1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ENABLE_NON_RANDOM_REFID = new ModConfigurationKey<bool>("ENABLE_NON_RANDOM_REFID", "Enable Hue-shift Mode (HSV and HSL only):", () => false);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<int3> NON_RANDOM_REFID_CHANNELS = new ModConfigurationKey<int3>("NON_RANDOM_REFID_CHANNELS", "Which channels to shift [1 to enable, 0 to disable]:", () => new int3(1, 0, 0));
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<float3> NON_RANDOM_REFID_OFFSETS = new ModConfigurationKey<float3>("NON_RANDOM_REFID_OFFSETS", "Channel Shift Offsets [-1 to 1]:", () => new float3(0f, 0f, 0f));
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<ChannelShiftWaveformEnum> NON_RANDOM_REFID_WAVEFORM = new ModConfigurationKey<ChannelShiftWaveformEnum>("NON_RANDOM_REFID_WAVEFORM", "Channel Shift Waveform:", () => ChannelShiftWaveformEnum.Sawtooth);
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_4_2 = new ModConfigurationKey<dummy>("DUMMY_SEP_4_2", $"<color={DETAIL_TEXT_COLOR}><i>Channel Shift will make the channel values go from zero to one over time as the selected waveform</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<BaseX.color> NODE_ERROR_COLOR = new ModConfigurationKey<BaseX.color>("NODE_ERROR_COLOR", "Node Error Color:", () => new BaseX.color(3.0f, 0.5f, 0.5f, 0.8f));

		// INTERNAL ACCESS CONFIG KEYS
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> COLOR_NULL_REFERENCE_NODES = new ModConfigurationKey<bool>("COLOR_NULL_REFERENCE_NODES", "Should Null Reference Nodes use Node Error Color:", () => true, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> COLOR_NULL_DRIVER_NODES = new ModConfigurationKey<bool>("COLOR_NULL_DRIVER_NODES", "Should Null Driver Nodes use Node Error Color:", () => true, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<int> REFID_MOD_DIVISOR = new ModConfigurationKey<int>("REFID_MOD_DIVISOR", "RefID divisor for Channel Shift (Smaller value = faster shifting, minimum 1):", () => 100000, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ALTERNATE_CATEGORY_STRING = new ModConfigurationKey<bool>("ALTERNATE_CATEGORY_STRING", "Use alternate node category string (only uses the part after the final '/'):", () => false, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_SYSTEM_TIME_RNG = new ModConfigurationKey<bool>("USE_SYSTEM_TIME_RNG", "Always use randomness seeded by system time (Complete randomness, not suitable for normal use):", () => false, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<StaticRangeModeEnum> STATIC_RANGE_MODE = new ModConfigurationKey<StaticRangeModeEnum>("STATIC_RANGE_MODE", "Seed for Random Ranges around Static Node Color:", () => StaticRangeModeEnum.SystemTime, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ALLOW_NEGATIVE_AND_EMISSIVE_COLORS = new ModConfigurationKey<bool>("ALLOW_NEGATIVE_AND_EMISSIVE_COLORS", "Allow negative and emissive colors:", () => false, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ENABLE_TEXT_CONTRAST = new ModConfigurationKey<bool>("ENABLE_TEXT_CONTRAST", "Make node text color contrast with the node background color:", () => true, internalAccessOnly: true);

		private enum ColorModelEnum
		{
			HSV,
			HSL,
			RGB
		}

		private enum NodeColorModeEnum
		{
			NodeName,
			NodeCategory,
			TopmostNodeCategory,
			FullTypeName,
			RefID
		}

		private enum StaticRangeModeEnum
		{
			NodeFactor,
			SystemTime
		}

		//private enum ChannelShiftWaveformEnum
		//{
		//	Sawtooth,
		//	Sine
		//}

		private static System.Random rng;
		private static System.Random rngTimeSeeded = new System.Random();

		private const string COLOR_SET_TAG = "ColorMyLogiXNodes.ColorSet";
		private const string DELEGATE_ADDED_TAG = "ColorMyLogiXNodes.DelegateAdded";

		static Dictionary<LogixNode, HashSet<IWorldElement>> nodeElementMap = new();

		public override void OnEngineInit()
		{
			Harmony harmony = new Harmony($"owo.{Author}.{Name}");
			Config = GetConfiguration()!;
			Config.Save(true);
			harmony.PatchAll();
		}

		private static void TrySetSlotTag(Slot s, string tag)
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
				if (image.Tint.IsDriven)
				{
					image.Tint.ReleaseLink(image.Tint.ActiveLink);
				}
				image.Tint.Value = color;
			}
			catch (Exception e)
			{
				Error($"Error occurred while trying to set Image Tint Value.\nError message: {e.Message}");
			}
		}

		private static void TrySetTextColor(Text text, BaseX.color color)
		{
			try
			{
				if (text.Color.IsDriven)
				{
					text.Color.ReleaseLink(text.Color.ActiveLink);
				}
				text.Color.Value = color;
			}
			catch (Exception e)
			{
				Error($"Error occurred while trying to set Text Color Value.\nError message: {e.Message}");
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

		private static float GetStaticColorChannelValue(int index, ColorModelEnum model, ref Random rand)
		{
			float val = 0;
			int coinflip = 0;
			switch (model)
			{
				case ColorModelEnum.HSV:
					ColorHSV colorHSV = new ColorHSV(Config.GetValue(NODE_COLOR));
					switch (index)
					{
						case 0:
							val = colorHSV.h;
							break;
						case 1:
							val = colorHSV.s;
							break;
						case 2:
							val = colorHSV.v;
							break;
						default:
							break;
					}
					break;
				case ColorModelEnum.HSL:
					ColorHSL colorHSL = new ColorHSL(Config.GetValue(NODE_COLOR));
					switch (index)
					{
						case 0:
							val = colorHSL.h;
							break;
						case 1:
							val = colorHSL.s;
							break;
						case 2:
							val = colorHSL.l;
							break;
						default:
							break;
					}
					break;
				case ColorModelEnum.RGB:
					color colorRGB = new color(Config.GetValue(NODE_COLOR));
					switch (index)
					{
						case 0:
							val = colorRGB.r;
							break;
						case 1:
							val = colorRGB.g;
							break;
						case 2:
							val = colorRGB.b;
							break;
						default:
							break;
					}
					break;
				default:
					break;
			}
			if (Config.GetValue(USE_STATIC_RANGES))
			{
				float range = Config.GetValue(RANDOM_RANGES_AROUND_STATIC_VALUES)[index];
				if (range >= 0)
				{
					switch (Config.GetValue(STATIC_RANGE_MODE))
					{
						case StaticRangeModeEnum.NodeFactor:
							coinflip = rand.Next(2) == 0 ? -1 : 1;
							val += (rand.Next(101) / 100.0f) * range * (float)coinflip / 2f;
							break;
						case StaticRangeModeEnum.SystemTime:
							coinflip = rngTimeSeeded.Next(2) == 0 ? -1 : 1;
							val += (rngTimeSeeded.Next(101) / 100.0f) * range * (float)coinflip / 2f;
							break;
						default:
							break;
					}
				}
				else
				{
					val = GetRandomColorChannelValue(index, ref rand);
				}
			}
			return val;
		}

		private static float GetRandomColorChannelValue(int index, ref Random rand)
		{
			float3 mins = Config.GetValue(COLOR_CHANNELS_MIN);
			float3 maxs = Config.GetValue(COLOR_CHANNELS_MAX);
			float3 random_strength = MathX.Abs(maxs - mins);
			float3 random_offset = mins;
			return (rand.Next(101) / 100.0f) * random_strength[index] + random_offset[index];
		}

		private static float GetColorChannelValue(int index, ref Random rand, ColorModelEnum model)
		{
			float val;
			if (Config.GetValue(USE_STATIC_COLOR))
			{
				val = GetStaticColorChannelValue(index, model, ref rand);
			}
			else
			{
				val = GetRandomColorChannelValue(index, ref rand);
			}
			return val;
		}

		private static BaseX.color GetColorFromUlong(ulong val, ulong divisor, ref Random rand)
		{
			float hue = 0f;
			float sat = 0f;
			float val_lightness = 0f;

			//float shift = 0f;
			float strength = (val % divisor) / (float)divisor;

			if (Config.GetValue(COLOR_MODEL) == ColorModelEnum.RGB)
			{
				hue = GetColorChannelValue(0, ref rand, Config.GetValue(COLOR_MODEL));
			}
			else
			{
				hue = strength;
			}

			//switch (Config.GetValue(NON_RANDOM_REFID_WAVEFORM))
			//{
			//	case ChannelShiftWaveformEnum.Sawtooth:
			//		shift = strength;
			//		break;
			//	case ChannelShiftWaveformEnum.Sine:
			//		shift = MathX.Sin(strength * 2f * (float)Math.PI);
			//		break;
			//	default:
			//		break;
			//}

			//int3 shiftChannels = Config.GetValue(NON_RANDOM_REFID_CHANNELS);
			//float3 shiftOffsets = Config.GetValue(NON_RANDOM_REFID_OFFSETS);

			//if (shiftChannels[0] != 0)
			//{
			//	hue = shift + shiftOffsets[0];
			//}
			//else
			//{
			//	// maybe it should use RNG from the dynamic section here?
			//	hue = GetColorChannelValue(0, ref rngTimeSeeded, Config.GetValue(COLOR_MODEL));
			//}
			//if (shiftChannels[1] != 0)
			//{
			//	sat = shift + shiftOffsets[1];
			//}
			//else
			//{
			//	sat = GetColorChannelValue(1, ref rngTimeSeeded, Config.GetValue(COLOR_MODEL));
			//}
			//if (shiftChannels[2] != 0)
			//{
			//	val_lightness = shift + shiftOffsets[2];
			//}
			//else
			//{
			//	val_lightness = GetColorChannelValue(2, ref rngTimeSeeded, Config.GetValue(COLOR_MODEL));
			//}

			sat = GetColorChannelValue(1, ref rand, Config.GetValue(COLOR_MODEL));
			val_lightness = GetColorChannelValue(2, ref rand, Config.GetValue(COLOR_MODEL));

			BaseX.color c = Config.GetValue(NODE_COLOR);
			switch (Config.GetValue(COLOR_MODEL))
			{
				case ColorModelEnum.HSV:
					c = new ColorHSV(hue, sat, val_lightness, 0.8f).ToRGB();
					break;
				case ColorModelEnum.HSL:
					c = new ColorHSL(hue, sat, val_lightness, 0.8f).ToRGB();
					break;
				case ColorModelEnum.RGB:
					// hue = r, sat = g, val_lightness = b
					c = new color(hue, sat, val_lightness, 0.8f);
					break;
				default:
					break;
			}
			return c;
		}

		private static color GetColorWithRNG(ref Random rand)
		{
			//color colorToSet = Config.GetValue(NODE_COLOR);

			// RNG seeded by any constant node factor will always give the same color
			float hue;
			float sat;
			float val_lightness;

			hue = GetColorChannelValue(0, ref rand, Config.GetValue(COLOR_MODEL));
			sat = GetColorChannelValue(1, ref rand, Config.GetValue(COLOR_MODEL));
			val_lightness = GetColorChannelValue(2, ref rand, Config.GetValue(COLOR_MODEL));

			switch (Config.GetValue(COLOR_MODEL))
			{
				case ColorModelEnum.HSV:
					return new ColorHSV(hue, sat, val_lightness, 0.8f).ToRGB();
					//var cHsv = new ColorHSV(colorToSet);
					//Msg($"{cHsv.h.ToString()} {cHsv.s.ToString()} {cHsv.v.ToString()}");
					//break;
				case ColorModelEnum.HSL:
					return new ColorHSL(hue, sat, val_lightness, 0.8f).ToRGB();
					//var cHsl = new ColorHSL(colorToSet);
					//Msg($"{cHsl.h.ToString()} {cHsl.s.ToString()} {cHsl.l.ToString()}");
					//break;
				case ColorModelEnum.RGB:
					return new color(hue, sat, val_lightness, 0.8f);
					//var cRgb = new color(colorToSet);
					//Msg($"{cRgb.r.ToString()} {cRgb.g.ToString()} {cRgb.b.ToString()}");
					//break;
				default:
					return Config.GetValue(NODE_COLOR);
					//break;
			}
			//return colorToSet;
		}

		private static float sRGBtoLin(float v)
		{
			if (v < 0.04045)
			{
				return v / 12.92f;
			}
			else
			{
				return (float)Math.Pow((v + 0.055) / 1.055, 2.4);
			}
		}

		private static float GetLuminance(color a)
		{
			float sR = (float)Math.Pow(a.r, 1f / 2.2f);
			float sG = (float)Math.Pow(a.g, 1f / 2.2f);
			float sB = (float)Math.Pow(a.b, 1f / 2.2f);

			float y = (0.2126f * sRGBtoLin(sR) + 0.7152f * sRGBtoLin(sG) + 0.0722f * sRGBtoLin(sB));
			
			return y;
		}

		private static float GetPerceptualLightness(float luminance)
		{
			if (luminance <= (216f/24389f))
			{
				return luminance * (24389f / 27f);
			}
			else
			{
				return (float)Math.Pow(luminance, (1f / 3f)) * 116f - 16f;
			}
		}

		private static float GetContrast(color a, color b)
		{
			return (Math.Max(GetLuminance(a), GetLuminance(b)) + 0.05f) / (Math.Min(GetLuminance(a), GetLuminance(b)) + 0.05f);
		}

		private static color GetTextColor(color bg)
		{
			float whiteContrast = GetContrast(bg, color.White);
			float blackContrast = GetContrast(bg, color.Black);
			return whiteContrast > blackContrast ? color.White : color.Black;
		}

		private static void UpdateRefOrDriverNodeColor(LogixNode node, string targetField)
		{
			if (node == null) return;
			if (node.ActiveVisual == null) return;
			//Debug("in UpdateRefOrDriverNodeColor method");
			node.RunInUpdates(0, () =>
			{
				var targetSyncRef = node.TryGetField(targetField) as ISyncRef;
				if (targetSyncRef == null) return;
				Debug($"Updating color for Node {node.Name} {node.ReferenceID.ToString()}");

				if (targetSyncRef.Target == null)
				{
					Debug("Null syncref target found! setting error color");
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
					Debug($"SyncRef Target not null. Setting default color. SyncRef Target: {targetSyncRef.Target.ToString()}");
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

		private static bool IsWorldElementInNodeElementMap(LogixNode key, IWorldElement key2)
		{
			HashSet<IWorldElement> worldElements;
			nodeElementMap.TryGetValue(key, out worldElements);
			return worldElements.Contains(key2);
		}

		private static void SubscribeToEvents(LogixNode node, string targetField)
		{
			Debug($"Subscribing to events for node {node.Name} {node.ReferenceID.ToString()}");

			var targetSyncRef = node.TryGetField(targetField) as ISyncRef;
			if (targetSyncRef == null) return;

			// find out when the nearest component or slot gets destroyed
			// and update the node color when that happens
			Component nearestComp = targetSyncRef.Target.FindNearestParent<Component>();
			Slot nearestSlot = targetSyncRef.Target.FindNearestParent<Slot>();

			Action<IChangeable> func = (worldElement) =>
			{
				UpdateRefOrDriverNodeColor(node, targetField);
			};

			Action<IChangeable> func3 = (worldElement) =>
			{
				Debug($"targetSyncRef Changed. Clearing IWorldElement HashSet for node {node.Name} {node.ReferenceID.ToString()} and resubscribing to events.");
				nodeElementMap[node].Clear();
				nodeElementMap[node].Add(targetSyncRef);
				SubscribeToEvents(node, targetField);
				UpdateRefOrDriverNodeColor (node, targetField);
			};

			if (!IsWorldElementInNodeElementMap(node, targetSyncRef))
			{
				Debug($"Subscribing to SyncRef Changed event for node {node.Name} {node.ReferenceID.ToString()}");
				targetSyncRef.Changed += func3;
				nodeElementMap[node].Add(targetSyncRef);
			}

			if (nearestSlot != null) Debug($"Found nearest slot: {nearestSlot.ToString()}");
			if (nearestComp != null) Debug($"Found nearest component: {nearestComp.ToString()}");

			if (nearestSlot == null && nearestComp == null)
			{
				// xD
			}
			else
			{
				if (nearestSlot != null)
				{
					Action<Worker> func2 = (worker) =>
					{
						UpdateRefOrDriverNodeColor(node, targetField);
					};
					if (!IsWorldElementInNodeElementMap(node, nearestSlot))
					{
						Debug("Subscribing for nearest slot");
						nearestSlot.Disposing += func2;
						nodeElementMap[node].Add(nearestSlot);
					}
					if (nearestComp != null && nearestComp.IsChildOfElement(nearestSlot) && nearestComp != ((Component)node))
					{
						if (!IsWorldElementInNodeElementMap(node, nearestComp))
						{
							Debug("Subscribing for nearest component");
							nearestComp.Destroyed += func;
							nodeElementMap[node].Add(nearestComp);
						}
					}
				}
				else if (nearestComp != null && nearestComp != ((Component)node))
				{
					if (!IsWorldElementInNodeElementMap(node, nearestComp))
					{
						Debug("Subscribing for nearest component");
						nearestComp.Destroyed += func;
						nodeElementMap[node].Add(nearestComp);
					}
				}

				//TrySetTag(node.ActiveVisual, DELEGATE_ADDED_TAG);
				//Debug("Setting slot tag");
			}
		}

		//private static int GetInt32FromUlong(ulong ul)
		//{
		//	int corrected;
		//	//corrected = (int)(ul - (ulong)Int32.MaxValue * Math.Floor(((double)ul / (double)Int32.MaxValue)));
		//	corrected = (int)(ul % (ulong)Int32.MaxValue);
  //          return corrected + Int32.MinValue;
		//}

		[HarmonyPatch(typeof(LogixNode))]
		[HarmonyPatch("GenerateVisual")]
		class Patch_LogixNode_GenerateVisual
		{
			//[HarmonyAfter(new string[] { "Banane9.LogixVisualCustomizer" })]
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
						Debug("Running main event subscription code");
						if (!nodeElementMap.ContainsKey(__instance))
						{
							nodeElementMap.Add(__instance, new HashSet<IWorldElement>());
							Debug($"Adding new node {__instance.Name} {__instance.ReferenceID.ToString()} to nodeElementMap.");
						}
						__instance.RunInUpdates(0, () =>
						{
							SubscribeToEvents(__instance, targetField);

							UpdateRefOrDriverNodeColor(__instance, targetField);

							TrySetSlotTag(__instance.ActiveVisual, DELEGATE_ADDED_TAG);
							Debug("Setting slot tag");

							// remove node from nodeElementMap when it gets destroyed
							((Component)__instance).Destroyed += (worker) =>
							{
								nodeElementMap.Remove(__instance);
								Debug($"Component destroyed. Removed node {__instance.Name} {__instance.ReferenceID.ToString()} from nodeElementMap.");
							};
						});
					}
					else if (targetField != null)
					{
						Debug("Delegate added tag found and targetField not null. Updating node color.");
						UpdateRefOrDriverNodeColor(__instance, targetField);
					}
				}
			}
		}

		[HarmonyPatch(typeof(LogixNode))]
		[HarmonyPatch("GenerateUI")]
		class Patch_LogixNode_GenerateUI
		{
			[HarmonyAfter(new string[] { "Banane9.LogixVisualCustomizer" })]
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
									// default color
									BaseX.color colorToSet = Config.GetValue(NODE_COLOR);
									rng = null;

									//float3 ranges = Config.GetValue(RANDOM_RANGES_AROUND_STATIC_VALUES);
									//Msg($"ranges: {ranges[0].ToString()} {ranges[1].ToString()} {ranges[2].ToString()}");

									string nodeCategoryString;
									//Msg("node color mode: " + Config.GetValue(NODE_COLOR_MODE).ToString());
									switch (Config.GetValue(NODE_COLOR_MODE))
									{
										case NodeColorModeEnum.NodeName:
											rng = new System.Random(LogixHelper.GetNodeName(__instance.GetType()).GetHashCode() + Config.GetValue(RANDOM_SEED));
											break;
										case NodeColorModeEnum.NodeCategory:
											nodeCategoryString = GetNodeCategoryString(__instance.GetType());
											rng = new System.Random(nodeCategoryString.GetHashCode() + Config.GetValue(RANDOM_SEED));
											break;
										case NodeColorModeEnum.TopmostNodeCategory:
											nodeCategoryString = GetNodeCategoryString(__instance.GetType(), onlyTopmost: true);
											rng = new System.Random(nodeCategoryString.GetHashCode() + Config.GetValue(RANDOM_SEED));
											break;
										case NodeColorModeEnum.FullTypeName:
											rng = new System.Random(__instance.GetType().FullName.GetHashCode() + Config.GetValue(RANDOM_SEED));
											break;
										case NodeColorModeEnum.RefID:
											rng = new System.Random(root.Parent.ReferenceID.Position.GetHashCode() + Config.GetValue(RANDOM_SEED));
											//Msg($"RefID Position: {root.Parent.ReferenceID.Position.ToString()}");
											break;
										default:
											break;
									}

									if (Config.GetValue(ENABLE_NON_RANDOM_REFID))
									{
										int refidModDivisor = Config.GetValue(REFID_MOD_DIVISOR);
										// force it to 1 to avoid dividing by 0
										ulong divisor = (refidModDivisor > 0) ? (ulong)refidModDivisor : 1;

										if (Config.GetValue(USE_SYSTEM_TIME_RNG))
										{
											colorToSet = GetColorFromUlong(root.Parent.ReferenceID.Position, divisor, ref rngTimeSeeded);
										}
										else
										{
											colorToSet = GetColorFromUlong(root.Parent.ReferenceID.Position, divisor, ref rng);
										}

										rng = null;
									}

									if (rng != null)
									{
										if (Config.GetValue(USE_SYSTEM_TIME_RNG))
										{
											colorToSet = GetColorWithRNG(ref rngTimeSeeded);
										}
										else
										{
											colorToSet = GetColorWithRNG(ref rng);
										}
									}

									if (Config.GetValue(MULTIPLY_OUTPUT_BY_RGB))
									{
										float3 multiplier = Config.GetValue(RGB_CHANNEL_MULTIPLIER);
										colorToSet = colorToSet.MulR(multiplier.x);
										colorToSet = colorToSet.MulG(multiplier.y);
										colorToSet = colorToSet.MulB(multiplier.z);
									}

									//Msg($"before clamp {colorToSet.r.ToString()} {colorToSet.g.ToString()} {colorToSet.b.ToString()}");
									if (!Config.GetValue(ALLOW_NEGATIVE_AND_EMISSIVE_COLORS))
									{
										// clamp color to min 0 and max 1 (no negative or emissive colors allowed)
										if (colorToSet.r > 1) colorToSet = colorToSet.SetR(1f);
										if (colorToSet.r < 0) colorToSet = colorToSet.SetR(0f);

										if (colorToSet.g > 1) colorToSet = colorToSet.SetG(1f);
										if (colorToSet.g < 0) colorToSet = colorToSet.SetG(0f);

										if (colorToSet.b > 1) colorToSet = colorToSet.SetB(1f);
										if (colorToSet.b < 0) colorToSet = colorToSet.SetB(0f);
									}
									//Msg($"after clamp {colorToSet.r.ToString()} {colorToSet.g.ToString()} {colorToSet.b.ToString()}");

									TrySetImageTint(image, colorToSet);

									if (Config.GetValue(ENABLE_TEXT_CONTRAST))
									{
										// set node label's text color
										__instance.RunInUpdates(0, () =>
										{
											Text firstText = root.GetComponent<Text>();
											bool flag = false;

											foreach (Text text in root.GetComponentsInChildren<Text>())
											{
												if (text.Slot.Parent.Name == "Vertical Layout" || text.Slot.Parent.Name == "Horizontal Layout")
												{
													flag = true;
													TrySetTextColor(text, GetTextColor(colorToSet));
													break;
												}
											}

											if (!flag)
											{
												TrySetTextColor(firstText, GetTextColor(colorToSet));
											}
										});
									}

									TrySetSlotTag(root, COLOR_SET_TAG);
								}
							}
						}
					}
				}
			}
		}
	}
}