//#define DEBUG

using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.UIX;
using HarmonyLib;
using NeosModLoader;
using System;
using System.Reflection;
using BaseX;

#if DEBUG

#endif

namespace ColorMyLogixNodes
{
	public class ColorMyLogixNodes : NeosMod
	{
		public override string Name => "ColorMyLogiX";
		public override string Author => "Nytra";
		public override string Version => "1.2.0";
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
		private static ModConfigurationKey<BaseX.color> NODE_COLOR = new ModConfigurationKey<BaseX.color>("NODE_COLOR", "Static Node Color:", () => new BaseX.color(1.0f, 1.0f, 1.0f, 0.8f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_STATIC_RANGES = new ModConfigurationKey<bool>("USE_STATIC_RANGES", "Use Random Ranges around Static Node Color:", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> RANDOM_RANGES_AROUND_STATIC_VALUES = new ModConfigurationKey<float3>("RANDOM_RANGES_AROUND_STATIC_VALUES", "Random Ranges around Static Node Color [0 to 1]:", () => new float3(0.1f, 0.1f, 0.1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<StaticRangeModeEnum> STATIC_RANGE_MODE = new ModConfigurationKey<StaticRangeModeEnum>("STATIC_RANGE_MODE", "Seed for Random Ranges around Static Node Color:", () => StaticRangeModeEnum.SystemTime, internalAccessOnly: true);
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
		private static ModConfigurationKey<bool> ALTERNATE_CATEGORY_STRING = new ModConfigurationKey<bool>("ALTERNATE_CATEGORY_STRING", "Use alternate node category string (only uses the part after the final '/'):", () => false, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<int> RANDOM_SEED = new ModConfigurationKey<int>("RANDOM_SEED", "Seed:", () => 0);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> COLOR_CHANNELS_MAX = new ModConfigurationKey<float3>("COLOR_CHANNELS_MAX", "Random Max [0 to 1]:", () => new float3(1f, 0.5f, 1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> COLOR_CHANNELS_MIN = new ModConfigurationKey<float3>("COLOR_CHANNELS_MIN", "Random Min [0 to 1]:", () => new float3(0f, 0.5f, 1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_2_2 = new ModConfigurationKey<dummy>("DUMMY_SEP_2_2", $"<color={DETAIL_TEXT_COLOR}><i>Maximum and minimum bounds for randomness in the channels of the Selected Color Model</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_NODE_ALPHA = new ModConfigurationKey<bool>("USE_NODE_ALPHA", "Use node alpha:", () => true, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float> NODE_ALPHA = new ModConfigurationKey<float>("NODE_ALPHA", "Node alpha [0 to 1] (Default 0.8):", () => 0.8f);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_2_3 = new ModConfigurationKey<dummy>("DUMMY_SEP_2_3", $"<color={DETAIL_TEXT_COLOR}><i>The randomness in this section is affected by the Selected Node Factor plus the Seed</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_6 = new ModConfigurationKey<dummy>("DUMMY_SEP_6", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_6_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_6_1", $"<color={HEADER_TEXT_COLOR}>[OVERRIDES]</color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_DISPLAY_COLOR_OVERRIDE = new ModConfigurationKey<bool>("USE_DISPLAY_COLOR_OVERRIDE", "Override display node color:", () => true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<BaseX.color> DISPLAY_COLOR_OVERRIDE = new ModConfigurationKey<BaseX.color>("DISPLAY_COLOR_OVERRIDE", "Display node color:", () => new BaseX.color(0.25f, 0.25f, 0.25f, 0.8f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_7 = new ModConfigurationKey<dummy>("DUMMY_SEP_7", $"<color={DETAIL_TEXT_COLOR}><i>~/~</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_INPUT_COLOR_OVERRIDE = new ModConfigurationKey<bool>("USE_INPUT_COLOR_OVERRIDE", "Override input node color:", () => true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<InputNodeOverrideEnum> INPUT_NODE_OVERRIDE_TYPE = new ModConfigurationKey<InputNodeOverrideEnum>("INPUT_NODE_OVERRIDE_TYPE", "Input Node Type:", () => InputNodeOverrideEnum.OnlyPrimitives);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<BaseX.color> INPUT_COLOR_OVERRIDE = new ModConfigurationKey<BaseX.color>("INPUT_COLOR_OVERRIDE", "Input node color:", () => new BaseX.color(0.25f, 0.25f, 0.25f, 0.8f));
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<bool> OVERRIDE_ENUM_INPUT = new ModConfigurationKey<bool>("OVERRIDE_ENUM_INPUT", "Include EnumInput nodes:", () => true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> OVERRIDE_DYNAMIC_VARIABLE_INPUT = new ModConfigurationKey<bool>("OVERRIDE_DYNAMIC_VARIABLE_INPUT", "Include DynamicVariableInput nodes:", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_4 = new ModConfigurationKey<dummy>("DUMMY_SEP_4", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_4_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_4_1", $"<color={HEADER_TEXT_COLOR}>[TEXT]</color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ENABLE_TEXT_CONTRAST = new ModConfigurationKey<bool>("ENABLE_TEXT_CONTRAST", "Automatically change the color of text to contrast better with the node background:", () => true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float> PERCEPTUAL_LIGHTNESS_EXPONENT = new ModConfigurationKey<float>("PERCEPTUAL_LIGHTNESS_EXPONENT", "Exponent for perceptual lightness calculation (affects automatic text color, best ~0.6 to ~0.8):", () => 0.5f, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_STATIC_TEXT_COLOR = new ModConfigurationKey<bool>("USE_STATIC_TEXT_COLOR", "Use Static Text Color (Disables automatic text coloring):", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<BaseX.color> STATIC_TEXT_COLOR = new ModConfigurationKey<BaseX.color>("STATIC_TEXT_COLOR", "Static Text Color:", () => new BaseX.color(0f, 0f, 0f, 1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_5 = new ModConfigurationKey<dummy>("DUMMY_SEP_5", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_5_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_1", $"<color={HEADER_TEXT_COLOR}>[EXTRA]</color>", () => new dummy());
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
		private static ModConfigurationKey<bool> USE_SYSTEM_TIME_RNG = new ModConfigurationKey<bool>("USE_SYSTEM_TIME_RNG", "Always use randomness seeded by system time (Complete randomness, not suitable for normal use):", () => false, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ALLOW_NEGATIVE_AND_EMISSIVE_COLORS = new ModConfigurationKey<bool>("ALLOW_NEGATIVE_AND_EMISSIVE_COLORS", "Allow negative and emissive colors:", () => false, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> MAKE_CONNECT_POINTS_FULL_ALPHA = new ModConfigurationKey<bool>("MAKE_CONNECT_POINTS_FULL_ALPHA", "Make connect points on nodes have full alpha:", () => true, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> COLOR_RELAY_NODES = new ModConfigurationKey<bool>("COLOR_RELAY_NODES", "Apply colors to Relay Nodes:", () => false, internalAccessOnly: true);

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

		private enum InputNodeOverrideEnum
		{
			OnlyPrimitives,
			PrimitivesAndEnumInputs,
			WholeInputCategory
		}

		//private enum ChannelShiftWaveformEnum
		//{
		//	Sawtooth,
		//	Sine
		//}

		private static System.Random rng;
		private static System.Random rngTimeSeeded = new System.Random();

		private const string COLOR_SET_TAG = "ColorMyLogiX.ColorSet";
		private const string DELEGATE_ADDED_TAG = "ColorMyLogiX.EventSubscribed";

		//private static HashSet<WeakReference<LogixNode>> nodeKeys = new();
		//private static ConditionalWeakTable<LogixNode, ISyncRef> nodeMap = new();

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

		private static float GetStaticColorChannelValue(int index, ColorModelEnum model, Random rand)
		{
			float val = 0;
			int coinflip;
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
							val += (float)rand.NextDouble() * range * (float)coinflip / 2f;
							break;
						case StaticRangeModeEnum.SystemTime:
							coinflip = rngTimeSeeded.Next(2) == 0 ? -1 : 1;
							val += (float)rngTimeSeeded.NextDouble() * range * (float)coinflip / 2f;
							break;
						default:
							break;
					}
				}
				else
				{
					val = GetRandomColorChannelValue(index, rand);
				}
			}
			return val;
		}

		private static float GetRandomColorChannelValue(int index, Random rand)
		{
			float3 mins = Config.GetValue(COLOR_CHANNELS_MIN);
			float3 maxs = Config.GetValue(COLOR_CHANNELS_MAX);
			float3 random_strength = MathX.Abs(maxs - mins);
			float3 random_offset = mins;
			return (float)rand.NextDouble() * random_strength[index] + random_offset[index];
		}

		private static float GetColorChannelValue(int index, Random rand, ColorModelEnum model)
		{
			float val;
			if (Config.GetValue(USE_STATIC_COLOR))
			{
				val = GetStaticColorChannelValue(index, model, rand);
			}
			else
			{
				val = GetRandomColorChannelValue(index, rand);
			}
			return val;
		}

		private static BaseX.color GetColorFromUlong(ulong val, ulong divisor, Random rand)
		{
			float hue = 0f;
			float sat = 0f;
			float val_lightness = 0f;
			float alpha = 0.8f;
			if (Config.GetValue(USE_NODE_ALPHA))
			{
				alpha = Config.GetValue(NODE_ALPHA);
			}

			//float shift = 0f;
			float strength = (val % divisor) / (float)divisor;

			if (Config.GetValue(COLOR_MODEL) == ColorModelEnum.RGB)
			{
				hue = GetColorChannelValue(0, rand, Config.GetValue(COLOR_MODEL));
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

			sat = GetColorChannelValue(1, rand, Config.GetValue(COLOR_MODEL));
			val_lightness = GetColorChannelValue(2, rand, Config.GetValue(COLOR_MODEL));

			if (Config.GetValue(USE_STATIC_COLOR))
			{
				alpha = Config.GetValue(NODE_COLOR).a;
			}

			BaseX.color c = Config.GetValue(NODE_COLOR);
			switch (Config.GetValue(COLOR_MODEL))
			{
				case ColorModelEnum.HSV:
					c = new ColorHSV(hue, sat, val_lightness, alpha).ToRGB();
					break;
				case ColorModelEnum.HSL:
					c = new ColorHSL(hue, sat, val_lightness, alpha).ToRGB();
					break;
				case ColorModelEnum.RGB:
					// hue = r, sat = g, val_lightness = b
					c = new color(hue, sat, val_lightness, alpha);
					break;
				default:
					break;
			}
			return c;
		}

		private static color GetColorWithRNG(Random rand)
		{
			// RNG seeded by any constant node factor will always give the same color
			float hue;
			float sat;
			float val_lightness;
			float alpha = 0.8f;
			if (Config.GetValue(USE_NODE_ALPHA))
			{
				alpha = Config.GetValue(NODE_ALPHA);
			}

			hue = GetColorChannelValue(0, rand, Config.GetValue(COLOR_MODEL));
			sat = GetColorChannelValue(1, rand, Config.GetValue(COLOR_MODEL));
			val_lightness = GetColorChannelValue(2, rand, Config.GetValue(COLOR_MODEL));

			if (Config.GetValue(USE_STATIC_COLOR))
			{
				alpha = Config.GetValue(NODE_COLOR).a;
			}

			switch (Config.GetValue(COLOR_MODEL))
			{
				case ColorModelEnum.HSV:
					return new ColorHSV(hue, sat, val_lightness, alpha).ToRGB();
				case ColorModelEnum.HSL:
					return new ColorHSL(hue, sat, val_lightness, alpha).ToRGB();
				case ColorModelEnum.RGB:
					return new color(hue, sat, val_lightness, alpha);
				default:
					return Config.GetValue(NODE_COLOR);
			}
		}

		private static float GetLuminance(color c)
		{
			float sR = (float)Math.Pow(c.r, 2.2f);
			float sG = (float)Math.Pow(c.g, 2.2f);
			float sB = (float)Math.Pow(c.b, 2.2f);

			float luminance = (0.2126f * sR + 0.7152f * sG + 0.0722f * sB);
			
			return luminance;
		}

		private static float GetPerceptualLightness(float luminance)
		{
			// 1 = white, 0.5 = middle gray, 0 = black
			// the power can be tweaked here. ~0.6 to ~0.8
			return (float)Math.Pow(luminance, Config.GetValue(PERCEPTUAL_LIGHTNESS_EXPONENT));
		}

		private static color GetTextColor(color bg)
		{
			color c;
			if (Config.GetValue(USE_STATIC_TEXT_COLOR))
			{
				c = Config.GetValue(STATIC_TEXT_COLOR);
			}
			else
			{
				c = GetPerceptualLightness(GetLuminance(bg)) >= 0.5f ? color.Black : color.White;
			}
			if (!Config.GetValue(ALLOW_NEGATIVE_AND_EMISSIVE_COLORS))
			{
				ClampColor(ref c);
			}
			return c;
		}

		private static void ClampColor(ref color c)
		{
			// clamp color to min 0 and max 1 (no negative or emissive colors allowed)
			if (c.r > 1) c = c.SetR(1f);
			if (c.r < 0) c = c.SetR(0f);

			if (c.g > 1) c = c.SetG(1f);
			if (c.g < 0) c = c.SetG(0f);

			if (c.b > 1) c = c.SetB(1f);
			if (c.b < 0) c = c.SetB(0f);

			if (c.a > 1) c = c.SetA(1f);
			if (c.a < 0) c = c.SetA(0f);
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

		private static bool ShouldColorInputNode(LogixNode node)
		{
			InputNodeOverrideEnum inputNodeType = Config.GetValue(INPUT_NODE_OVERRIDE_TYPE);

			// Primitive input
			return (inputNodeType == InputNodeOverrideEnum.OnlyPrimitives && (node.Name.EndsWith("Input") && !node.Name.StartsWith("EnumInput"))) ||
				// Primitive and enum
				(inputNodeType == InputNodeOverrideEnum.PrimitivesAndEnumInputs && (node.Name.EndsWith("Input")) || (node.Name.StartsWith("EnumInput"))) ||
				// Whole input category
				(inputNodeType == InputNodeOverrideEnum.WholeInputCategory && (GetNodeCategoryString(node.GetType()) == "LogiX/Input" || GetNodeCategoryString(node.GetType()) == "LogiX/Input/Uncommon")) ||
				// Dynamic variable input
				(Config.GetValue(OVERRIDE_DYNAMIC_VARIABLE_INPUT) && node.Name.StartsWith("DynamicVariableInput"));
		}

		[HarmonyPatch(typeof(LogixNode))]
		[HarmonyPatch("GenerateVisual")]
		class Patch_LogixNode_GenerateVisual
		{
			[HarmonyAfter(new string[] { "Banane9.LogixVisualCustomizer", "Banane9, Fro Zen.LogixVisualCustomizer" })]
			static void Postfix(LogixNode __instance)
			{
				if (Config.GetValue(MOD_ENABLED) == true && __instance.ActiveVisual != null && __instance.ActiveVisual.ReferenceID.User == __instance.LocalUser.AllocationID)
				{
					string targetField = null;
					if (Config.GetValue(COLOR_NULL_REFERENCE_NODES) == true && __instance.Name.StartsWith("ReferenceNode"))
					{
						targetField = "RefTarget";
					}
					else if (Config.GetValue(COLOR_NULL_DRIVER_NODES) == true && __instance.Name.StartsWith("DriverNode"))
					{
						targetField = "DriveTarget";
					}
					if (targetField != null)
					{
						var targetSlot = __instance.Slot.FindChild((Slot c) => c.Name == __instance.LocalUser.ReferenceID.ToString() && c.Tag == DELEGATE_ADDED_TAG);
						//var targetComp = __instance.Slot.GetComponent<Comment>((Comment c) => c.ReferenceID.User == __instance.LocalUser.AllocationID && c.Text.Value == DELEGATE_ADDED_TAG);
						if (targetSlot == null)
						{
							__instance.RunInUpdates(0, () =>
							{
								var targetSlot = __instance.Slot.FindChild((Slot c) => c.Name == __instance.LocalUser.ReferenceID.ToString() && c.Tag == DELEGATE_ADDED_TAG);
								if (targetSlot != null) return;
								//var targetComp = __instance.Slot.GetComponent<Comment>((Comment c) => c.ReferenceID.User == __instance.LocalUser.AllocationID && c.Text.Value == DELEGATE_ADDED_TAG);
								//if (targetComp != null) return;

								ISyncRef syncRef = __instance.TryGetField(targetField) as ISyncRef;

								Debug("Subscribing to this node's Changed event.");

								syncRef.Changed += (iChangeable) =>
								{
									UpdateRefOrDriverNodeColor(__instance, targetField);
								};

								UpdateRefOrDriverNodeColor(__instance, targetField);

								//TrySetSlotTag(__instance.Slot, DELEGATE_ADDED_TAG);
								
								//nodeKeys.Add(new WeakReference<LogixNode>(__instance));

								//var newComment = __instance.Slot.AttachComponent<Comment>();
								//newComment.Text.Value = DELEGATE_ADDED_TAG;
								//newComment.Persistent = false;

								//__instance.World.UserLeft += (user) =>
								//{
									//if (user == __instance.LocalUser)
									//{
										//newComment.Destroy();
									//}
								//};

								// could maybe do this with a comment and subscribe to World.UserLeft to destroy it?
								var newSlot = __instance.Slot.AddSlot(__instance.LocalUser.ReferenceID.ToString());
								newSlot.PersistentSelf = false;
								newSlot.Tag = DELEGATE_ADDED_TAG;
								var destroyOnUserLeave = newSlot.AttachComponent<DestroyOnUserLeave>();
								destroyOnUserLeave.TargetUser.User.Value = __instance.LocalUser.ReferenceID;
								destroyOnUserLeave.Persistent = false;
							});
						}
						else
						{
							Debug("Node already subscribed. Updating node color.");
							UpdateRefOrDriverNodeColor(__instance, targetField);
						}
					}
				}
			}
		}

		[HarmonyPatch(typeof(LogixNode))]
		[HarmonyPatch("GenerateUI")]
		class Patch_LogixNode_GenerateUI
		{
			[HarmonyAfter(new string[] { "Banane9.LogixVisualCustomizer", "Banane9, Fro Zen.LogixVisualCustomizer" })]
			static void Postfix(LogixNode __instance, Slot root)
			{
				// only run if the logix node visual slot is allocated to the local user
				if (Config.GetValue(MOD_ENABLED) == true && root != null && root.ReferenceID.User == root.LocalUser.AllocationID)
				{
					// don't apply custom color to cast nodes, because it makes it confusing to read the data types
					if (__instance.Name.StartsWith("CastClass")) return;
					if (__instance.Name.StartsWith("Cast_")) return;
					if (!Config.GetValue(COLOR_RELAY_NODES) && (__instance.Name.StartsWith("RelayNode") || __instance.Name.StartsWith("ImpulseRelay"))) return;

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

									if (Config.GetValue(USE_DISPLAY_COLOR_OVERRIDE) && (__instance.Name.StartsWith("Display_") || __instance.Name == "DisplayImpulse"))
									{
										colorToSet = Config.GetValue(DISPLAY_COLOR_OVERRIDE);
									}
									//else if (Config.GetValue(USE_INPUT_COLOR_OVERRIDE) && (__instance.Name.EndsWith("Input") || (Config.GetValue(OVERRIDE_DYNAMIC_VARIABLE_INPUT) && __instance.Name.StartsWith("DynamicVariableInput"))))
									//else if (Config.GetValue(USE_INPUT_COLOR_OVERRIDE) && (GetNodeCategoryString(__instance.GetType()) == "LogiX/Input" || (Config.GetValue(OVERRIDE_DYNAMIC_VARIABLE_INPUT) && __instance.Name.StartsWith("DynamicVariableInput"))))
									//{
									//	colorToSet = Config.GetValue(INPUT_COLOR_OVERRIDE);
									//}
									else if (Config.GetValue(USE_INPUT_COLOR_OVERRIDE) && ShouldColorInputNode(__instance))
									{
										colorToSet = Config.GetValue(INPUT_COLOR_OVERRIDE);
									}
									else
									{
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
												rng = new System.Random(root.Parent.ReferenceID.GetHashCode() + Config.GetValue(RANDOM_SEED));
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
												colorToSet = GetColorFromUlong(root.Parent.ReferenceID.Position, divisor, rngTimeSeeded);
											}
											else
											{
												colorToSet = GetColorFromUlong(root.Parent.ReferenceID.Position, divisor, rng);
											}

											rng = null;
										}

										if (rng != null)
										{
											if (Config.GetValue(USE_SYSTEM_TIME_RNG))
											{
												colorToSet = GetColorWithRNG(rngTimeSeeded);
											}
											else
											{
												colorToSet = GetColorWithRNG(rng);
											}
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
										ClampColor(ref colorToSet);
									}
									//Msg($"after clamp {colorToSet.r.ToString()} {colorToSet.g.ToString()} {colorToSet.b.ToString()}");

									// ensure the alpha is always 0.8 (default alpha)
									//colorToSet = colorToSet.SetA(0.8f);

									TrySetImageTint(image, colorToSet);

									if (Config.GetValue(MAKE_CONNECT_POINTS_FULL_ALPHA))
									{
										// Make the type-colored images on nodes have full alpha to make them easier to read
										foreach (Image i in imageSlot.GetComponentsInChildren<Image>())
										{
											if (i != image) TrySetImageTint(i, i.Tint.Value.SetA(1f));
										}
									}

									if (Config.GetValue(ENABLE_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR))
									{
										// set node label's text color
										// need to wait 3 updates because who knows why...
										__instance.RunInUpdates(3, () =>
										{
											foreach (Text text in root.GetComponentsInChildren<Text>())
											{
												if (text.Slot.Name == "Text" && (text.Slot.Parent.Name == "Vertical Layout" || text.Slot.Parent.Name == "Horizontal Layout" || text.Slot.Parent.Name == "TextPadding"))
												{
													TrySetTextColor(text, GetTextColor(colorToSet));
													//break;
												}
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