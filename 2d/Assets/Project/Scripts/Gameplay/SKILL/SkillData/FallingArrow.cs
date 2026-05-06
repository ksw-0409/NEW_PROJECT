using UnityEngine;

public class FallingArrow : MonoBehaviour
{
    private ArrowProjectile projectile; // 기존 패시브 로직 스크립트 참조
    private Vector2 targetPos;
    private float speed = 25f;

    public void Initialize(float dmg, bool exec, bool conc, Vector2 target, Vector2 rainCenter, float rainRadius)
    {
        projectile = GetComponent<ArrowProjectile>();
        targetPos = target;

        // 1. 기존 스크립트의 기본 셋업 호출 (데미지 전달)
        // 방향은 아래쪽(Vector2.down)으로 설정
        projectile.Setup(dmg, conc ? 2.0f : 1.0f, speed, Vector2.down);
        projectile.SetAllowedArea(rainCenter, rainRadius);

        // 2. 패시브 효과 강제 적용 (원한다면)
        // BowSkill에서 하던 것처럼 여기서도 패시브 데이터에 따라 AddPassive 호출 가능
        ApplyRainPassives(exec, conc);
    }

    void Update()
    {
        // 목표 지점(바닥)까지 거의 다 왔을 때 삭제 (바닥에 박히는 느낌)
        if (Vector2.Distance(transform.position, targetPos) < 0.2f)
        {
            Destroy(gameObject);
        }
    }

    private void ApplyRainPassives(bool exec, bool conc)
    {
        // PlayerSkillController에 연결된 패시브 에셋들을 가져와서 
        // 확률적으로 혹은 확정적으로 화살에 패시브를 주입합니다.
        var controller = PlayerSkillController.Instance;

        // 예: 얼음 패시브가 있다면 30% 확률로 화살비의 화살에도 적용
        if (controller.iceCardAsset != null && controller.iceCardAsset.skillInstance != null)
        {
            var ld = controller.iceCardAsset.GetCurrentLevelData();
            projectile.AddPassive(ld, "Ice", Color.cyan);
        }

        // 집중 사격(conc) 상태라면 관통 패시브를 강제로 줄 수도 있습니다.
        if (conc && controller.pierceCardAsset != null)
        {
            projectile.SetPierce(controller.pierceCardAsset.GetCurrentLevelData());
        }
    }
}