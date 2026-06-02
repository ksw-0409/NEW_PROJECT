using UnityEngine;
public class MouseManager : MonoBehaviour
{
    // 싱글톤 패턴을 활용하면 어디서나 쉽게 커서를 바꿀 수 있습니다.
    public static MouseManager Instance;

    [Header("커서 텍스처")]
    public Texture2D normalM;
    public Texture2D normalPick;
    public Texture2D pachinkoM;
    public Texture2D pachinkoPick;

    bool isPachinko = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject); // 혹시라도 다른 씬에 또 있으면 중복이니까 삭제!
            return;
        }
        SetHardwareCursor(normalM);
    }

    public void OpenPachinko()
    {
        SetHardwareCursor(pachinkoM);
        isPachinko = true;
    }

    // 기본 커서로 복구
    public void ResetToDefault()
    {
        SetHardwareCursor(normalM); 
        isPachinko = false;
    }
    void Update()
    {
        // 1. 화면 어디든 마우스 왼쪽 버튼을 누르는 '그 순간' 딱 1번 실행
        if (Input.GetMouseButtonDown(0))
        {
            if (isPachinko) SetHardwareCursor(pachinkoPick);
            else SetHardwareCursor(normalPick);
        }

        // 2. 화면 어디든 마우스 왼쪽 버튼을 떼는 '그 순간' 딱 1번 실행
        if (Input.GetMouseButtonUp(0))
        {
            if (isPachinko) SetHardwareCursor(pachinkoM);
            else SetHardwareCursor(normalM);
        }
    }
    // 커서 변경을 안전하게 처리하는 공용 메서드
    private void SetHardwareCursor(Texture2D cursorTexture)
    {
        Vector2 hotSpot = Vector2.zero;
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
    }
}