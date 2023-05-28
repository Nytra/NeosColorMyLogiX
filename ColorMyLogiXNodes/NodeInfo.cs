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

		public class RefDriverNodeInfo
		{
			public LogixNode node;
			public ISyncRef syncRef;

			public void UpdateColor(IChangeable iChangeable)
			{
				if (node == null || syncRef == null)
				{
					Warn("Tried to update color for null node or null syncref.");
					return;
				}
				Debug($"UpdateColor called for node {node.Name} {node.ReferenceID}.");
				UpdateRefOrDriverNodeColor(node, syncRef);
			}
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

		private static bool NodeInfoSetContainsNode(LogixNode node)
		{
			return nodeInfoSet.Any(nodeInfo => nodeInfo.node == node);
		}

		private static NodeInfo GetNodeInfoForNode(LogixNode node)
		{
			return nodeInfoSet.FirstOrDefault(nodeInfo => nodeInfo.node == node) ?? nullNodeInfo;
		}

		private static void NodeInfoRemove(NodeInfo nodeInfo)
		{
			if (nodeInfo == null)
			{
				Debug("Tried to remove null from nodeInfoSet");
				TryTrimExcessNodeInfo();
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

			TryTrimExcessNodeInfo();

		}

		private static void TryTrimExcessNodeInfo()
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

		private static void TryTrimExcessRefDriverNodeInfo()
		{
			try
			{
				refDriverNodeInfoSet.TrimExcess();
			}
			catch (Exception e)
			{
				Error("Error while trying to trim excess RefDriverNodeInfo's. " + e.ToString());
			}
		}

		private static void NodeInfoSetClear()
		{
			foreach (NodeInfo nodeInfo in nodeInfoSet)
			{
				nodeInfo.node = null;
				nodeInfo.bgField = null;
				nodeInfo.textFields = null;
			}
			nodeInfoSet.Clear();
			TryTrimExcessNodeInfo();
		}

		private static void RefDriverNodeInfoSetClear()
		{
			foreach (RefDriverNodeInfo refDriverNodeInfo in refDriverNodeInfoSet)
			{
				refDriverNodeInfo.syncRef.Changed -= refDriverNodeInfo.UpdateColor;
				refDriverNodeInfo.node = null;
				refDriverNodeInfo.syncRef = null;
			}
			refDriverNodeInfoSet.Clear();
			TryTrimExcessRefDriverNodeInfo();
		}
	}
}