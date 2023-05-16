using FrooxEngine.LogiX;
using FrooxEngine;
using NeosModLoader;
using System.Collections.Generic;
using BaseX;
using System;
using System.Linq;

namespace ColorMyLogixNodes
{
	public partial class ColorMyLogixNodes : NeosMod
	{
		public class NodeInfo
		{
			public LogixNode node;
			public IField<color> bgField;
			public HashSet<IField<color>> textFields;
		}

        private static void NodeInfoSetBgColor(NodeInfo nodeInfo, color c)
        {
            NodeInfo outNodeInfo = GetNodeInfoForNode(nodeInfo.node);

            if (outNodeInfo != null && outNodeInfo != nullNodeInfo)
            {
                if (outNodeInfo.bgField.IsRemoved)
                {
                    RemoveNodeInfo(nodeInfo);
                }
                else
                {
                    if (outNodeInfo.bgField.Value != c) outNodeInfo.bgField.Value = c;
                }
            }
            else
            {
                Debug("Could not set Bg Color. NodeInfo was not found.");
            }
        }


        private static void NodeInfoSetTextColor(NodeInfo nodeInfo, color c)
        {
            NodeInfo outNodeInfo = GetNodeInfoForNode(nodeInfo.node);

            if (outNodeInfo != null && outNodeInfo != nullNodeInfo)
            {
                foreach (IField<color> field in outNodeInfo.textFields)
                {
                    if (field.IsRemoved)
                    {
                        RemoveNodeInfo(nodeInfo);
                        return;
                    }

                    if (field.Value != c) field.Value = c;
                }
            }
            else
            {
                Debug("Could not set Text Color. NodeInfo was not found.");
            }
        }


        private static bool NodeInfoListContainsNode(LogixNode node)
        {
            return nodeInfoSet.Any(nodeInfo => nodeInfo.node == node);
        }

        private static NodeInfo GetNodeInfoForNode(LogixNode node)
        {
            return nodeInfoSet.FirstOrDefault(nodeInfo => nodeInfo.node == node) ?? nullNodeInfo;
        }

        private static void RemoveNodeInfo(NodeInfo nodeInfo)
        {
            if (nodeInfo == null)
            {
                Debug("Attempted to remove null from nodeInfoSet");
                TryTrimExcess();
                return;
            }

            if (!nodeInfoSet.Contains(nodeInfo))
            {
                Debug("NodeInfo not found in nodeInfoSet.");
                return;
            }

            if (nodeInfoSet.TryGetValue(nodeInfo, out NodeInfo existingNodeInfo))
            {
                existingNodeInfo.node = null;
                existingNodeInfo.bgField = null;
                existingNodeInfo.textFields = null;

                if (nodeInfoSet.Remove(nodeInfo))
                {
                    Debug($"NodeInfo removed. New size of nodeInfoSet: {nodeInfoSet.Count}");
                }
                else
                {
                    Debug("NodeInfo not found in nodeInfoSet (unexpected error).");
                }
            }
            else
            {
                Debug("Failed to retrieve NodeInfo from nodeInfoSet.");
            }

            TryTrimExcess();
        }


        private static void TryTrimExcess()
		{
			try
			{
				nodeInfoSet.TrimExcess();
			}
			catch (Exception e)
			{
				Error("Error while trying to trim excess NodeInfo's. " + e.ToString());
			}
		}
	
		private static void NodeInfoListClear()
		{
			foreach (NodeInfo nodeInfo in nodeInfoSet)
			{
				nodeInfo.node = null;
				nodeInfo.bgField = null;
				nodeInfo.textFields = null;
			}
			nodeInfoSet.Clear();
			TryTrimExcess();
		}
	}
}