using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

namespace GameCore.BehaviorFuncs {
  [CreateNodeMenu("节点/行为/卡牌/获取卡牌数量")]
  public class GetCardCount : ActionNode {
    [LabelText("目标单位")]
    public NodeParamKey TargetUnit;
    [LabelText("卡牌堆类型")]
    public CardHeapType CardHeapType;
    [LabelText("存值")]
    public NodeParamKey TargetKey;

    public override UniTask<NodeResult> Run(Behavior behavior, Context context) {
      Unit targetUnit = behavior.GetUnit(TargetUnit);
      if(targetUnit == null) {
        return UniTask.FromResult(NodeResult.False);
      }
      behavior.SetInt(TargetKey, targetUnit.CardHeapDict[CardHeapType].Count);
      return UniTask.FromResult(NodeResult.True);
    }
  }
}