using UnityEngine;
using TMPro;

// 역할: 현재 보유 골드를 TMP에 표시

public class GoldUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;

    void OnEnable()
    {
        GameDataManager.OnGoldChanged += RefreshGold;

        if (GameDataManager.Instance != null)
            RefreshGold(GameDataManager.Instance.Gold);
    }

    void OnDisable()
    {
        GameDataManager.OnGoldChanged -= RefreshGold;
    }

    private void RefreshGold(int gold)
    {
        if (goldText != null)
            goldText.text = gold.ToString();
    }
}