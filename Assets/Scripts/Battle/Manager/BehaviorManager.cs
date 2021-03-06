using Cysharp.Threading.Tasks;
using GameCore.BehaviorFuncs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameCore {
  public enum TrickTime {
    [InspectorName("无")]
    NONE,
    [InspectorName("回合开始")]
    ON_START_TURN,
    [InspectorName("回合结束")]
    ON_END_TURN,
    [InspectorName("回合中等待指令")]
    ON_TURN_WAIT_OP,
    [InspectorName("造成伤害前")]
    ON_BEFORE_DAMAGE,
    [InspectorName("造成伤害后")]
    ON_LATE_DAMAGE,
    [InspectorName("被造成伤害前")]
    ON_BEFORE_DAMAGED,
    [InspectorName("被造成伤害后")]
    ON_LATE_DAMAGED,
    [InspectorName("单位濒死时")]
    ON_UNIT_DYING,
    [InspectorName("单位死亡时")]
    ON_UNIT_DEAD,
  }

  public class BehaviorManager : TemplateManager<BehaviorGraph> {
    private int IncId;
    private Dictionary<int, Behavior> Behaviors = new Dictionary<int, Behavior>();
    private Dictionary<TrickTime, List<int>> BehaviorTimes = new Dictionary<TrickTime, List<int>>();

    public BehaviorManager(Battle battle) : base(battle) {
      foreach (TrickTime behaviorTime in Enum.GetValues(typeof(TrickTime))) {
        BehaviorTimes.Add(behaviorTime, new List<int>());
      }
    }

    public async UniTask RunRoot(TrickTime behaviorTime, Unit[] units, Context context = null) {
      foreach (var unit in units) {
        await RunRoot(behaviorTime, unit, context);
      }
    }

    public async UniTask RunRoot(TrickTime behaviorTime, Unit unit = null, Context context = null) {
      // 优先更新Buff回合数
      await Battle.BuffManager.Update(behaviorTime, unit);

      var behaviorList = TempList<int>.Get();
      behaviorList.AddRange(BehaviorTimes[behaviorTime]);
      foreach (int runtimeId in behaviorList) {
        var behavior = Behaviors[runtimeId];
        if (unit == null || behavior.Unit == null || unit == behavior.Unit) {
          await behavior.Run<Root>(context);
        }
      }
      TempList<int>.CleanUp();
    }

    public async UniTask<Behavior> Add(string behaviorId, Unit source = null, Unit target = null) {
      if (!Templates.TryGetValue(behaviorId, out var behaviorGraph)) {
        Debug.LogError($"BehaviorManager.Add error, behaviorGraph is not preload. Id:{behaviorId}");
        return null;
      }

      Behavior behavior = Battle.ObjectPool.Get<Behavior>();
      behavior.Init(Battle, ++IncId, behaviorGraph, source, target);
      Behaviors.Add(behavior.RuntimeId, behavior);
      BehaviorTimes[behaviorGraph.BehaviorTime].Add(behavior.RuntimeId);

      await behavior.Run<Init>();

      if (!Behaviors.ContainsKey(behavior.RuntimeId)) {
        return null;
      }

      return behavior;
    }

    public async UniTask<bool> Remove(int runtimeId) {
      if (!Behaviors.TryGetValue(runtimeId, out var behavior)) {
        Debug.LogError($"BehaviorManager.Remove error, behavior is not exists. runtimeId:{runtimeId}");
        return false;
      }

      await behavior.Run<Finalize>();

      Behaviors.Remove(runtimeId);
      BehaviorTimes[behavior.BehaviorGraph.BehaviorTime].Remove(runtimeId);
      Battle.ObjectPool.Release(behavior);

      return true;
    }
  }
}