using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

public class Pachinko_Real : MonoBehaviour
{
    //벨류당 멈출 위치 저장 
    Dictionary<int, float> positionMap = new Dictionary<int, float>()
    {
    { 0, 0f },
    { 1, -1f },
    { 2, -2f },
    { 3, -3f },
    { -3, -4f },
    { -2, -5f },
    { -1, -6f }
    };

    private bool spinning = false;
    [Header("Reel Settings")]
    private float spinSpeed = 20f;        // 회전 속도
    private float itemHeight = 1f;       // 숫자 한 칸의 높이 
    private int totalItems=7;         // 숫자의 총 개수 

    [Header("Stop Animation")]
    public float stopDuration = 1f;   // 멈추는 데 걸리는 시간
    private float reelHeight;

    private void Start()
    {
        // 전체 릴의 높이 계산 (루프 기준점)
        reelHeight = itemHeight * totalItems;
    }
    private void Update()
    {
        if (spinning)
        {
            MoveDistance(spinSpeed);
        }
    }


    //반복처리
    private void MoveDistance(float dist)
    {
        transform.Translate(Vector3.down* dist * Time.deltaTime);
        if (transform.localPosition.y <= -reelHeight + 0.1f)
        {
            transform.localPosition += new Vector3(0, reelHeight, 0);
        }
    }

    // 회전 시작을 위한 함수 
    public void StartSpin()=>spinning = true;
    //밖에서 멈추게 호출
    public void RequestStop(int value, float stap)
    {
        if (!spinning) return; // 이미 멈추는 중이면 중복 실행 방지

        if (positionMap.ContainsKey(value))
        {
            // 코루틴 시작 (직접 정지 로직 제어)
            StartCoroutine(StopRoutine(value, stap));
        }
        else
        {
            Debug.LogError($"{value}에 해당하는 좌표 데이터가 Map에 없습니다!");
        }

    }
    //멈추는 로직 
    private IEnumerator StopRoutine(int targetValue, float delay)
    {
        // 입력받은 초(stap)만큼 기다림
        if (delay > 0f)
        {
            yield return new WaitForSeconds(delay);
        }

        // Update의 이동을 멈추고 코루틴이 직접 제어 시작
        spinning = false;

        float elapsed = 0f;
        float currentSpeed = spinSpeed;
        float targetY = positionMap[targetValue];

        // 감속 구간 
        while (elapsed < stopDuration)
        {
            elapsed += Time.deltaTime;

            // 속도를 조금 줄임
            float t = elapsed / stopDuration;
            currentSpeed = Mathf.Lerp(spinSpeed, 0, t);
            MoveDistance(currentSpeed);

            yield return null;
        }

        Vector3 startPos = transform.localPosition;

        // 마지막에 '탁' 하고 자석처럼 붙는 효과 (0.3초)
        float snapElapsed = 0f;
        while (snapElapsed < 0.3f)
        {
            snapElapsed += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(startPos, new Vector3(startPos.x,targetY,startPos.z), snapElapsed / 0.3f);
            yield return null;
        }
        transform.localPosition = new Vector3(startPos.x, targetY, startPos.z);
    }
   
}
