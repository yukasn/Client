using UnityEngine;

namespace GameCore.MagicFuncs {
  [CreateAssetMenu(menuName = "模板/效果/加行为树")]
  public class AddBehavior : MagicFuncBase {
    public override bool IgnoreOnEnd => false;
    public string BehaviorId;

    public override void Run(Battle battleManager, Context context, MagicArgs args) {
      if (args.IsEnd) {
        if (context is BuffContext buffContext && buffContext.Behavior != null) {
          if (battleManager.BehaviorManager.RemoveBehavior(buffContext.Behavior.RuntimeId)) {
            buffContext.Behavior = null;
          }
        }
      } else {
        var behavior = battleManager.BehaviorManager.AddBehavior(BehaviorId, args.Source, args.Target);
        if (behavior != null && context is BuffContext buffContext) {
          buffContext.Behavior = behavior;
        }
      }
    }
  }
}