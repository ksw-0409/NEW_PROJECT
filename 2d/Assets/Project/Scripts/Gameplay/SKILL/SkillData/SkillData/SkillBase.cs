using UnityEngine;
using System.Collections;

public abstract class SkillBase : MonoBehaviour
{
    protected SkillData data;
    protected SkillInstance instance;

    public void Init(SkillData data, SkillInstance instance)
    {
        this.data = data;
        this.instance = instance;

        //StartCoroutine(AutoCast());
    }

    protected virtual void Start()
    {
        StartCoroutine(AutoCast());
    }
    protected IEnumerator AutoCast()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetCooldown());
            Execute();
        }
    }

    protected abstract void Execute();

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