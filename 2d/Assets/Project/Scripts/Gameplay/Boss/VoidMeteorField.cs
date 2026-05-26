using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 10층 보스(공허의 대사제) — 메테오 낙하 + 지속 피해 장판.
/// 경고 표시(낙하 예고) → 메테오 낙하 → 보라색 장판 생성 → 장판 위 Player에게 지속 피해.
/// 별도 프리팹 없이 코드로 비주얼을 생성한다 (원형 스프라이트 사용).
/// </summary>
public class VoidMeteorField : MonoBehaviour
{
    private float damagePerTick;
    private float tickInterval = 0.5f;
    private float fieldRadius = 2.0f;
    private float fieldDuration = 4f;
    private float warningTime = 1.0f;

    private readonly HashSet<Collider2D> inside = new HashSet<Collider2D>();
    private float tickTimer = 0f;
    private bool active = false;

    private SpriteRenderer warnSr;
    private SpriteRenderer fieldSr;
    private CircleCollider2D col;

    private static Sprite _circle;
    private static Sprite GetCircle()
    {
        if (_circle != null) return _circle;
        const int S = 128;
        var tex = new Texture2D(S, S, TextureFormat.RGBA32, false);
        var px = new Color[S * S];
        Vector2 c = new Vector2(S / 2f, S / 2f);
        float r = S / 2f - 2f;
        for (int y = 0; y < S; y++)
            for (int x = 0; x < S; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c);
                float a = d <= r ? 1f : 0f;
                // 가장자리 부드럽게
                if (d > r - 6f && d <= r) a = Mathf.InverseLerp(r, r - 6f, d);
                px[y * S + x] = new Color(1f, 1f, 1f, a);
            }
        tex.SetPixels(px);
        tex.Apply();
        _circle = Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), 64f);
        _circle.name = "VoidFieldCircle";
        return _circle;
    }

    /// <summary>장판 셋업 후 자동 진행 (경고 → 낙하 → 지속피해 → 소멸)</summary>
    public void Setup(float dmgPerTick, float radius, float duration, float warning = 1.0f)
    {
        damagePerTick = dmgPerTick;
        fieldRadius = radius;
        fieldDuration = duration;
        warningTime = warning;

        // 콜라이더 (트리거)
        col = gameObject.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f; // 스프라이트 64ppu 기준 반지름 0.5 = 1유닛, 스케일로 조절
        col.enabled = false;

        // 경고 표시 (테두리 느낌의 옅은 원)
        var warnGo = new GameObject("Warning");
        warnGo.transform.SetParent(transform, false);
        warnSr = warnGo.AddComponent<SpriteRenderer>();
        warnSr.sprite = GetCircle();
        warnSr.color = new Color(0.7f, 0.3f, 1f, 0.25f);
        warnSr.sortingOrder = 50;
        warnGo.transform.localScale = Vector3.one * (fieldRadius * 2f);

        // 장판 비주얼 (처음엔 숨김)
        var fieldGo = new GameObject("Field");
        fieldGo.transform.SetParent(transform, false);
        fieldSr = fieldGo.AddComponent<SpriteRenderer>();
        fieldSr.sprite = GetCircle();
        fieldSr.color = new Color(0.55f, 0.2f, 0.9f, 0f);
        fieldSr.sortingOrder = 51;
        fieldGo.transform.localScale = Vector3.one * (fieldRadius * 2f);

        // 콜라이더 반지름을 fieldRadius에 맞춤 (scale 1 기준)
        col.radius = fieldRadius;

        StartCoroutine(Routine());
    }

    private IEnumerator Routine()
    {
        // 1) 경고 단계 — 깜빡이는 착지 예고 + 하늘에서 메테오 낙하
        //    화면 위쪽 높은 곳에서 보라 메테오가 장판 중심으로 떨어진다.
        Vector3 landPos = transform.position;
        float fallHeight = 12f; // 하늘에서 떨어지는 시작 높이
        Vector3 startPos = landPos + new Vector3(1.5f, fallHeight, 0f); // 살짝 비스듬히 낙하

        // 메테오 비주얼 생성 (독립 오브젝트 — 장판의 자식으로 두면 부모 좌표 영향으로 흔들릴 수 있음)
        var meteorGo = new GameObject("FallingMeteor");
        meteorGo.transform.position = startPos;
        float meteorScale = Mathf.Max(1.2f, fieldRadius * 1.4f);

        // 트레일(꼬리) — 회전에 휩쓸리지 않도록 본체(meteorGo) 바로 밑, 회전 안 함
        var trail = new GameObject("MeteorTrail");
        trail.transform.SetParent(meteorGo.transform, false);
        trail.transform.localPosition = new Vector3(0.1f, 0.7f, 0f); // 위쪽으로 꼬리
        var trailSr = trail.AddComponent<SpriteRenderer>();
        trailSr.sprite = GetCircle();
        trailSr.color = new Color(0.85f, 0.5f, 1f, 0.5f);
        trailSr.sortingOrder = 89;
        trail.transform.localScale = Vector3.one * (meteorScale * 0.8f);

        // 메테오 몸체 스프라이트 — 회전은 이 자식에만 적용 (트레일/위치에 영향 없음)
        var meteorBody = new GameObject("MeteorBody");
        meteorBody.transform.SetParent(meteorGo.transform, false);
        meteorBody.transform.localPosition = Vector3.zero;
        var meteorSr = meteorBody.AddComponent<SpriteRenderer>();
        meteorSr.sprite = GetCircle();
        meteorSr.color = new Color(0.7f, 0.3f, 1f, 1f);
        meteorSr.sortingOrder = 90; // 장판/경고보다 위
        meteorBody.transform.localScale = Vector3.one * meteorScale;

        float t = 0f;
        while (t < warningTime)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / warningTime);

            // 착지 예고 깜빡임
            float blink = Mathf.PingPong(t * 4f, 1f);
            if (warnSr != null) warnSr.color = new Color(0.7f, 0.3f, 1f, 0.15f + blink * 0.25f);

            // 메테오 낙하 (가속감 위해 ease-in) — 위치는 직선(startPos→landPos)만 따름
            if (meteorGo != null)
            {
                float fall = p * p; // ease-in (점점 빨라짐)
                meteorGo.transform.position = Vector3.Lerp(startPos, landPos, fall);
                // 낙하하면서 살짝 커짐 (몸체+트레일 함께)
                float s = Mathf.Lerp(0.7f, 1f, p);
                meteorGo.transform.localScale = Vector3.one * s;
                // 회전은 몸체 스프라이트에만 (트레일·위치엔 영향 없음)
                if (meteorBody != null)
                    meteorBody.transform.localRotation = Quaternion.Euler(0f, 0f, t * 360f);
            }
            yield return null;
        }

        // 메테오 착지 → 제거
        if (meteorGo != null) Destroy(meteorGo);

        // 2) 낙하 임팩트 — 화면 흔들림 + 장판 활성
        // 기존 메테오와 동일한 착지 흔들림 (Shake(magnitude, duration, frequency))
        if (CameraShake.Instance != null) CameraShake.Shake(0.7f, 0.55f, 14f);
        if (warnSr != null) warnSr.enabled = false;
        if (fieldSr != null) fieldSr.color = new Color(0.55f, 0.2f, 0.9f, 0.45f);
        if (col != null) col.enabled = true;
        active = true;

        // 3) 지속 피해 단계
        float life = 0f;
        while (life < fieldDuration)
        {
            life += Time.deltaTime;
            tickTimer += Time.deltaTime;
            if (tickTimer >= tickInterval)
            {
                tickTimer = 0f;
                DealTick();
            }
            // 마지막에 서서히 사라지기
            if (fieldSr != null)
            {
                float fade = life > fieldDuration - 0.6f ? Mathf.InverseLerp(fieldDuration, fieldDuration - 0.6f, life) : 1f;
                var cc = fieldSr.color; cc.a = 0.45f * fade; fieldSr.color = cc;
            }
            yield return null;
        }

        Destroy(gameObject);
    }

    private void DealTick()
    {
        foreach (var c in inside)
        {
            if (c == null) continue;
            if (c.CompareTag("Player"))
            {
                var pc = c.GetComponent<PlayerController>();
                if (pc == null) pc = c.GetComponentInParent<PlayerController>();
                if (pc != null) pc.TakeDamage(damagePerTick);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!active) { inside.Add(other); return; }
        inside.Add(other);
    }
    void OnTriggerStay2D(Collider2D other) { inside.Add(other); }
    void OnTriggerExit2D(Collider2D other) { inside.Remove(other); }
}
