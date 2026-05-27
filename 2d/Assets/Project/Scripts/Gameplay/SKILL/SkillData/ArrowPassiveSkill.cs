using UnityEngine;

/// <summary>
/// 활 패시브(ArrowPassiveData: 독화살/얼음화살/폭발화살/관통화살) 전용 마커 스킬.
/// 실제 효과는 BowSkill이 ArrowPassiveData.skillInstance를 읽어 적용하므로,
/// 이 컴포넌트는 "레벨 관리 + skillDict 등록"만 담당한다 (AutoCast로 아무것도 발사하지 않음).
///
/// 이게 없으면 PlayerSkillController.CreateSkill이 null을 반환해서 skillDict에 등록되지 않고,
/// 그 결과 HasSkill이 항상 false → 카드 선택 시 LevelUpSkill 대신 AddNewSkill만 반복 →
/// 활 패시브 레벨이 영원히 1로 고정되는 버그가 발생한다.
/// </summary>
public class ArrowPassiveSkill : SkillBase
{
    private ArrowPassiveData passiveData;

    public void Init(ArrowPassiveData data, SkillInstance instance)
    {
        this.data = data;
        this.passiveData = data;
        base.Init(instance);

        // 에셋의 skillInstance를 이 인스턴스와 동기화 (BowSkill이 읽는 핵심 참조)
        if (passiveData != null)
            passiveData.skillInstance = instance;
    }

    // 패시브는 자체적으로 발사/시전하지 않는다 → AutoCast 루프를 돌리지 않음
    protected override void Start()
    {
        // 의도적으로 base.Start()를 호출하지 않음 (AutoCast 비활성)
    }

    // SkillBase 추상 메서드 구현 — 아무것도 하지 않음
    protected override void Execute(Transform player) { }
}
