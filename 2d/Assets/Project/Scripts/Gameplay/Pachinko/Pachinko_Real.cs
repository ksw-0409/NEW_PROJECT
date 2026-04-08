using System.Collections;
using System.Xml;
using UnityEngine;

public class Pachinko_Real : MonoBehaviour
{
    public enum State { Idle, Spinning, Stopping }
    public State CurrentState { get; private set; } = State.Idle;

    [Header("Reel Settings")]
    public float spinSpeed = 30f;        // 회전 속도
    public float itemHeight = 2f;       // 숫자 한 칸의 높이 (인스펙터에서 조절)
    public int totalItems=6;         // 숫자의 총 개수 

    [Header("Stop Animation")]
    public float stopDuration = 2.5f;   // 멈추는 데 걸리는 시간
    public AnimationCurve stopCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // 감속 커브
    private float reelHeight;
    private void Start()
    {
        // 전체 릴의 높이 계산 (루프 기준점)
        reelHeight = itemHeight * totalItems;
    }
    private void Update()
    {
        if (CurrentState == State.Spinning)
        {
            // 아래로 계속 이동
            transform.Translate(Vector3.down * spinSpeed * Time.deltaTime);
            // 무한 루프: 하단 경계를 넘어가면 위로 순간이동 (Snap)
            // 기준점은 릴의 이미지 구성에 따라 조정이 필요할 수 있습니다.
            if (transform.localPosition.y <= -reelHeight)
            {
                transform.localPosition += new Vector3(0, reelHeight, 0);
            }
        }
    }
    // 회전 시작을 위한 함수 (참고용)
    public void StartSpin()
    {
        CurrentState = State.Spinning;
    }
}
