//#define DEBUG

using FrooxEngine;
using FrooxEngine.LogiX;
using FrooxEngine.UIX;
using HarmonyLib;
using NeosModLoader;
using BaseX;

#if DEBUG

#endif

namespace ColorMyLogixNodes
{
	public partial class ColorMyLogixNodes : NeosMod
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
		private static ModConfigurationKey<dummy> DUMMY_SEP_7_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_7_1", SEP_STRING, () => new dummy());
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
		//[AutoRegisterConfigKey]
		//private static ModConfigurationKey<dummy> DUMMY_SEP_7_2 = new ModConfigurationKey<dummy>("DUMMY_SEP_7_2", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<int> RANDOM_SEED = new ModConfigurationKey<int>("RANDOM_SEED", "Seed:", () => 0);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_7_3 = new ModConfigurationKey<dummy>("DUMMY_SEP_7_3", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> COLOR_CHANNELS_MAX = new ModConfigurationKey<float3>("COLOR_CHANNELS_MAX", "Random Max [0 to 1]:", () => new float3(1f, 0.5f, 1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> COLOR_CHANNELS_MIN = new ModConfigurationKey<float3>("COLOR_CHANNELS_MIN", "Random Min [0 to 1]:", () => new float3(0f, 0.5f, 1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_2_2 = new ModConfigurationKey<dummy>("DUMMY_SEP_2_2", $"<color={DETAIL_TEXT_COLOR}><i>Maximum and minimum bounds for randomness in the channels of the Selected Color Model</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_2_3 = new ModConfigurationKey<dummy>("DUMMY_SEP_2_3", $"<color={DETAIL_TEXT_COLOR}><i>The randomness in this section is affected by the Selected Node Factor plus the Seed</i></color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_7_8 = new ModConfigurationKey<dummy>("DUMMY_SEP_7_8", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_NODE_ALPHA = new ModConfigurationKey<bool>("USE_NODE_ALPHA", "Use custom node alpha:", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float> NODE_ALPHA = new ModConfigurationKey<float>("NODE_ALPHA", "Custom node alpha [0 to 1]:", () => 0.8f);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_6 = new ModConfigurationKey<dummy>("DUMMY_SEP_6", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_6_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_6_1", $"<color={HEADER_TEXT_COLOR}>[OVERRIDES]</color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_DISPLAY_COLOR_OVERRIDE = new ModConfigurationKey<bool>("USE_DISPLAY_COLOR_OVERRIDE", "Override display node color:", () => true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<BaseX.color> DISPLAY_COLOR_OVERRIDE = new ModConfigurationKey<BaseX.color>("DISPLAY_COLOR_OVERRIDE", "Display node color:", () => new BaseX.color(0.25f, 0.25f, 0.25f, 0.8f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_7 = new ModConfigurationKey<dummy>("DUMMY_SEP_7", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_INPUT_COLOR_OVERRIDE = new ModConfigurationKey<bool>("USE_INPUT_COLOR_OVERRIDE", "Override input node color:", () => true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<InputNodeOverrideEnum> INPUT_NODE_OVERRIDE_TYPE = new ModConfigurationKey<InputNodeOverrideEnum>("INPUT_NODE_OVERRIDE_TYPE", "Input Node Type:", () => InputNodeOverrideEnum.PrimitivesAndEnums, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<BaseX.color> INPUT_COLOR_OVERRIDE = new ModConfigurationKey<BaseX.color>("INPUT_COLOR_OVERRIDE", "Input node color:", () => new BaseX.color(0.25f, 0.25f, 0.25f, 0.8f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> OVERRIDE_DYNAMIC_VARIABLE_INPUT = new ModConfigurationKey<bool>("OVERRIDE_DYNAMIC_VARIABLE_INPUT", "Include DynamicVariableInput nodes:", () => true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_4 = new ModConfigurationKey<dummy>("DUMMY_SEP_4", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_4_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_4_1", $"<color={HEADER_TEXT_COLOR}>[TEXT]</color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> USE_STATIC_TEXT_COLOR = new ModConfigurationKey<bool>("USE_STATIC_TEXT_COLOR", "Use Static Text Color (Disables automatic text coloring):", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<BaseX.color> STATIC_TEXT_COLOR = new ModConfigurationKey<BaseX.color>("STATIC_TEXT_COLOR", "Static Text Color:", () => new BaseX.color(0f, 0f, 0f, 1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_7_4 = new ModConfigurationKey<dummy>("DUMMY_SEP_7_4", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ENABLE_TEXT_CONTRAST = new ModConfigurationKey<bool>("ENABLE_TEXT_CONTRAST", "Automatically change the color of text to contrast better with the node background:", () => true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float> PERCEPTUAL_LIGHTNESS_EXPONENT = new ModConfigurationKey<float>("PERCEPTUAL_LIGHTNESS_EXPONENT", "Exponent for perceptual lightness calculation (affects automatic text color, best ~0.6 to ~0.8):", () => 0.5f, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_5 = new ModConfigurationKey<dummy>("DUMMY_SEP_5", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_5_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_5_1", $"<color={HEADER_TEXT_COLOR}>[EXTRA]</color>", () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> MULTIPLY_OUTPUT_BY_RGB = new ModConfigurationKey<bool>("MULTIPLY_OUTPUT_BY_RGB", "Use Output RGB Channel Multiplier:", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<float3> RGB_CHANNEL_MULTIPLIER = new ModConfigurationKey<float3>("RGB_CHANNEL_MULTIPLIER", "Output RGB Channel Multiplier:", () => new float3(1f, 1f, 1f));
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_7_5 = new ModConfigurationKey<dummy>("DUMMY_SEP_7_5", SEP_STRING, () => new dummy());
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ENABLE_NON_RANDOM_REFID = new ModConfigurationKey<bool>("ENABLE_NON_RANDOM_REFID", "Enable Hue-shift Mode (HSV and HSL only):", () => false);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_7_6 = new ModConfigurationKey<dummy>("DUMMY_SEP_7_6", SEP_STRING, () => new dummy());
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

		// MORE INTERNAL ACCESS CONFIG KEYS
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_7_7 = new ModConfigurationKey<dummy>("DUMMY_SEP_7_7", SEP_STRING, () => new dummy(), internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> UPDATE_NODES_ON_CONFIG_CHANGED = new ModConfigurationKey<bool>("UPDATE_NODES_ON_CONFIG_CHANGED", "Auto-refresh the colors of regular nodes in the data slot when your mod config changes:", () => true, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ADD_REGULAR_NODES_TO_DATA_SLOT = new ModConfigurationKey<bool>("ADD_REGULAR_NODES_TO_DATA_SLOT", "Add regular nodes to the data slot (Required for auto-refresh to work):", () => true, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> ADD_REF_DRIVER_NODES_TO_DATA_SLOT = new ModConfigurationKey<bool>("ADD_REF_DRIVER_NODES_TO_DATA_SLOT", "Add ref and driver nodes to the data slot (Makes them change color when their references change):", () => true, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_8 = new ModConfigurationKey<dummy>("DUMMY_SEP_8", SEP_STRING, () => new dummy(), internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> COLOR_NULL_REFERENCE_NODES = new ModConfigurationKey<bool>("COLOR_NULL_REFERENCE_NODES", "Should Null Reference Nodes use Node Error Color:", () => true, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<bool> COLOR_NULL_DRIVER_NODES = new ModConfigurationKey<bool>("COLOR_NULL_DRIVER_NODES", "Should Null Driver Nodes use Node Error Color:", () => true, internalAccessOnly: true);
		[AutoRegisterConfigKey]
		private static ModConfigurationKey<dummy> DUMMY_SEP_8_1 = new ModConfigurationKey<dummy>("DUMMY_SEP_8_1", SEP_STRING, () => new dummy(), internalAccessOnly: true);
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
			Primitives,
			PrimitivesAndEnums,
			Everything
		}

		//private enum ChannelShiftWaveformEnum
		//{
		//	Sawtooth,
		//	Sine
		//}

		private static System.Random rng;
		private static System.Random rngTimeSeeded = new System.Random();

		private const string COLOR_SET_TAG = "ColorMyLogiX.ColorSet";
		//private const string DELEGATE_ADDED_TAG = "ColorMyLogiX.EventSubscribed";
		private const string DATA_ROOT_SLOT_NAME = "ColorMyLogiX.DataRoot";
		private const string REF_DRIVER_NODE_DATA_SLOT_NAME = "ColorMyLogiX.RefDriverNodeData";
		private const string REGULAR_NODE_DATA_SLOT_NAME = "ColorMyLogiX.RegularNodeData";

		private static Slot dataRootSlot = null;
		private static Slot refDriverNodeDataSlot = null;
		private static Slot regularNodeDataSlot = null;

		public override void OnEngineInit()
		{
			Harmony harmony = new Harmony($"owo.{Author}.{Name}");
			Config = GetConfiguration()!;
			Config.Save(true);
			harmony.PatchAll();

			Config.OnThisConfigurationChanged += (config) =>
			{
				if (Config.GetValue(MOD_ENABLED) && Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED)) {

					if (dataRootSlot == null || regularNodeDataSlot == null) return;

					if (dataRootSlot.World != Engine.Current.WorldManager.FocusedWorld)
					{
						//dataRootSlot = Engine.Current.WorldManager.FocusedWorld.AssetsSlot.FindOrAdd(DATA_ROOT_SLOT_NAME, false);
						dataRootSlot = Engine.Current.WorldManager.FocusedWorld.AssetsSlot.FindChild((Slot s) => s.Name == DATA_ROOT_SLOT_NAME);
					}

					if (regularNodeDataSlot.World != Engine.Current.WorldManager.FocusedWorld)
					{
						//regularNodeDataSlot = dataRootSlot.FindOrAdd(REGULAR_NODE_DATA_SLOT_NAME, false);
						regularNodeDataSlot = Engine.Current.WorldManager.FocusedWorld.AssetsSlot.FindChild((Slot s) => s.Name == REGULAR_NODE_DATA_SLOT_NAME);
					}

					if (dataRootSlot == null || regularNodeDataSlot == null) return;

					foreach (Slot slot in regularNodeDataSlot.Children)
					{
						if (slot.Tag != slot.LocalUser.ReferenceID.ToString()) return;

						LogixNode node = slot.GetComponent<ReferenceField<LogixNode>>().Reference;
						if (node != null)
						{
							foreach (var refField in slot.GetComponents<ReferenceField<IField<color>>>())
							{
								if (refField.Reference == null) continue;
								if (refField.Reference.RawTarget == null) continue;
								node.RunInUpdates(0, () =>
								{
									color c = ComputeColorForLogixNode(node);
									IField<color> colorField = refField.Reference.RawTarget;

									// background
									if (refField.UpdateOrder == 0)
									{
										if (node.ActiveVisual != null && node.ActiveVisual.Tag == "Disabled")
										{
											colorField.Value = Config.GetValue(NODE_ERROR_COLOR);
										}
										else
										{
											colorField.Value = c;
										}
									}
									// connect point (unused)
									else if (refField.UpdateOrder == 1)
									{
										if (Config.GetValue(MAKE_CONNECT_POINTS_FULL_ALPHA))
										{
											colorField.Value = colorField.Value.SetA(1f);
										}
										else
										{
											colorField.Value = colorField.Value.SetA(0.8f);
										}
									}
									// text
									else if (refField.UpdateOrder == 2)
									{
										if (Config.GetValue(ENABLE_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR))
										{
											colorField.Value = GetTextColor(c);
										}
									}
								});
							}
						}
						else
						{
							slot.RunInUpdates(0, () =>
							{
								slot.Destroy();
							});
						}
					}
				}
			};
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
						if (dataRootSlot == null || dataRootSlot.World != Engine.Current.WorldManager.FocusedWorld)
						{
							dataRootSlot = __instance.World.AssetsSlot.FindOrAdd(DATA_ROOT_SLOT_NAME, false);
							dataRootSlot.GetComponentOrAttach<AssetOptimizationBlock>();
						}

						if (refDriverNodeDataSlot == null || refDriverNodeDataSlot.World != Engine.Current.WorldManager.FocusedWorld)
						{
							refDriverNodeDataSlot = dataRootSlot.FindOrAdd(REF_DRIVER_NODE_DATA_SLOT_NAME, false);
							refDriverNodeDataSlot.GetComponentOrAttach<AssetOptimizationBlock>();
						}

						var targetSlot = refDriverNodeDataSlot.FindChild((Slot c) => c.Name == __instance.ReferenceID.ToString() && c.Tag == __instance.LocalUser.ReferenceID.ToString());
						if (targetSlot == null)
						{
							__instance.RunInUpdates(0, () =>
							{
								if (Config.GetValue(ADD_REF_DRIVER_NODES_TO_DATA_SLOT))
								{
									var targetSlot = refDriverNodeDataSlot.FindChild((Slot c) => c.Name == __instance.ReferenceID.ToString() && c.Tag == __instance.LocalUser.ReferenceID.ToString());

									if (targetSlot != null) return;

									ISyncRef syncRef = __instance.TryGetField(targetField) as ISyncRef;

									Debug("Subscribing to this node's Changed event.");

									syncRef.Changed += (iChangeable) =>
									{
										UpdateRefOrDriverNodeColor(__instance, targetField);
									};

									UpdateRefOrDriverNodeColor(__instance, targetField);

									var newSlot = refDriverNodeDataSlot.AddSlot(__instance.ReferenceID.ToString());
									newSlot.PersistentSelf = false;
									newSlot.Tag = __instance.LocalUser.ReferenceID.ToString();

									var destroyOnUserLeave = newSlot.AttachComponent<DestroyOnUserLeave>();
									destroyOnUserLeave.TargetUser.User.Value = __instance.LocalUser.ReferenceID;
									destroyOnUserLeave.Persistent = false;

									var destroyProxy = __instance.Slot.AttachComponent<DestroyProxy>();
									destroyProxy.Persistent = false;
									destroyProxy.DestroyTarget.Value = newSlot.ReferenceID;
								}
								else
								{
									UpdateRefOrDriverNodeColor(__instance, targetField);
								}
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

					if (root.Tag != COLOR_SET_TAG)
					{
						if (dataRootSlot == null || dataRootSlot.World != Engine.Current.WorldManager.FocusedWorld)
						{
							dataRootSlot = __instance.World.AssetsSlot.FindOrAdd(DATA_ROOT_SLOT_NAME, false);
							dataRootSlot.GetComponentOrAttach<AssetOptimizationBlock>();
						}
						
						if (regularNodeDataSlot == null || regularNodeDataSlot.World != Engine.Current.WorldManager.FocusedWorld)
						{
							regularNodeDataSlot = dataRootSlot.FindOrAdd(REGULAR_NODE_DATA_SLOT_NAME, false);
							regularNodeDataSlot.GetComponentOrAttach<AssetOptimizationBlock>();
						}

						__instance.RunInUpdates(3, () =>
						{
							Slot nodeDataSlot = null;

							if (Config.GetValue(ADD_REGULAR_NODES_TO_DATA_SLOT))
							{
								nodeDataSlot = regularNodeDataSlot.FindOrAdd(__instance.ReferenceID.ToString(), false);
								TrySetSlotTag(nodeDataSlot, __instance.LocalUser.ReferenceID.ToString());

								var nodeRefField = nodeDataSlot.GetComponentOrAttach<ReferenceField<LogixNode>>();
								nodeRefField.Reference.Value = __instance.ReferenceID;

								var destroyProxy = __instance.ActiveVisual.GetComponentOrAttach<DestroyProxy>();
								destroyProxy.DestroyTarget.Value = nodeDataSlot.ReferenceID;
								destroyProxy.Persistent = false;
							}

							var backgroundImage = GetBackgroundImageForNode(__instance);
							if (backgroundImage != null)
							{
								if (Config.GetValue(ADD_REGULAR_NODES_TO_DATA_SLOT))
								{
									var bgImageRefField = nodeDataSlot.GetComponentOrAttach<ReferenceField<IField<color>>>();
									bgImageRefField.Reference.Value = backgroundImage.TryGetField<color>("Tint").ReferenceID;
									bgImageRefField.UpdateOrder = 0;
								}

								if (root.Tag == "Disabled")
								{
									TrySetImageTint(backgroundImage, Config.GetValue(NODE_ERROR_COLOR));
								}
								else
								{
									color colorToSet;

									colorToSet = ComputeColorForLogixNode(__instance);

									TrySetImageTint(backgroundImage, colorToSet);

									if (Config.GetValue(MAKE_CONNECT_POINTS_FULL_ALPHA))
									{
										// Make the connect points on nodes have full alpha to make it easier to read type information
										foreach (Image img in backgroundImage.Slot.GetComponentsInChildren<Image>())
										{
											if (img != backgroundImage)
											{
												TrySetImageTint(img, img.Tint.Value.SetA(1f));

												//if (Config.GetValue(ADD_REGULAR_NODES_TO_DATA_SLOT))
												//{
													//var connectPointColorRefField = nodeDataSlot.AttachComponent<ReferenceField<IField<color>>>();
													//connectPointColorRefField.Reference.Value = img.TryGetField<color>("Tint").ReferenceID;
													//connectPointColorRefField.UpdateOrder = 1;
												//}
											}
										}
									}

									if (Config.GetValue(ENABLE_TEXT_CONTRAST) || Config.GetValue(USE_STATIC_TEXT_COLOR))
									{
										// set node's text color, there could be multiple text components that need to be colored
										// need to wait 3 updates because who knows why...
										__instance.RunInUpdates(3, () =>
										{
											foreach (Text text in GetTextListForNode(__instance))
											{
												TrySetTextColor(text, GetTextColor(colorToSet));

												if (Config.GetValue(ADD_REGULAR_NODES_TO_DATA_SLOT))
												{
													var textColorRefField = nodeDataSlot.AttachComponent<ReferenceField<IField<color>>>();
													textColorRefField.Reference.Value = text.TryGetField<color>("Color").ReferenceID;
													textColorRefField.UpdateOrder = 2;
												}
											}
										});
									}

									TrySetSlotTag(root, COLOR_SET_TAG);
								}
							}
						});
					}
				}
			}
		}
	}
}