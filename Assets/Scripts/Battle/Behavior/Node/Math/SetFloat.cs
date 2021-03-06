using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

namespace GameCore.BehaviorFuncs {
  [CreateNodeMenu("节点/行为/数学/设置Float")]
  public class SetFloat : ActionNode {
    [LabelText("数据源")]
    public NodeFloatParam Source;
    [LabelText("存值")]
    public NodeParamKey TargetKey;

    public override UniTask<NodeResult> Run(Behavior behavior, Context context) {
      behavior.SetFloat(TargetKey, behavior.GetFloat(Source));
      return UniTask.FromResult(NodeResult.True);
    }
  }
}