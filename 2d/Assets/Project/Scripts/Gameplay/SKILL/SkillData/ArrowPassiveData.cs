using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ArrowPassiveData", menuName = "Skill/ArrowPassiveData")]
public class ArrowPassiveData : SkillData
{
    [Header("Visual Settings")]
    //public Color arrowColor = Color.white; // 화살 이펙트 색상

    public Sprite arrowSprite; // ⭐ 이 패시브 전용 화살 이미지
    public Color arrowColor = Color.white;

    //[HideInInspector] public SkillInstance skillInstance;

    // 현재 게임에서 이 스킬의 상태를 관리하는 인스턴스
    [HideInInspector] public SkillInstance skillInstance;

    // ⭐ BowSkill에서 호출하는 함수: 현재 레벨의 데이터를 안전하게 반환
    public SkillLevelData GetCurrentLevelData()
    {
        // 1. 카드를 획득해서 인스턴스가 생성된 경우에만 데이터를 반환합니다.
        if (skillInstance != null)
        {
            return skillInstance.GetCurrentLevelData();
        }

        // 2. ⭐ 중요: 테스트용으로 넣어두었던 levels[0] 반환 로직을 삭제하거나 주석 처리하세요.
        // if (levels != null && levels.Count > 0) return levels[0]; <-- 이 줄이 있다면 삭제!

        // 3. 인스턴스가 없으면(미습득) 아무것도 반환하지 않습니다.
        return null;
    }
    // ⭐ 레벨업에 따라 확률이 변하도록 multiplier를 확률값으로 활용
    public float CurrentProcChance
    {
        get
        {
            var ld = GetCurrentLevelData();
            // 인스펙터의 Multiplier 칸에 0.1(10%), 0.2(20%) 식으로 입력하세요.
            return ld != null ? ld.multiplier : 0f;
        }
    }
}