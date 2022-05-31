using Cysharp.Threading.Tasks;
using GameCore.BehaviorFuncs;
using GameCore.MagicFuncs;
using System;
using UnityEngine;

namespace GameCore {
  public enum BattleState {
    None,
    Load,
    Run,
    Exit,
  }

  public enum BattleTurnPhase {
    ON_BEFORE_TURN,
    ON_TURN,
    ON_LATE_TURN,
  }

  public class Battle {
    public BattleState BattleState { get; private set; }
    public BattleData BattleData { get; private set; }
    public Blackboard Blackboard { get; private set; }
    public ObjectPool ObjectPool { get; private set; }
    public Player CurPlayer { get; private set; }
    public Player SelfPlayer { get; private set; }

    #region Manager
    public UnitManager UnitManager { get; private set; }
    public BuffManager BuffManager { get; private set; }
    public MagicManager MagicManager { get; private set; }
    public BehaviorManager BehaviorManager { get; private set; }
    public AttribManager AttribManager { get; private set; }
    public CardManager CardManager { get; private set; }
    public LevelManager LevelManager { get; private set; }
    public SkillManager SkillManager { get; private set; }

    public PlayerManager PlayerManager { get; private set; }
    public DamageManager DamageManager { get; private set; }
    #endregion

    #region Static
    public static Battle Instance { get; private set; }
    #endregion

    public static bool Enter(BattleData battleData) {
      if(Instance != null) {
        Debug.LogError("上一场战斗未结束!");
        return false;
      }
      // 战斗实例初始化
      Instance = new Battle(battleData);
      // 首先是加载资源
      Instance.BattleState = BattleState.Load;
      Instance.Update();
      return true;
    }

    private Battle(BattleData battleData) {
      BattleData = battleData;
      UnitManager = new UnitManager(this);
      BuffManager = new BuffManager(this);
      MagicManager = new MagicManager(this);
      BehaviorManager = new BehaviorManager(this);
      AttribManager = new AttribManager(this);
      CardManager = new CardManager(this);
      LevelManager = new LevelManager(this);
      SkillManager = new SkillManager(this);

      PlayerManager = new PlayerManager(this);
      DamageManager = new DamageManager(this);

      Blackboard = new Blackboard();
      ObjectPool = new ObjectPool();
    }

    public async void Update() {
      while (BattleState != BattleState.None) {
        switch (BattleState) {
          case BattleState.Load:
            await Load();
            break;
          case BattleState.Run:
            try {
              await Run();
            } catch (Exception e) {
              if (e.GetType() != typeof(OperationCanceledException)) {
                Debug.LogError(e.GetType());
              }
            }
            break;
          case BattleState.Exit:
            await Clear();
            break;
        }
      }
    }

    private async UniTask Load() {
      // 加载关卡模板
      var levelTemplate = await LevelManager.Preload(BattleData.LevelId);
      // 加载关卡行为树
      foreach (var behaviorId in levelTemplate.BehaviorIds) {
        var behavior = await PreloadBehavior(behaviorId);
        if (behavior) {
          BehaviorManager.Add(behaviorId);
        }
      }      
      // 先加载我方角色
      foreach (var unitData in BattleData.PlayerData.UnitData) {
        await PreloadUnit(unitData);
      }
      // 暂定我方先手
      SelfPlayer = PlayerManager.Create(BattleData.PlayerData);
      // 再加载敌方角色
      foreach (var enemyData in levelTemplate.EnemyData) {
        foreach (var unitData in enemyData.UnitData) {
          await PreloadUnit(unitData);
        }
        PlayerManager.Create(enemyData);
      }

      BattleState = BattleState.Run;
      Debug.Log("战斗资源加载完毕");
    }

    private async UniTask PreloadUnit(UnitData unitData) {
      var unitTemplate = await UnitManager.Preload(unitData.TemplateId);
      await AttribManager.Preload(unitTemplate.AttribId);
      // 预加载卡牌
      foreach (var cardData in unitData.CardData) {
        var cardTemplate = await CardManager.Preload(cardData.TemplateId);
        foreach (var item in cardTemplate.LvCardItems) {
          var skillTemplate = await SkillManager.Preload(item.SkillId);
          foreach (var skillEvent in skillTemplate.SKillEvents) {
            await PreloadMagic(skillEvent.MagicId);
          }
        }
      }
    }

    private async UniTask<BehaviorGraph> PreloadBehavior(string behaviorId) {
      var behavior = await BehaviorManager.Preload(behaviorId);
      if (behavior) {
        foreach (var behaviorNode in behavior.nodes) {
          switch (behaviorNode) {
            case DoMagic doMagic:
              await PreloadMagic(doMagic.MagicId);
              break;
          }
        }
      }
      return behavior;
    }

    private async UniTask<MagicFuncBase> PreloadMagic(string magicId) {
      var magicFunc = await MagicManager.Preload(magicId);
      if (magicFunc) {
        switch (magicFunc) {
          case AddBehavior addBehavior:
            await PreloadBehavior(addBehavior.BehaviorId);
            break;
        }
      }
      return magicFunc;
    }

    public void Settle(bool isWin) {
      Debug.Log(isWin ? "Battle win" : "Battle lose");
      BattleState = BattleState.Exit;
    }

    private async UniTask Clear() {
      Instance = null;
      BattleData = null;
      BattleState = BattleState.None;

      UnitManager.Release();
      UnitManager = null;

      BuffManager.Release();
      BuffManager = null;

      MagicManager.Release();
      MagicManager = null;

      BehaviorManager.Release();
      BehaviorManager = null;

      AttribManager.Release();
      AttribManager = null;

      CardManager.Release();
      CardManager = null;

      LevelManager.Release();
      LevelManager = null;

      SkillManager.Release();
      SkillManager = null;

      PlayerManager = null;
      DamageManager = null;

      Blackboard = null;
      ObjectPool = null;
      CurPlayer = null;
      SelfPlayer = null;

      await UniTask.Yield();

      GC.Collect();
      Debug.Log("清理战斗");
    }

    private async UniTask Run() {
      // 更新当前回合的玩家
      CurPlayer = PlayerManager.MoveNext();
      Debug.Log($"当前玩家 id:{CurPlayer.RuntimeId} name:{CurPlayer.PlayerId}");
      // 刷新角色能量
      CurPlayer.RefreshEnergy();
      Debug.Log($"回合开始前刷新能量 energy:{CurPlayer.Master.GetAttrib(AttribType.ENERGY).Value}/{CurPlayer.Master.GetAttrib(AttribType.ENERGY).MaxValue}");

      // 先结算buff
      BuffManager.Update(BattleTurnPhase.ON_BEFORE_TURN);
      // 执行回合开始前的行为树
      await BehaviorManager.Run(BehaviorTime.ON_BEFORE_TURN);

      // 先结算buff
      BuffManager.Update(BattleTurnPhase.ON_TURN);
      // 回合中的逻辑
      await CurPlayer.OnTurn();

      // 先结算buff
      BuffManager.Update(BattleTurnPhase.ON_LATE_TURN);
      // 执行回合结束后的行为树
      await BehaviorManager.Run(BehaviorTime.ON_LATE_TURN);
    }
  }
}