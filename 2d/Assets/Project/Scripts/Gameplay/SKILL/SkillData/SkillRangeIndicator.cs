using UnityEngine;

[DisallowMultipleComponent]
public class SkillRangeIndicator : MonoBehaviour
{
    public enum Shape { Circle, Sector, Box }

    public Shape shape = Shape.Circle;
    public float radius = 1f;
    public float sectorAngle = 90f;
    public float boxHeight = 1f;

    public Color edgeColor = new Color(1f, 0.85f, 0.2f, 0.95f);
    public Color fillColor = new Color(1f, 0.85f, 0.2f, 0.18f);
    public float lineWidth = 0.08f;
    public bool useWorldSpace = true;
    public int sortingOrder = 80;

    public float fadeInDuration = 0.05f;
    public float holdDuration = 0.25f;
    public float fadeOutDuration = 0.25f;
    public bool autoDestroy = true;

    private LineRenderer edgeRenderer;
    private LineRenderer fillRenderer;
    private const int CircleSegments = 64;
    private float lifeTimer;

    void Awake() { BuildRenderers(); }

    void OnEnable() { lifeTimer = 0f; UpdateLines(); }

    void Update()
    {
        UpdateLines();
        if (!autoDestroy) return;

        lifeTimer += Time.deltaTime;
        float total = fadeInDuration + holdDuration + fadeOutDuration;
        float alpha = 1f;
        if (lifeTimer < fadeInDuration) alpha = Mathf.Clamp01(lifeTimer / Mathf.Max(0.001f, fadeInDuration));
        else if (lifeTimer < fadeInDuration + holdDuration) alpha = 1f;
        else if (lifeTimer < total)
        {
            float t = (lifeTimer - fadeInDuration - holdDuration) / Mathf.Max(0.001f, fadeOutDuration);
            alpha = 1f - Mathf.Clamp01(t);
        }
        else { Destroy(gameObject); return; }

        ApplyAlpha(alpha);
    }

    void BuildRenderers()
    {
        var edgeGo = new GameObject("RangeEdge");
        edgeGo.transform.SetParent(transform, false);
        edgeRenderer = edgeGo.AddComponent<LineRenderer>();
        edgeRenderer.useWorldSpace = useWorldSpace;
        edgeRenderer.loop = true;
        edgeRenderer.material = new Material(Shader.Find("Sprites/Default"));
        edgeRenderer.startWidth = lineWidth;
        edgeRenderer.endWidth = lineWidth;
        edgeRenderer.startColor = edgeColor;
        edgeRenderer.endColor = edgeColor;
        edgeRenderer.sortingOrder = sortingOrder + 1;

        var fillGo = new GameObject("RangeFill");
        fillGo.transform.SetParent(transform, false);
        fillRenderer = fillGo.AddComponent<LineRenderer>();
        fillRenderer.useWorldSpace = useWorldSpace;
        fillRenderer.loop = false;
        fillRenderer.material = new Material(Shader.Find("Sprites/Default"));
        fillRenderer.startWidth = lineWidth * 0.5f;
        fillRenderer.endWidth = lineWidth * 0.5f;
        fillRenderer.startColor = fillColor;
        fillRenderer.endColor = fillColor;
        fillRenderer.sortingOrder = sortingOrder;
    }

    void UpdateLines()
    {
        if (edgeRenderer == null || fillRenderer == null) return;
        Vector3 center = transform.position;

        if (shape == Shape.Circle)
        {
            edgeRenderer.positionCount = CircleSegments;
            edgeRenderer.loop = true;
            for (int i = 0; i < CircleSegments; i++)
            {
                float a = (float)i / CircleSegments * Mathf.PI * 2f;
                edgeRenderer.SetPosition(i, center + new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f));
            }
            fillRenderer.positionCount = CircleSegments * 2;
            fillRenderer.loop = false;
            for (int i = 0; i < CircleSegments; i++)
            {
                float a = (float)i / CircleSegments * Mathf.PI * 2f;
                Vector3 outer = center + new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, 0f);
                fillRenderer.SetPosition(i * 2, center);
                fillRenderer.SetPosition(i * 2 + 1, outer);
            }
        }
        else if (shape == Shape.Sector)
        {
            int seg = Mathf.Max(8, Mathf.RoundToInt(sectorAngle / 5f));
            edgeRenderer.positionCount = seg + 2;
            edgeRenderer.loop = true;
            edgeRenderer.SetPosition(0, center);
            Vector3 forward = transform.right;
            for (int i = 0; i <= seg; i++)
            {
                float a = -sectorAngle * 0.5f + sectorAngle * (i / (float)seg);
                Vector3 d = Quaternion.Euler(0, 0, a) * forward;
                edgeRenderer.SetPosition(i + 1, center + d * radius);
            }
            fillRenderer.positionCount = (seg + 1) * 2;
            fillRenderer.loop = false;
            for (int i = 0; i <= seg; i++)
            {
                float a = -sectorAngle * 0.5f + sectorAngle * (i / (float)seg);
                Vector3 d = Quaternion.Euler(0, 0, a) * forward;
                fillRenderer.SetPosition(i * 2, center);
                fillRenderer.SetPosition(i * 2 + 1, center + d * radius);
            }
        }
        else
        {
            edgeRenderer.positionCount = 4;
            edgeRenderer.loop = true;
            Vector3 right = transform.right * radius * 0.5f;
            Vector3 up = transform.up * boxHeight * 0.5f;
            edgeRenderer.SetPosition(0, center - right - up);
            edgeRenderer.SetPosition(1, center + right - up);
            edgeRenderer.SetPosition(2, center + right + up);
            edgeRenderer.SetPosition(3, center - right + up);
            fillRenderer.positionCount = 0;
        }
    }

    void ApplyAlpha(float a)
    {
        if (edgeRenderer != null)
        {
            var c = edgeColor; c.a = edgeColor.a * a;
            edgeRenderer.startColor = c; edgeRenderer.endColor = c;
        }
        if (fillRenderer != null)
        {
            var c = fillColor; c.a = fillColor.a * a;
            fillRenderer.startColor = c; fillRenderer.endColor = c;
        }
    }

    public static SkillRangeIndicator Spawn(Vector3 worldPos, float radius, Color edge, float duration = 0.6f, Shape shape = Shape.Circle)
    {
        var go = new GameObject("SkillRangeIndicator");
        go.transform.position = worldPos;
        var ind = go.AddComponent<SkillRangeIndicator>();
        ind.shape = shape;
        ind.radius = radius;
        ind.edgeColor = edge;
        ind.fillColor = new Color(edge.r, edge.g, edge.b, edge.a * 0.2f);
        ind.holdDuration = Mathf.Max(0.05f, duration - 0.3f);
        ind.fadeOutDuration = 0.3f;
        return ind;
    }

    public static SkillRangeIndicator SpawnSector(Vector3 worldPos, Vector2 dir, float radius, float angleDeg, Color edge, float duration = 0.6f)
    {
        var go = new GameObject("SkillRangeIndicator");
        go.transform.position = worldPos;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        go.transform.rotation = Quaternion.Euler(0, 0, ang);
        var ind = go.AddComponent<SkillRangeIndicator>();
        ind.shape = Shape.Sector;
        ind.radius = radius;
        ind.sectorAngle = angleDeg;
        ind.edgeColor = edge;
        ind.fillColor = new Color(edge.r, edge.g, edge.b, edge.a * 0.2f);
        ind.holdDuration = Mathf.Max(0.05f, duration - 0.3f);
        ind.fadeOutDuration = 0.3f;
        return ind;
    }
}
