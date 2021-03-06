using Cysharp.Threading.Tasks;
using UnityEngine;

namespace GameCore {
  public abstract class BattleOperation : IPoolObject {
    public Unit Unit;

    public abstract UniTask DoOperation();
    public virtual void Release() { 
      Unit = null;
    }
  }

  public class EndTurnOp : BattleOperation {
    public override UniTask DoOperation() {
      Debug.Log("结束回合");
      Unit.Player.EndTurnFlag = true;
      return UniTask.CompletedTask;
    }
  }

  public class PlayCardOp : BattleOperation {
    public Card Card;
    public Unit MainTarget;

    public override async UniTask DoOperation() {
      Debug.Log($"{Card.Owner.RuntimeId}:{Card.Owner.Name} 对 {MainTarget.RuntimeId}:{MainTarget.Name} 使用了卡牌{Card.CardTemplate.name} Lv:{Card.Lv}");
      await Card.Cast(MainTarget);
    }

    public override void Release() {
      base.Release();
      Card = null;
      MainTarget = null;
    }
  }
}