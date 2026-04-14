using UnityEngine;
using TMPro;

// 역할: 스킬 트리 UI에서 일반/특수 재화 보유 수를 표시

public class SkillCurrencyUI : MonoBehaviour
{
    [Header("재화 표시 텍스트")]
    [SerializeField] private TextMeshProUGUI normalCurrencyText;
    [SerializeField] private TextMeshProUGUI specialCurrencyText;

    void OnEnable()
    {
        GameDataManager.OnNormalCurrencyChanged += RefreshNormal;
        GameDataManager.OnSpecialCurrencyChanged += RefreshSpecial;

        if (GameDataManager.Instance != null)
        {
            RefreshNormal(GameDataManager.Instance.NormalCurrency);
            RefreshSpecial(GameDataManager.Instance.SpecialCurrency);
        }
    }

    void OnDisable()
    {
        GameDataManager.OnNormalCurrencyChanged -= RefreshNormal;
        GameDataManager.OnSpecialCurrencyChanged -= RefreshSpecial;
    }

    private void RefreshNormal(int amount)
    {
        if (normalCurrencyText != null)
            normalCurrencyText.text = $"일반 재화: {amount}";
    }

    private void RefreshSpecial(int amount)
    {
        if (specialCurrencyText != null)
            specialCurrencyText.text = $"특수 재화: {amount}";
    }
}