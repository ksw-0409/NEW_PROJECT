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
        // ⭐ 선의 두께 (이 값을 조절하여 판정을 더 널찍하게 만듭니다)
        float thickness = 1.5f;

        // 1. 이펙트 생성
        SpawnEffect(startPos, dir, range, thickness);

        // 2. ⭐ BoxCast를 사용하여 "두께가 있는 직선" 판정 수행
        // 파라미터: 시작점, 박스크기(두께), 회전각도, 방향, 거리
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            startPos,
            new Vector2(0.1f, thickness), // 아주 얇은 박스지만 가로 두께(thickness)를 가짐
            angle,
            dir,
            range
        );

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                var enemy = hit.collider.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    // 최종 데미지 = 기본 데미지 * 배율
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

        // 위치: 발사 지점에서 사거리의 절반만큼 앞쪽
        fx.transform.position = startPos + (dir * range * 0.5f);

        // 회전: 발사 방향에 맞춤
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        fx.transform.rotation = Quaternion.Euler(0, 0, angle);

        // ⭐ 크기 조절: 
        // 이펙트 이미지가 한쪽 방향으로만 되어 있다면, 
        // 방향(dir.x)에 따라 이미지의 X축도 반전시켜야 할 수 있습니다.
        float flipX = dir.x < 0 ? -1f : 1f;
        fx.transform.localScale = new Vector3(range * flipX, thickness, 1f);

        Destroy(fx, 0.3f);
    }

    // ⭐ 검은색 기즈모로 박스 판정 범위 시각화
    void OnDrawGizmos()
    {
        if (instance == null) return;

        var data = instance.GetCurrentLevelData();
        Gizmos.color = Color.black;

        Vector3 dir = lastFireDir == Vector2.zero ? transform.right : (Vector3)lastFireDir;
        Vector3 center = transform.position + (dir * data.range * 0.5f);

        // 박스 형태로 공격 범위 표시
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(center, Quaternion.LookRotation(Vector3.forward, dir) * Quaternion.Euler(0, 0, 90), Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(data.range, 1.5f, 0.1f));
    }
}