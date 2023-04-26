using FrooxEngine.LogiX;
using FrooxEngine;
using NeosModLoader;
using System.Collections.Generic;
using BaseX;
using System.Security.Permissions;

namespace ColorMyLogixNodes
{
	public partial class ColorMyLogixNodes : NeosMod
	{
		public class NodeInfo
		{
			public LogixNode node;
			public ISyncRef syncRef;
			public IWorldElement prevTarget;
			public IField<color> bgField;
			public List<IField<color>> textFields;
		}

		private static void NodeInfoSetBgColor(NodeInfo nodeInfo, color c)
		{
			int index = standardNodeInfoList.IndexOf(nodeInfo);
			if (index >= 0)
			{
				standardNodeInfoList[index].bgField.Value = c;
			}
			else
			{
				Debug("Index was -1");
			}
		}

		private static void NodeInfoSetTextColor(NodeInfo nodeInfo, color c)
		{
			int index = standardNodeInfoList.IndexOf(nodeInfo);
			if (index >= 0)
			{
				foreach (IField<color> field in standardNodeInfoList[index].textFields)
				{
					if (field.IsRemoved)
					{
						NodeInfoRemove(nodeInfo, ref standardNodeInfoList);
						return;
					}
					else
					{
						field.Value = c;
					}
				}
			}
			else
			{
				Debug("Index was -1");
			}
		}

		private static bool NodeInfoListContainsNode(LogixNode node, ref List<NodeInfo> list)
		{
			foreach (NodeInfo nodeInfo in list)
			{
				if (nodeInfo.node == node) return true;
			}
			return false;
		}

		private static NodeInfo GetNodeInfoForNode(LogixNode node, ref List<NodeInfo> list)
		{
			foreach (NodeInfo nodeInfo in list)
			{
				if (nodeInfo.node == node) return nodeInfo;
			}
			return nullNodeInfo;
		}

		private static void NodeInfoRemove(NodeInfo nodeInfo, ref List<NodeInfo> list)
		{
			if (!list.Contains(nodeInfo))
			{
				Debug("NodeInfo was not in list.");
				return;
			}
			int index = list.IndexOf(nodeInfo);
			list[index].node = null;
			list[index].syncRef = null;
			list[index].prevTarget = null;
			//list[index] = null;
			if (list.Remove(nodeInfo))
			{
				Debug("NodeInfo was removed from list. New size of list: " + list.Count.ToString());
			}
			else
			{
				Debug("NodeInfo was not in list (somehow?).");
			}
			list.TrimExcess();
		}

		private static void NodeInfoListClear(ref List<NodeInfo> list)
		{
			for (int i = 0; i < list.Count; i++)
			{
				list[i].node = null;
				list[i].syncRef = null;
				list[i].prevTarget = null;
				list[i].bgField = null;
				list[i].textFields = null;
			}
			list.Clear();
			list.TrimExcess();
		}
	}
}