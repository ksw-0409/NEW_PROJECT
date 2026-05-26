using System.Collections.Generic;
using UnityEngine;

public class SimpleInfiniteMap : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform; // 플레이어 위치

    [Header("9 Random Floor Sprites")]
    [SerializeField] private Sprite[] floorSprites1; // 인스펙터에서 9개 스프라이트 등록
    [SerializeField] private Sprite[] floorSprites2; // 인스펙터에서 9개 스프라이트 등록

    [Header("Settings")]
    [SerializeField] private Vector2 chunkSize = new Vector2(40f, 40f); // 스프라이트(바닥) 크기에 맞춤

    private Vector2Int currentCenterChunk = new Vector2Int(-999, -999);
    private Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();

    private Sprite[] floorSprites; // 그층에 맞는걸로 

    public void FloorStart(int floor)
    {
        if (floor == 1 || floor == 2 || floor == 3 || floor == 4) floorSprites = floorSprites1;
        else if (floor == 6 || floor == 7 || floor == 8 || floor == 9) floorSprites = floorSprites2;
        else Debug.Log("잘못된층");
    }
    void Update()
    {
        if (playerTransform == null || floorSprites.Length == 0) return;

        // 플레이어 중심 청크 계산
        Vector2Int playerChunk = new Vector2Int(
            Mathf.FloorToInt(playerTransform.position.x / chunkSize.x),
            Mathf.FloorToInt(playerTransform.position.y / chunkSize.y)
        );

        // 새로운 청크 영역 진입 시 업데이트
        if (playerChunk != currentCenterChunk)
        {
            UpdateGrid(playerChunk);
        }
    }

    private void UpdateGrid(Vector2Int newCenter)
    {
        currentCenterChunk = newCenter;

        // 3. 유지해야 할 3x3 영역 정의
        HashSet<Vector2Int> requiredChunks = new HashSet<Vector2Int>();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                requiredChunks.Add(newCenter + new Vector2Int(x, y));
            }
        }

        // 4. 멀어진 청크 파괴 및 삭제
        List<Vector2Int> chunksToRemove = new List<Vector2Int>();
        foreach (var coord in activeChunks.Keys)
        {
            if (!requiredChunks.Contains(coord))
            {
                chunksToRemove.Add(coord);
            }
        }

        foreach (var coord in chunksToRemove)
        {
            Destroy(activeChunks[coord]);
            activeChunks.Remove(coord);
        }

        // 5. 비어있는 공간에 새로운 랜덤 바닥 스폰
        foreach (var coord in requiredChunks)
        {
            if (!activeChunks.ContainsKey(coord))
            {
                CreateSimpleChunk(coord);
            }
        }
    }

    private void CreateSimpleChunk(Vector2Int coord)
    {
        // 빈 게임오브젝트 동적 생성
        GameObject chunkObj = new GameObject($"Floor_{coord.x}_{coord.y}");

        // 부모를 이 스크립트 오브젝트로 지정하여 하이Hierarchy 정리
        chunkObj.transform.SetParent(this.transform);

        // 월드 좌표 설정
        Vector3 worldPosition = new Vector3(coord.x * chunkSize.x, coord.y * chunkSize.y, 0f);
        chunkObj.transform.position = worldPosition;

        // SpriteRenderer 컴포넌트 추가 후 9개 중 하나 랜덤 배정
        SpriteRenderer spriteRenderer = chunkObj.AddComponent<SpriteRenderer>();
        int randomIndex = Random.Range(0, floorSprites.Length);
        spriteRenderer.sprite = floorSprites[randomIndex];

        // 팁: 2D 탑다운 게임 레이어 꼬임 방지를 위한 Order 설정 (보통 바닥은 -100 등 낮게 설정)
        spriteRenderer.sortingOrder = -10;

        // 추적을 위해 저장
        activeChunks.Add(coord, chunkObj);
    }
}