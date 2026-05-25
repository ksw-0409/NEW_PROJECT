using UnityEngine;

/// <summary>
/// 레벨업 카드로 등장하는 패시브 카드 데이터.
/// 스킬(SkillData)과 달리 PassiveSystem의 레벨을 1 올리는 역할만 한다.
/// 카드를 고르면 PassiveSystem.Instance.LevelUp(passiveID) 가 호출되어
/// PassiveSystem 내부의 levels[passiveID] 값이 +1 저장된다.
///
/// passiveID 는 PassiveSystem 의 상수와 일치해야 한다:
///   passive_hp / passive_defense / passive_phys_dmg / passive_mag_dmg /
///   passive_speed / passive_exp / passive_gold / passive_crit
/// (화살 관련 패시브는 카드 풀에 넣지 않는다)
/// </summary>
[CreateAssetMenu(fileName = "PassiveCardData", menuName = "Skill/PassiveCardData")]
public class PassiveCardData : ScriptableObject
{
    [Header("패시브 식별자 (PassiveSystem 상수와 일치)")]
    public string passiveID = "passive_gold";

    [Header("카드 표시 정보")]
    public string cardName;          // 카드 이름 (예: "골드 획득량 증가")
    [TextArea] public string description;  // 설명
    public Sprite icon;              // 아이콘
    public Sprite card;              // 카드 배경
    public Sprite gem;               // 레벨 표시용 보석
    public Sprite maxGem;            // 최대 레벨 보석

    /// <summary>이 패시브의 현재 레벨 (0~6). PassiveSystem에서 조회.</summary>
    public int CurrentLevel
    {
        get
        {
            if (PassiveSystem.Instance == null) return 0;
            return PassiveSystem.Instance.GetLevel(passiveID);
        }
    }

    /// <summary>최대 레벨(6) 도달 여부.</summary>
    public bool IsMaxLevel => CurrentLevel >= 6;

    /// <summary>카드 선택 시 호출 — 패시브 레벨 +1.</summary>
    public bool Apply()
    {
        if (PassiveSystem.Instance == null)
        {
            Debug.LogError("[PassiveCardData] PassiveSystem.Instance 가 없습니다.");
            return false;
        }
        return PassiveSystem.Instance.LevelUp(passiveID);
    }
}
