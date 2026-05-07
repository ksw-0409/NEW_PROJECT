using UnityEngine;

public class SwordWaveSkill : SkillBase
{
    private SkillLevelData levelData;
    public GameObject effectPrefab;

    private Vector2 lastFireDir; // 기즈모 표시용

    protected override void Execute(Transform player)
    {
        levelData = instance.GetCurrentLevelData();

        // ⭐ [방향 결정 로직 수정]
        // 캐릭터의 localScale.x가 0보다 크면 오른쪽(1), 작으면 왼쪽(-1)을 바라본다고 가정합니다.
        float lookDirection = player.localScale.x > 0 ? 1f : -1f;

        // 최종 발사 방향 (오른쪽 벡터에 바라보는 방향을 곱함)
        Vector2 fireDir = Vector2.right * lookDirection;

        lastFireDir = fireDir;

        for (int i = 0; i < GetCount(); i++)
        {
            Fire(player.position, fireDir);
        }
    }

    void Fire(Vector2 startPos, Vector2 dir)
    {
        float range = levelData.range;
        // ⭐ 선의 두께 (이 값을 조절하여 판정을 더 널껏하게 만듭니다)
        float thickness = 1.5f;

        // 1. 이펙트 생성
        SpawnEffect(startPos, dir, range, thickness);

        // 2. ⭐ BoxCast를 사용하여 "두께가 있는 직선" 판정 수행
        // 파라미터: 시작점, 박스크기(두께), 회전각도, 방향, 거리
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            startPos,
            new Vector2(0.1f, thickness),
            angle,
            dir,
            range
        );

        // ⭐ 피격범위 가시화: 검기는 직선형 박스 판정
        var indicator = new GameObject("SwordWaveRangeBox");
        Vector3 center = (Vector3)startPos + (Vector3)(dir * range * 0.5f);
        indicator.transform.position = center;
        indicator.transform.rotation = Quaternion.Euler(0, 0, angle);
        var ind = indicator.AddComponent<SkillRangeIndicator>();
        ind.shape = SkillRangeIndicator.Shape.Box;
        ind.radius = range; // Box mode에서 radius = 가로
        ind.boxHeight = thickness;
        ind.edgeColor = new Color(0.85f, 0.6f, 1f, 0.95f);
        ind.fillColor = new Color(0.85f, 0.6f, 1f, 0.18f);
        ind.useWorldSpace = false; // 박스는 로컬 좌표 기준으로 회전 적용
        ind.holdDuration = 0.2f;
        ind.fadeOutDuration = 0.3f;

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                var enemy = hit.collider.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    float totalDmg = levelData.damage * levelData.multiplier;
                    enemy.TakeDamage(totalDmg);
                }
            }
        }
    }

    void SpawnEffect(Vector2 startPos, Vector2 dir, float range, float thickness)
    {
        if (effectPrefab == null) return;

        GameObject fx = Instantiate(effectPrefab);

        // 위치: 발사 지점에서 사거리의 절반만큼 앞쪽 (BoxCast의 중심과 일치)
        fx.transform.position = startPos + (dir * range * 0.5f);

        // 회전: 발사 방향에 맞춤
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        fx.transform.rotation = Quaternion.Euler(0, 0, angle);

        // ⭐ 새 스프라이트 native 크기(0.95 x 0.48)을 BoxCast 영역(range x thickness)에 정확히 맞춤
        // BoxCast: new Vector2(0.1f, thickness) + dir 방향 거리=range → 가로=range, 세로=thickness
        float visualX = (range / 0.95f) * (dir.x < 0 ? -1f : 1f);
        float visualY = thickness / 0.48f;
        fx.transform.localScale = new Vector3(visualX, visualY, 1f);

        Destroy(fx, 0.4f);
    }

    // ⭐ 검은색 기즈모로 박스 판정 범위 시각화
    // ⭐ 기즈모: 실제 BoxCast 영역(range 길이, thickness 두께)과 완벽 일치
    void OnDrawGizmos()
    {
        if (instance == null) return;

        var data = instance.GetCurrentLevelData();
        const float thickness = 1.5f; // SwordWaveSkill.Fire()의 thickness와 동일
        Gizmos.color = new Color(0.85f, 0.6f, 1f, 0.9f);

        // 발사 방향: 캐스터 localScale.x 기준 (스킬 로직과 동일)
        float lookDir = transform.localScale.x >= 0 ? 1f : -1f;
        Vector3 dir = Vector3.right * lookDir;
        Vector3 center = transform.position + (dir * data.range * 0.5f);

        Matrix4x4 rotationMatrix = Matrix4x4.TRS(center, Quaternion.LookRotation(Vector3.forward, dir) * Quaternion.Euler(0, 0, 90), Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(data.range, thickness, 0.1f));
        Gizmos.matrix = Matrix4x4.identity;
    }
}