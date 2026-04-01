using UnityEngine;

// ■ 역할: 테스트용 플레이어 강제 사망
//   - 에디터에서 버튼 클릭 또는 K키로 즉사
//   - 빌드에 포함되지 않도록 #if UNITY_EDITOR 처리
//
// ■ 유니티 세팅
//   - Battle 씬 아무 GameObject에나 추가
//   - 테스트 끝나면 삭제할 것
public class DebugKillPlayer : MonoBehaviour
{
    private PlayerStats playerStats;

    void Awake()
    {
        playerStats = FindFirstObjectByType<PlayerStats>();

        if (playerStats == null)
            Debug.LogError("[DebugKillPlayer] PlayerStats를 찾을 수 없습니다.");
    }

    void Update()
    {
#if UNITY_EDITOR
        // K키로 즉사
        if (Input.GetKeyDown(KeyCode.K))
        {
            KillPlayer();
        }
#endif
    }

#if UNITY_EDITOR
    // Inspector에서 버튼으로 호출 가능
    [ContextMenu("플레이어 즉사 테스트")]
    private void KillPlayer()
    {
        if (playerStats == null)
        {
            Debug.LogError("[DebugKillPlayer] PlayerStats가 없습니다.");
            return;
        }

        Debug.Log("[DebugKillPlayer] 플레이어 강제 사망 실행");
        playerStats.TakeDamage(playerStats.data.maxHealth);
    }
#endif
}