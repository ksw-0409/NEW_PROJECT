using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Scriptable Objects/SkillData")]
public class SkillData : ScriptableObject
{
    public string skillName;
    [TextArea] public string description; //스킬 상승 능력치 설명란 
    public Sprite icon;

    // 능력치 상승치 (필요한 것만 사용가능)
    public float healthAdd;
    public float damageAdd;
    public float speedAdd;
}
