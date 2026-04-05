using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Unity.VisualScripting;

public class SuperEasyLever : MonoBehaviour
{
    private Camera mainCamera; //메인카메라 
    public GameObject draggingObject; //드레그 오브젝트 
    public GameObject shaftObject; // 레버 막대  
    private Vector2 screenPosition;    // 마우스의 화면 좌표
    private bool isClicked;            // 클릭 상태 여부
    private bool isRolling = false;            // 룰렛 돌리는 여부
    private Vector3 startPos; //오브젝트 스타트 포지션 
    private Vector3 offset=new Vector3(0,0.5f,0);

    void Awake()
    {
        mainCamera = Camera.main;
        startPos = draggingObject.transform.position;
    }

    void OnEnable() {
        isRolling = false;
        isClicked = false;
    }

    public void OnPoint(InputValue value)
    {
        screenPosition = value.Get<Vector2>();
    }

    public void OnClick(InputValue value)
    {
        if (isRolling) return;
        if (value.isPressed)
        {
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
            RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

            if (hit.collider != null)
            {
                isClicked = true;
            }
        }
        else
        {
            if (isClicked)
            {
                isClicked = false;
                StartCoroutine(ReturnToOrigin(draggingObject, startPos));
            }
        }
    }
    
    //복귀하는 로직 
    IEnumerator ReturnToOrigin(GameObject obj, Vector3 target)
    {
        float elapsed = 0f;
        float duration = 0.2f; //속도조절 
        Vector3 startPos = obj.transform.position;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            obj.transform.position = Vector3.Lerp(startPos, target, elapsed / duration);
            shaftObject.transform.position = draggingObject.transform.position - offset;
            yield return null;
        }

        obj.transform.position = target; // 마지막에 정확한 위치로 고정
    }

    void Update()
    {
        if (isClicked && !isRolling)
        {
            Vector3 targetPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, 10f));
            targetPos.z = 0;
            targetPos.x = startPos.x;
            if (targetPos.y > startPos.y) return;
            draggingObject.transform.position = targetPos;
            shaftObject.transform.position = draggingObject.transform.position - offset;
            // 임계점 도달 체크 (드래그 중에만 체크)
            if (draggingObject.transform.position.y < -0.5f)
            {
                isClicked = false;
                isRolling = true; 
                StartCoroutine(ReturnToOrigin(draggingObject, startPos));

                //여기 룰렛 돌아가는 함수 
                Debug.Log("룰렛 시작!");
            }
        }
    }
}