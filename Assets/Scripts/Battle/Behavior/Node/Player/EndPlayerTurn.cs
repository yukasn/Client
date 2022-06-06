using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

namespace GameCore.BehaviorFuncs {
  [CreateNodeMenu("节点/行为/玩家/结束玩家回合")]
  public class EndPlayerTurn : ActionNode {
    [LabelText("目标单位")]
    public NodeParamKey TargetUnit;

    public override UniTask<bool> Run(Behavior behavior, Context context) {
      var targetUnit = behavior.GetUnit(TargetUnit);
      if (targetUnit == null) {
        return UniTask.FromResult(false);
      }
      return UniTask.FromResult(targetUnit.EndTurn());
    }
  }
}