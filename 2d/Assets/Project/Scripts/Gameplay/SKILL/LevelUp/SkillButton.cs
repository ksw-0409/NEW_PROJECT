using TMPro;
using UnityEngine;
using UnityEngine.EventSystems; 
using UnityEngine.UI;
using DG.Tweening; // DOTween 사용
public class SkillButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public Image iconImage;
    public TextMeshProUGUI levelText;

    private SkillData data;
    private LevelUpManager manager;

    public Image[] Gems;
    public Image MaxGems;
    public Image Card;

    private Vector3 originalScale;
    public float hoverScale = 1.1f; // 마우스 올렸을 때 커질 배율 

    void Start()
    {
        // 시작할 때 원래 크기를 저장해 둡니다.
        originalScale = transform.localScale;
    }

    public void Setup(SkillData data, LevelUpManager mgr)
    {
        // ⭐ null 방어 (이거 중요)
        if (data == null)
        {
            Debug.LogError("SkillButton: data가 null임");
            return;
        }

        if (mgr == null)
        {
            Debug.LogError("SkillButton: manager가 null임");
            return;
        }

        this.data = data;
        this.manager = mgr;

        // ⭐ 텍스트 null 방어
        if (nameText != null)
            nameText.text = data.skillName;
        else
            Debug.LogError("nameText 연결 안됨");

        if (descText != null)
            descText.text = data.description;
        else
            Debug.LogError("descText 연결 안됨");

        if (iconImage != null)
            iconImage.sprite = data.icon;

        if(Card!=null&&data.card!=null)
            Card.sprite= data.card;

        if (data.gem != null)
        {
            for (int i = 0; i < Gems.Length; i++)
            {
                Gems[i].sprite = data.gem;
            }
        }

        if (MaxGems!= null && data.maxGem != null)
            MaxGems.sprite = data.maxGem;

        // ⭐ SkillController 찾기
        PlayerSkillController skillController = FindObjectOfType<PlayerSkillController>();

        if (skillController == null)
        {
            Debug.LogError("PlayerSkillController 못찾음");
            return;
        }

        // ⭐ 레벨 표시
        if (levelText == null)
        {
            Debug.LogError("levelText 연결 안됨");
            return;
        }

        if (skillController.HasSkill(data))
        {
            int level = skillController.GetSkillLevel(data);

            if (skillController.IsMaxLevel(data))
            {
                levelText.text = "MAX";
                MaxGems.enabled = true;
            }
            else
            {
                levelText.text = "Lv." + level;

                MaxGems.enabled = false;

                for(int i = 0; i <= level; i++)
                {
                    Gems[i].enabled = true; 
                }
                for(int i= level; i < Gems.Length; i++)
                {
                    Gems[i].enabled=false;
                }
            }
        }
        else
        {
            levelText.text = "NEW";
            for (int i = 0; i < Gems.Length; i++)
            {
                Gems[i].enabled = false;
                MaxGems.enabled = false;
            }
        }
    }

    public void OnClick()
    {
        if (manager == null || data == null)
        {
            Debug.LogError("버튼 클릭 실패: manager 또는 data null");
            return;
        }
        // 카드가 눌리는 효과
        transform.DOScale(originalScale * 1.5f, 0.2f)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true) // 게임이 일시정지(Time.timeScale = 0)된 상태여도 작동
            .OnComplete(() =>
            {
                // 애니메이션이 완전히 끝난 후 스킬 적용 로직 실행
                manager.OnSkillSelected(data);
            });

    }

    // 마우스 커서가 카드 영역 안으로 들어왔을 때 자동 실행
    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("in");
        transform.DOScale(originalScale * hoverScale, 0.2f).SetEase(Ease.OutBack).SetUpdate(true);
    }

    // 마우스 커서가 카드 영역 밖으로 나갔을 때 자동 실행
    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("out");
        transform.DOScale(originalScale, 0.2f).SetEase(Ease.OutQuad).SetUpdate(true);
    }

}