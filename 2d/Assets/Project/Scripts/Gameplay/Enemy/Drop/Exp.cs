using UnityEngine;
using UnityEngine.Pool;

public class Exp : MonoBehaviour
{
    private IObjectPool<Exp> managedPool;
    public bool IsEaten { get; private set; } = false;
    public float expAmount = 1;
    public Sprite[] expSprite;
    private SpriteRenderer spriteRenderer;
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    public void SetPool(IObjectPool<Exp> pool)
    {
        managedPool = pool;
        IsEaten = false; // 풀에서 꺼낼 때 초기화
    }

    public void SetExp(float expAmount)
    {
        this.expAmount = expAmount;
        IsEaten = false; // 풀에서 꺼낼 때 초기화
        int index = 0; //기본 소 
        // 조건에 따른 인덱스 결정
        if (expAmount >= 200) index = 3; // 특대(초록)
        else if (expAmount >= 60) index = 2; // 대(노랑)
        else if (expAmount >= 20) index = 1; // 중(하늘)
        spriteRenderer.sprite = expSprite[index];
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            IsEaten = true;
            // 플레이어 경험치 증가 로직 호출 
            other.GetComponent<PlayerStats>().TakeExp(expAmount);
            ExpManager.Instance.EnqueueToRelease(this);
        }
    }
}
