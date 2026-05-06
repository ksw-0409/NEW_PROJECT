using UnityEngine;
using System.Collections;

public abstract class SkillBase : MonoBehaviour
{
    protected SkillData data;
    protected SkillInstance instance;

    public void Init(SkillInstance instance)
    {
        this.instance = instance;
    }

    protected virtual void Start()
    {
        StartCoroutine(AutoCast());
    }
    // 추상 메서드 정의 (Transform을 받도록 유지)
    protected abstract void Execute(Transform player);

    protected IEnumerator AutoCast()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetCooldown());

            // 에러 해결: Execute 호출 시 인자를 넣어줘야 합니다.
            // SkillBase가 MonoBehaviour를 상속받으므로 'this.transform'을 넘겨주면 됩니다.
            Execute(this.transform);
        }
    }

    protected float GetDamage()
    {
        return instance.GetCurrentLevelData().damage;
    }

    protected int GetCount()
    {
        return instance.GetCurrentLevelData().count;
    }

    protected float GetCooldown()
    {
        return instance.GetCurrentLevelData().cooldown;
    }

    public int GetLevel()
    {
        return instance.level;
    }

    public bool IsMaxLevel()
    {
        return instance.IsMaxLevel();
    }

    public void LevelUp()
    {
        instance.LevelUp();
    }
}