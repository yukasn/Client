﻿using Battle;
using System;
using XNode;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

namespace BehaviorTree.Battle {
  [Serializable]
	public struct NodePort { }

  [Serializable]
  public struct NodeParamKey {
    public SourceType Type;
    public string Key;
  }

  [Serializable]
  public struct NodeParam {
    public bool IsDict;
    [ShowIf("IsDict"), HideLabel]
    public NodeParamKey ParamKey;
    [HideIf("IsDict")]
    public float Value;
  }

  public abstract class BehaviorNode : Node {
    [HideInInspector]
    public int Index = -1;
    protected BehaviorGraph Behavior;

		protected override void Init() {
			Behavior = graph as BehaviorGraph;
		}

    public abstract UniTask<bool> Run(BattleManager battleManager, Context context);

    public void UpdateIndexInEditor() {
      var inputPort = GetInputPort("In");
      Index = inputPort?.Connection?.GetConnectionIndex(inputPort) ?? -1;
    }
  }
}