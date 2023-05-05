using FrooxEngine.LogiX;
using FrooxEngine;
using NeosModLoader;
using System.Collections.Generic;
using BaseX;
using System;

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
			NodeInfo outNodeInfo = null;
			if (nodeInfoSet.TryGetValue(nodeInfo, out outNodeInfo))
			{
				if (outNodeInfo.bgField.IsRemoved)
				{
					NodeInfoRemove(nodeInfo);
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
			NodeInfo outNodeInfo = null;
			if (nodeInfoSet.TryGetValue(nodeInfo, out outNodeInfo))
			{
				foreach (IField<color> field in outNodeInfo.textFields)
				{
					if (field.IsRemoved)
					{
						NodeInfoRemove(nodeInfo);
						return;
					}
					else
					{
						if (field.Value != c) field.Value = c;
					}
				}

			}
			else
			{
				Debug("Could not set Text Color. NodeInfo was not found.");
			}
		}

		private static bool NodeInfoListContainsNode(LogixNode node)
		{
			foreach (NodeInfo nodeInfo in nodeInfoSet)
			{
				if (nodeInfo.node == node) return true;
			}
			return false;
		}

		private static NodeInfo GetNodeInfoForNode(LogixNode node)
		{
			foreach (NodeInfo nodeInfo in nodeInfoSet)
			{
				if (nodeInfo.node == node) return nodeInfo;
			}
			return nullNodeInfo;
		}

		private static void NodeInfoRemove(NodeInfo nodeInfo)
		{
			if (nodeInfo == null)
			{
				Debug("Tried to remove null from nodeInfoSet");
				TryTrimExcess();
				return;
			}
			if (!nodeInfoSet.Contains(nodeInfo))
			{
				Debug("NodeInfo was not in nodeInfoSet.");
				return;
			}
			NodeInfo outNodeInfo = null;
			nodeInfoSet.TryGetValue(nodeInfo, out outNodeInfo);
			outNodeInfo.node = null;
			outNodeInfo.bgField = null;
			outNodeInfo.textFields = null;
			if (nodeInfoSet.Remove(nodeInfo))
			{
				Debug("NodeInfo removed. New size of nodeInfoSet: " + nodeInfoSet.Count.ToString());
			}
			else
			{
				Debug("NodeInfo was not in nodeInfoSet (this should never happen).");
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
				Debug("Error while trying to trim excess NodeInfos. " + e.ToString());
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