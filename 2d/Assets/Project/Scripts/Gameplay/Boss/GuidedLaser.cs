using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 10층 보스(공허의 대사제) — 유도 레이저.
/// 보스에서 뻗어나가는 빔. 발사 전 5초 = 3초 플레이어 추적(천천히 회전) + 2초 정지(조준 고정).
/// 그 후 발사. 플레이어는 움직여서 레이저 직선이 봉인석 기둥을 지나도록 유도해야 한다.
/// - 발사 시 직선상에 기둥(SealPillar)이 있으면 기둥 파괴 + 명중 성공.
/// - 기둥에 안 맞고 플레이어만 맞으면 데미지 (명중 실패).
/// 결과는 onResolved(bool hitPillar) 콜백으로 보스에 전달.
/// </summary>
public class GuidedLaser : MonoBehaviour
{
    private Transform origin;       // 보스 (레이저 시작점)
    private Transform target;       // 플레이어 (추적 대상)
    private float trackDuration = 3f;
    private float lockDuration = 2f;
    private float damage = 20f;
    private float beamLength = 30f;
    private float beamWidth = 0.5f;
    private float trackTurnSpeed = 120f; // 초당 회전 각도 (천천히 추적 → 플레이어가 피할 여지)
    private Action<bool> onResolved;

    private LineRenderer line;
    private float currentAngle;     // 현재 빔 방향 각도 (deg)
    private bool tracking = true;

    public void Setup(Transform origin, Transform target, float trackDur, float lockDur,
                      float damage, float beamLength, float beamWidth, Action<bool> onResolved)
    {
        this.origin = origin;
        this.target = target;
        this.trackDuration = trackDur;
        this.lockDuration = lockDur;
        this.damage = damage;
        this.beamLength = beamLength;
        this.beamWidth = beamWidth;
        this.onResolved = onResolved;

        // 초기 각도: 보스→플레이어 방향
        Vector2 dir = target != null && origin != null
            ? ((Vector2)target.position - (Vector2)origin.position).normalized
            : Vector2.down;
        currentAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        SetupLine();
        StartCoroutine(Routine());
    }

    private void SetupLine()
    {
        line = gameObject.AddComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.numCapVertices = 4;
        var sh = Shader.Find("Sprites/Default");
        if (sh != null) line.material = new Material(sh);
        line.sortingOrder = 80;
        SetWidth(beamWidth * 0.25f); // 조준 단계엔 얇게
        SetColor(new Color(0.8f, 0.4f, 1f, 0.5f));
    }

    private void SetWidth(float w) { if (line != null) { line.startWidth = w; line.endWidth = w; } }
    private void SetColor(Color c) { if (line != null) { line.startColor = c; line.endColor = c; } }

    private IEnumerator Routine()
    {
        // 1) 추적 단계 (3초) — 플레이어 향해 천천히 회전
        tracking = true;
        float t = 0f;
        while (t < trackDuration)
        {
            t += Time.deltaTime;
            TurnTowardTarget(Time.deltaTime);
            UpdateBeamVisual();
            // 조준 단계 깜빡임
            float blink = 0.35f + Mathf.PingPong(t * 3f, 0.3f);
            SetColor(new Color(0.8f, 0.4f, 1f, blink));
            yield return null;
        }

        // 2) 정지(조준 고정) 단계 (2초) — 방향 고정, 빔 점점 굵고 진하게(발사 예고)
        tracking = false;
        t = 0f;
        while (t < lockDuration)
        {
            t += Time.deltaTime;
            UpdateBeamVisual();
            float p = t / lockDuration;
            SetWidth(Mathf.Lerp(beamWidth * 0.25f, beamWidth * 0.5f, p));
            SetColor(new Color(1f, 0.5f, 1f, 0.6f + p * 0.3f));
            yield return null;
        }

        // 3) 발사 — 굵은 빔 + 히트 판정
        yield return StartCoroutine(Fire());

        Destroy(gameObject);
    }

    private void TurnTowardTarget(float dt)
    {
        if (origin == null || target == null) return;
        Vector2 toTarget = ((Vector2)target.position - (Vector2)origin.position).normalized;
        float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
        currentAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, trackTurnSpeed * dt);
    }

    private Vector2 Dir => new Vector2(Mathf.Cos(currentAngle * Mathf.Deg2Rad), Mathf.Sin(currentAngle * Mathf.Deg2Rad));

    private void UpdateBeamVisual()
    {
        if (line == null || origin == null) return;
        Vector3 start = origin.position;
        Vector3 end = start + (Vector3)(Dir * beamLength);
        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }

    private IEnumerator Fire()
    {
        SetWidth(beamWidth);
        SetColor(new Color(1f, 0.3f, 1f, 0.95f));
        UpdateBeamVisual();
        if (CameraShake.Instance != null) CameraShake.ShakePreset(CameraShake.Preset.Heavy);

        // 발사 직선상 충돌 검사 (보스 위치에서 Dir 방향 빔)
        Vector2 start = origin != null ? (Vector2)origin.position : (Vector2)transform.position;
        Vector2 dir = Dir;

        // ⭐ 한 번에 raycast — 기둥 검사
        RaycastHit2D[] hits = Physics2D.RaycastAll(start, dir, beamLength);
        bool hitPillar = false;
        bool hitPlayer = false;
        Debug.Log($"[GuidedLaser] Fire! start={start} dir={dir} length={beamLength} hits={hits.Length}");
        foreach (var h in hits)
        {
            if (h.collider == null) continue;
            Debug.Log($"  → 충돌: {h.collider.gameObject.name} tag={h.collider.tag} layer={LayerMask.LayerToName(h.collider.gameObject.layer)} isTrigger={h.collider.isTrigger}");
            var pillar = h.collider.GetComponent<SealPillar>();
            if (pillar == null) pillar = h.collider.GetComponentInParent<SealPillar>();
            if (pillar != null && !pillar.IsDestroyed)
            {
                pillar.OnHitByLaser();
                hitPillar = true;
            }
            if (h.collider.CompareTag("Player")) hitPlayer = true;
        }

        // ⭐ 빔이 보이는 0.25초 동안 매 프레임 플레이어 충돌 재체크 (지속 데미지 판정)
        float remaining = 0.25f;
        float dmgInterval = 0f;
        while (remaining > 0f)
        {
            var hits2 = Physics2D.RaycastAll(start, dir, beamLength);
            foreach (var h in hits2)
            {
                if (h.collider != null && h.collider.CompareTag("Player"))
                {
                    hitPlayer = true;
                    break;
                }
            }
            // ⭐ 빔에 닿은 플레이어는 항상 데미지 (기둥 명중 여부 무관)
            if (dmgInterval >= 0.05f && hitPlayer)
            {
                var pgo = GameObject.FindGameObjectWithTag("Player");
                if (pgo != null)
                {
                    var pc = pgo.GetComponent<PlayerController>();
                    if (pc == null) pc = pgo.GetComponentInParent<PlayerController>();
                    if (pc != null)
                    {
                        Debug.Log($"[GuidedLaser] 플레이어 피격 → 데미지 {damage}");
                        pc.TakeDamage(damage);
                        hitPlayer = false; // 한 번만 데미지
                        dmgInterval = -999f; // 더 이상 적용 X
                    }
                }
            }
            dmgInterval += Time.deltaTime;
            remaining -= Time.deltaTime;
            yield return null;
        }

        onResolved?.Invoke(hitPillar);
    }
}
