using FrooxEngine.LogiX;
using FrooxEngine;
using NeosModLoader;
using System.Linq;
using System;
using System.Threading;
using BaseX;

namespace ColorMyLogixNodes
{
	public partial class ColorMyLogixNodes : NeosMod
	{
        private static void ProcessRefDriverNodes()
        {
            while (true)
            {
                try
                {
                    bool isEnabled = Config.GetValue(MOD_ENABLED);
                    bool autoUpdateRefAndDriverNodes = Config.GetValue(AUTO_UPDATE_REF_AND_DRIVER_NODES);

                    if (isEnabled && autoUpdateRefAndDriverNodes)
                    {
                        foreach (ISyncRef syncRef in syncRefTargetMap.Keys.ToList())
                        {
                            if (IsSyncRefInvalid(syncRef))
                            {
                                UnsubscribeFromNode(syncRef);
                            }
                            else
                            {
                                UpdateRefDriverNode(syncRef);
                            }

                            if (THREAD_INNER_SLEEP_TIME_MILLISECONDS > 0)
                            {
                                Thread.Sleep(THREAD_INNER_SLEEP_TIME_MILLISECONDS);
                            }
                        }
                    }

                    Thread.Sleep(250);
                }
                catch (Exception e)
                {
                    Warn($"Ref driver node thread error! This is probably fine.{Environment.NewLine}{e}");
                    Warn("Continuing thread...");
                    continue;
                }
            }
        }

        private static bool IsSyncRefInvalid(ISyncRef syncRef)
        {
            return syncRef == null || syncRef.IsRemoved || syncRef.Parent == null || syncRef.Parent.IsRemoved;
        }

        private static void UnsubscribeFromNode(ISyncRef syncRef)
        {
            Debug("=== Unsubscribing from a node ===");
            syncRefTargetMap[syncRef] = null;
            syncRefTargetMap.Remove(syncRef);
            Debug($"New syncRefTargetMap size: {syncRefTargetMap.Count}");
        }

        private static void UpdateRefDriverNode(ISyncRef syncRef)
        {
            if (syncRefTargetMap.TryGetValue(syncRef, out IWorldElement existingSyncRefTarget) && syncRef.Target != existingSyncRefTarget)
            {
                syncRefTargetMap[syncRef] = syncRef.Target;
                UpdateRefOrDriverNodeColor(syncRef.Parent as LogixNode, syncRef);
            }
        }


        private static void ProcessStandardNodes()
        {
            while (true)
            {
                try
                {
                    bool isEnabled = Config.GetValue(MOD_ENABLED);
                    bool updateOnConfigChanged = Config.GetValue(UPDATE_NODES_ON_CONFIG_CHANGED);
                    bool useAutoRandomColorChange = Config.GetValue(USE_AUTO_RANDOM_COLOR_CHANGE);

                    if (isEnabled && (updateOnConfigChanged || useAutoRandomColorChange))
                    {
                        foreach (NodeInfo nodeInfo in nodeInfoSet.ToList())
                        {
                            if (IsNodeInvalid(nodeInfo))
                            {
                                RemoveNodeInfo(nodeInfo);
                                Thread.Sleep(THREAD_INNER_SLEEP_TIME_MILLISECONDS);
                                continue;
                            }

                            if (useAutoRandomColorChange && nodeInfo.node.World == Engine.Current.WorldManager.FocusedWorld)
                            {
                                color newColor = ComputeColorForLogixNode(nodeInfo.node);
                                UpdateNodeColors(nodeInfo, newColor);
                            }

                            if (THREAD_INNER_SLEEP_TIME_MILLISECONDS > 0)
                            {
                                Thread.Sleep(THREAD_INNER_SLEEP_TIME_MILLISECONDS);
                            }
                        }
                    }

                    int waitTime = useAutoRandomColorChange
                        ? Clamp(Config.GetValue(AUTO_RANDOM_COLOR_CHANGE_THREAD_SLEEP_TIME), 2500, 30000)
                        : 10000;

                    manualResetEvent.Reset();
                    manualResetEvent.WaitOne(waitTime);
                }
                catch (Exception e)
                {
                    Warn($"Standard node thread error! This is probably fine.{Environment.NewLine}{e}");
                    Warn("Continuing thread...");
                    continue;
                }
            }
        }
        private static int Clamp(int value, int minValue, int maxValue)
        {
            return Math.Min(Math.Max(value, minValue), maxValue);
        }

        private static bool IsNodeInvalid(NodeInfo nodeInfo)
        {
            return nodeInfo == null ||
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
                   nodeInfo.node.World.IsDisposed;
        }

        private static void UpdateNodeColors(NodeInfo nodeInfo, color newColor)
        {
            nodeInfo.node.RunInUpdates(0, () =>
            {
                if (IsNodeInvalid(nodeInfo))
                {
                    RemoveNodeInfo(nodeInfo);
                    return;
                }

                if (nodeInfoSet.Contains(nodeInfo))
                {
                    NodeInfoSetBgColor(nodeInfo, newColor);
                    NodeInfoSetTextColor(nodeInfo, GetTextColor(newColor));
                }
            });
        }





    }
}