using Sirenix.OdinInspector;
using UnityEngine;

namespace Battle.MagicFuncs {
  public struct MagicArgs {
    public bool IsEnd;
    public Unit Source;
    public Unit Target;
  }

  public abstract class MagicTemplate : ScriptableObject {
    [ReadOnly]
    public string MagicId;
    public abstract bool IgnoreOnEnd { get; }

    public abstract void Run(BattleManager battleManager, Context context, MagicArgs args);
  }
}