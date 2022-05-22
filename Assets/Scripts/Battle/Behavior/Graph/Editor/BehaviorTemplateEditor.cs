using Battle.BehaviorFuncs;
using System;
using UnityEditor;
using XNodeEditor;

namespace Battle {
  [CustomNodeGraphEditor(typeof(BehaviorTemplate))]
  public class BehaviorTemplateEditor : NodeGraphEditor {
    public override void OnGUI() {
      window.titleContent.text = (target as BehaviorTemplate).BehaviorId;
    }

    /// <summary> 
    /// Overriding GetNodeMenuName lets you control if and how nodes are categorized.
    /// In this example we are sorting out all node types that are not in the XNode.Examples namespace.
    /// </summary>
    public override string GetNodeMenuName(Type type) {
      if (type.Namespace.StartsWith("Battle")) {
        return base.GetNodeMenuName(type);
      } else return null;
    }

    protected override bool CheckAddNode(Type type) {
      if (typeof(Root).IsAssignableFrom(type)) {
        var root = target.nodes.Find(node => node is Root);
        if (root) {
          EditorUtility.DisplayDialog("提示", $"已添加根节点{root.GetType().Name},请勿重复添加!", "确定");
          return false;
        }
      }
      return true;
    }
  }
}