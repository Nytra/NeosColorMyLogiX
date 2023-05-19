﻿using FrooxEngine.LogiX;
using FrooxEngine;
using NeosModLoader;
using System.Linq;
using System;
using System.Threading;
using BaseX;
using Leap.Unity.Attributes;
using Leap.Unity.Query;

namespace ColorMyLogixNodes
{
	public partial class ColorMyLogixNodes : NeosMod
	{
		private static bool IsNodeInvalid(NodeInfo nodeInfo)
		{
			return (nodeInfo == null ||
				   nodeInfo.node == null ||
				   nodeInfo.node.IsRemoved ||
				   nodeInfo.node.IsDestroyed ||
				   nodeInfo.node.IsDisposed ||
				   nodeInfo.node.Slot == null ||
				   nodeInfo.node.Slot.IsRemoved ||
				   nodeInfo.node.Slot.IsDestroyed ||
				   nodeInfo.node.Slot.IsDisposed ||
				   nodeInfo.node.World == null ||
				   nodeInfo.node.World.IsDestroyed ||
				   nodeInfo.node.World.IsDisposed);
		}

		private static int Clamp(int value, int minValue, int maxValue)
		{
			return Math.Min(Math.Max(value, minValue), maxValue);
		}

		private static void RefDriverNodeThread()
		{
			while (true)
			{
				try
				{
					if (Config.GetValue(MOD_ENABLED) && Config.GetValue(AUTO_UPDATE_REF_AND_DRIVER_NODES))
					{
						foreach (RefDriverNodeInfo refDriverNodeInfo in refDriverNodeInfoSet.ToList())
						{
							if (refDriverNodeInfo.syncRef == null || refDriverNodeInfo.syncRef.IsRemoved || refDriverNodeInfo.syncRef.Parent == null || refDriverNodeInfo.syncRef.Parent.IsRemoved)
							{
								Debug("=== Unsubscribing from a node ===");
								refDriverNodeInfo.syncRef.Changed -= refDriverNodeInfo.UpdateColor;
								refDriverNodeInfoSet.Remove(refDriverNodeInfo);
								Debug("New refDriverNodeInfoSet size: " + refDriverNodeInfoSet.Count.ToString());
							}
							else
							{
								//IWorldElement outSyncRefTarget;
								//syncRefTargetMap.TryGetValue(syncRef, out outSyncRefTarget);
								//if (syncRef.Target != outSyncRefTarget)
								//{
								//	// node could be null?
								//	syncRefTargetMap[syncRef] = syncRef.Target;
								//	UpdateRefOrDriverNodeColor(syncRef.Parent as LogixNode, syncRef);
								//}
							}

							if (THREAD_INNER_SLEEP_TIME_MILLISECONDS > 0)
							{
								Thread.Sleep(THREAD_INNER_SLEEP_TIME_MILLISECONDS);
							}
						}
					}

					Thread.Sleep(10000);
				}
				catch (Exception e)
				{
					Warn($"Ref driver node thread error! This is probably fine.{Environment.NewLine}{e}");
					Warn("Continuing thread...");
					continue;
				}
			}
		}

		private static void StandardNodeThread()
		{
			while (true)
			{
				try
				{
					if (Config.GetValue(MOD_ENABLED) && (Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED) || Config.GetValue(USE_AUTO_RANDOM_COLOR_CHANGE)))
					{
						foreach (NodeInfo nodeInfo in nodeInfoSet.ToList())
						{
							if (IsNodeInvalid(nodeInfo))
							{
								NodeInfoRemove(nodeInfo);
								Thread.Sleep(THREAD_INNER_SLEEP_TIME_MILLISECONDS);
								continue;
							}

							if (Config.GetValue(USE_AUTO_RANDOM_COLOR_CHANGE))
							{
								if (nodeInfo.node.World != Engine.Current.WorldManager.FocusedWorld) continue;

								color c = ComputeColorForLogixNode(nodeInfo.node);
								nodeInfo.node.RunInUpdates(0, () =>
								{
									if (nodeInfo == null || nodeInfo.node == null)
									{
										NodeInfoRemove(nodeInfo);
										return;
									}
									// if it didn't already get removed in another thread uwu
									if (nodeInfoSet.Contains(nodeInfo))
									{
										NodeInfoSetBgColor(nodeInfo, c);
									}
									if (nodeInfoSet.Contains(nodeInfo))
									{
										NodeInfoSetTextColor(nodeInfo, GetTextColor(c));
									}
								});
							}

							if (THREAD_INNER_SLEEP_TIME_MILLISECONDS > 0)
							{
								Thread.Sleep(THREAD_INNER_SLEEP_TIME_MILLISECONDS);
							}
						}
					}
					if (!Config.GetValue(USE_AUTO_RANDOM_COLOR_CHANGE))
					{
						manualResetEvent.Reset();
						manualResetEvent.WaitOne(10000);
					}
					else
					{
						manualResetEvent.Reset();
						manualResetEvent.WaitOne(Math.Min(Math.Max(Config.GetValue(AUTO_RANDOM_COLOR_CHANGE_THREAD_SLEEP_TIME), 2500), 30000));
						manualResetEvent.WaitOne(Clamp(Config.GetValue(AUTO_RANDOM_COLOR_CHANGE_THREAD_SLEEP_TIME), 2500, 30000));
					}
				}
				catch (Exception e)
				{
					Warn($"Standard node thread error! This is probably fine.{Environment.NewLine}{e}");
					Warn("Continuing thread...");
					continue;
				}
			}
		}
	}
}