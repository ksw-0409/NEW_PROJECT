using UnityEngine;

public class ShieldSkill : SkillBase
{
    private ShieldData shieldData;

    private bool hasShield = false;
    private GameObject currentEffect;

    public void Init(ShieldData data, SkillInstance instance)
    {
        base.Init(data, instance);
        shieldData = data;

        StartCoroutine(AutoCast());
    }

    protected override void Execute()
    {
        hasShield = true;

        // ⭐ 이미 있으면 생성 안함
        if (currentEffect != null) return;

        if (shieldData.effectPrefab != null)
        {
            currentEffect = Instantiate(shieldData.effectPrefab, transform);
            currentEffect.transform.localPosition = Vector3.zero;
        }
    }

    public bool UseShield()
    {
        if (!hasShield) return false;

        hasShield = false;

        // ⭐ 이펙트 제거
        if (currentEffect != null)
        {
            Destroy(currentEffect);
            currentEffect = null;
        }

        return true;
    }
}