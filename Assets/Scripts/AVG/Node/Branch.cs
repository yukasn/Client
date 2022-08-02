﻿using Sirenix.OdinInspector;
using UnityEngine;

namespace GameCore.AVGFuncs {
	[CreateNodeMenu("节点/控制/分支")]
	public class Branch : AVGNode {
		[LabelText("入")]
		[Input]
		public NodePort In;
		[LabelText("选项")]
    [TextArea]
		[Output(dynamicPortList = true)]
		public string[] Options;

		public override void Run(AVG avg) {
			// TODO:初始化选项
		}
	}
}