using UnityEngine;
using Sirenix.OdinInspector;

namespace Battle {
  [CreateAssetMenu(menuName = "模板/单位")]
  public class UnitTemplate : ScriptableObject {
    [ReadOnly]
    public string UnitId;
    public string Name;
    public string AttribId;
    public int MaxLevel;
  }
}