using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

namespace GameCore.BehaviorFuncs {
  [CreateNodeMenu("节点/行为/行为树/获取行为树来源单位")]
  public class GetBehaviorSource : ActionNode {
    [LabelText("存值")]
    public NodeParamKey TargetKey;

    public override UniTask<NodeResult> Run(Behavior behavior, Context context) {
      if (behavior.Unit == null) {
        return UniTask.FromResult(NodeResult.False);
      }
      behavior.SetInt(TargetKey, behavior.SourceUnit.RuntimeId);
      return UniTask.FromResult(NodeResult.True);
    }
  }
}