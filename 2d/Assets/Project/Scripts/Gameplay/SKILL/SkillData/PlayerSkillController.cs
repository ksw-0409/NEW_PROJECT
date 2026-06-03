using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerSkillController : MonoBehaviour
{
    [SerializeField] private GameObject slashEffectPrefab;
    [SerializeField] private GameObject RotatingSlashPrefab;
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private GameObject slamPrefab;
    [SerializeField] private GameObject shieldPrefab;
    [SerializeField] private GameObject SwordWavePrefab;
    [SerializeField] private GameObject meteorFireFieldPrefab;
    [SerializeField] private GameObject ChainLightningPrefab;
    [SerializeField] private GameObject IceRainPrefab;
    [SerializeField] private GameObject FireFielfPrefab;
    [Tooltip("메테오 떨어지기 전 사전 표시 마법진 prefab")]
    [SerializeField] private GameObject meteorWarningCirclePrefab;

    [SerializeField] private GameObject bowArrowPrefab;
    [SerializeField] private GameObject arrowRainPrefab;
    [Tooltip("폭발화살이 발동될 때 사용할 폭발 이펙트 프리팹 (보통 FireExplosionEffect)")]
    public GameObject bowExplosionEffectPrefab;


    [Header("Bow Passive Assets")]
    public ArrowPassiveData iceCardAsset;
    public ArrowPassiveData explosionCardAsset;
    public ArrowPassiveData poisonCardAsset;
    public ArrowPassiveData pierceCardAsset;

    public static PlayerSkillController Instance { get; private set; }

    [Header("\ud83c\udfae \uce58\ud2b8 (\ud14c\uc2a4\ud2b8\uc6a9)")]
    [Tooltip("\ud0a4\ubcf4\ub4dc 1\ubc88 \u2192 \ubca0\uae30 \uc2a4\ud0ac \ud68d\ub4dd")]
    public SkillData cheatSlashSkill;

    [Tooltip("Ctrl+A 치트로 한꺼번에 획득할 모든 스킬 목록")]
    public System.Collections.Generic.List<SkillData> cheatAllSkills = new System.Collections.Generic.List<SkillData>();

    [Header("References")]
    public EnemyManager enemyManager;
    public List<SkillData> equippedSkills = new();

    private Dictionary<SkillData, SkillBase> skillDict = new();

    [SerializeField] private List<SkillData> saveSkills;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        foreach (var data in equippedSkills)
        {
            AddNewSkill(data);
        }

        RestoreSavedSkills();
    }

    // 🎮 치트키 (테스트용)
    //   1번 → 베기 스킬 획득/레벨업
    //   5번 → base 씬의 5층 보스 포탈 토글
    //   0번 → base 씬의 10층 보스 포탈 토글
    //   L   → 전설 장비 7개 인벤토리에 추가
    void Update()
    {

        if (Keyboard.current == null) return;

        // === 1번: 베기 스킬 ===
        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            if (cheatSlashSkill == null)
            {
                Debug.LogWarning("[치트] cheatSlashSkill이 인스펙터에 할당되지 않음");
            }
            else if (!HasSkill(cheatSlashSkill))
            {
                AddNewSkill(cheatSlashSkill);
                Debug.Log($"<color=cyan>[치트]</color> {cheatSlashSkill.skillName} 획득!");
            }
            else if (!IsMaxLevel(cheatSlashSkill))
            {
                LevelUpSkill(cheatSlashSkill);
                Debug.Log($"<color=cyan>[치트]</color> {cheatSlashSkill.skillName} 레벨업! → Lv.{GetSkillLevel(cheatSlashSkill)}");
            }
            else
            {
                Debug.Log($"<color=cyan>[치트]</color> {cheatSlashSkill.skillName} 최대 레벨");
            }
        }

        // === 5번: 5층 포탈 토글 ===
        if (Keyboard.current.digit5Key.wasPressedThisFrame)
        {
            TogglePortal("BossPortal");
        }

        // === 0번: 10층 포탈 토글 ===
        if (Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            TogglePortal("BossPortal_Floor10");
        }

        // === L: 전설 장비 전부 인벤토리에 추가 ===
        bool lPressed = false;
        try {
            if (Keyboard.current.lKey.wasPressedThisFrame) lPressed = true;
            else if (Keyboard.current[UnityEngine.InputSystem.Key.L].wasPressedThisFrame) lPressed = true;
        } catch (System.Exception ex) {
            Debug.LogWarning("[치트] L 키 감지 예외: " + ex.Message);
        }
        if (lPressed)
        {
            Debug.Log("<color=yellow>[치트] L 입력 감지!</color>");
            GrantAllLegendaryItems();
        }

        // === Ctrl+A: 모든 액티브 스킬 획득 ===
        // === Ctrl+R: 스킬트리(패시브) 초기화 ===
        bool ctrlHeld = false;
        try { ctrlHeld = Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed; } catch { }

        if (ctrlHeld && Keyboard.current.aKey.wasPressedThisFrame)
        {
            Debug.Log("<color=yellow>[치트] Ctrl+A 입력 감지!</color>");
            GrantAllSkills();
        }
        if (ctrlHeld && Keyboard.current.rKey.wasPressedThisFrame)
        {
            Debug.Log("<color=yellow>[치트] Ctrl+R 입력 감지!</color>");
            ResetSkillTree();
        }
        // === Ctrl+E: 레벨업(경험치) 잠금 토글 ===
        if (ctrlHeld && Keyboard.current.eKey.wasPressedThisFrame)
        {
            PlayerStats.LevelLocked = !PlayerStats.LevelLocked;
            string state = PlayerStats.LevelLocked ? "잠금 ON (경험치 무시)" : "잠금 OFF (정상)";
            Debug.Log($"<color=cyan>[치트]</color> 레벨업 잠금 → {state}");
        }
    }

    /// <summary>이름으로 포탈을 찾아 active 토글</summary>
    private void TogglePortal(string portalName)
    {
        var go = GameObject.Find(portalName);
        if (go == null)
        {
            // 비활성 상태인 오브젝트는 Find로 못 찾음 → Resources 검색
            foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
            {
                if (t.gameObject.name == portalName && t.gameObject.scene.IsValid()
                    && !UnityEditor_IsPartOfPrefabAsset(t.gameObject))
                {
                    go = t.gameObject; break;
                }
            }
        }
        if (go == null)
        {
            Debug.LogWarning($"[치트] 포탈 '{portalName}'을 찾을 수 없습니다 (base 씬이 로드되지 않았을 수 있음)");
            return;
        }
        bool newState = !go.activeSelf;
        go.SetActive(newState);
        Debug.Log($"<color=cyan>[치트]</color> {portalName} → {(newState?"보이기":"숨기기")}");
    }

    // 런타임에서는 PrefabUtility를 못 쓰니 항상 false 반환 (씬 오브젝트만 다룬다는 가정)
    private bool UnityEditor_IsPartOfPrefabAsset(GameObject go) => false;

    /// <summary>전설(Legendary) 등급 장비 7개를 모두 인벤토리에 추가</summary>
    /// <summary>전설(Legendary) 등급 장비 7개를 모두 인벤토리에 추가</summary>
    /// <summary>전설(Legendary) 등급 장비 7개를 모두 인벤토리에 추가</summary>
    private void GrantAllLegendaryItems()
    {
        Debug.Log("<color=yellow>[치트 진단] GrantAllLegendaryItems 시작</color>");

        // EquipmentManager.Instance 우선, 폴백으로 Resources 검색
        EquipmentManager em = EquipmentManager.Instance;
        if (em == null || em.DatabaseCount == 0)
        {
            // DB가 로드된 인스턴스를 찾기 (DontDestroyOnLoad 전환 직후 등 안전망)
            var allEms = Resources.FindObjectsOfTypeAll<EquipmentManager>();
            foreach (var candidate in allEms)
            {
                if (candidate == null || !candidate.gameObject.scene.IsValid()) continue;
                #if UNITY_EDITOR
                if (UnityEditor.EditorUtility.IsPersistent(candidate.gameObject)) continue;
                #endif
                if (candidate.DatabaseCount > 0) { em = candidate; break; }
            }
        }

        if (em == null)
        {
            Debug.LogWarning("[치트] EquipmentManager가 어디에도 없습니다");
            return;
        }
        Debug.Log($"<color=yellow>[치트 진단]</color> EquipmentManager: {em.gameObject.name} (scene={em.gameObject.scene.name}, dbCount={em.DatabaseCount})");

        if (Inventory.Instance == null)
        {
            Debug.LogWarning("[치트] Inventory.Instance가 null입니다");
            return;
        }

        var ids = em.GetIdsByGrade(ItemGrade.Legendary);
        Debug.Log($"<color=yellow>[치트 진단]</color> 전설 ID {ids.Count}개: {string.Join(",", ids)}");
        int added = 0;
        foreach (var id in ids)
        {
            var data = em.CreateItem(id);
            if (data != null)
            {
                Inventory.Instance.AddItem(data);
                added++;
            }
            else Debug.LogWarning($"[치트] CreateItem({id}) 실패");
        }
        Debug.Log($"<color=cyan>[치트]</color> 전설 장비 {added}개를 인벤토리에 추가 (총 {Inventory.Instance.Items.Count}개)");
    }

    /// <summary>cheatAllSkills 리스트의 모든 스킬을 한 번에 획득 (이미 있으면 레벨업)</summary>
    private void GrantAllSkills()
    {
        if (cheatAllSkills == null || cheatAllSkills.Count == 0)
        {
            Debug.LogWarning("[치트] cheatAllSkills 리스트가 비어있습니다 (인스펙터에서 스킬을 추가하세요)");
            return;
        }
        int newCount = 0, upCount = 0, maxCount = 0;
        foreach (var data in cheatAllSkills)
        {
            if (data == null) continue;
            if (!HasSkill(data))
            {
                AddNewSkill(data);
                newCount++;
            }
            else if (!IsMaxLevel(data))
            {
                LevelUpSkill(data);
                upCount++;
            }
            else maxCount++;
        }
        Debug.Log($"<color=cyan>[치트]</color> 스킬 일괄 획득 → 신규 {newCount}개 / 레벨업 {upCount}개 / 이미 최대 {maxCount}개");
    }

    /// <summary>스킬트리(패시브) 초기화 — PassiveSystem.ResetAll() 호출</summary>
    /// <summary>스킬트리 + 패시브 + 효과 전체 초기화 (Ctrl+R)</summary>
    private void ResetSkillTree()
    {
        // 1) GameDataManager 데이터 비우기 (unlockedSkillNodes/passiveLevels/unlockedEffects)
        if (GameDataManager.Instance != null) GameDataManager.Instance.ResetSkillTree();

        // 2) PassiveSystem 런타임 레벨 비우기 + PassiveSkillNode UI 갱신
        if (PassiveSystem.Instance != null) PassiveSystem.Instance.ResetAll();

        // 3) ⭐ 모든 SkillNode UI 강제 갱신 (잠금 해제 상태 → 잠금 시각)
        var nodes = UnityEngine.Object.FindObjectsByType<SkillNode>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var n in nodes) { if (n != null) n.ResetUnlocked(); }
        Debug.Log($"<color=cyan>[치트]</color> 스킬트리 + 패시브 초기화 완료 — SkillNode {nodes.Length}개 갱신");
    }

    public bool HasSkill(SkillData data)
    {
        if (data == null) return false;
        if (skillDict.ContainsKey(data)) return true;
        foreach (var key in skillDict.Keys)
        {
            if (key != null && key.skillName == data.skillName)
                return true;
        }
        return false;
    }

    public bool IsMaxLevel(SkillData data)
    {
        if (data == null) return false;
        if (skillDict.ContainsKey(data)) return skillDict[data].IsMaxLevel();
        foreach (var pair in skillDict)
        {
            if (pair.Key != null && pair.Key.skillName == data.skillName)
                return pair.Value.IsMaxLevel();
        }
        return false;
    }

    public void AddNewSkill(SkillData data)
    {
        if (this == null || data == null || HasSkill(data)) return;

        SkillInstance instance = new SkillInstance(data);

        // ⭐ 패시브 카드일 경우, 에셋에 인스턴스 연결 (실시간 반영의 핵심)
        if (data is ArrowPassiveData passiveData)
        {
            passiveData.skillInstance = instance;
            Debug.Log($"<color=green>[SkillController] {data.skillName} 패시브 인스턴스 연결 완료!</color>");
        }

        SkillBase skill = CreateSkill(data, instance);

        if (skill != null)
        {
            skillDict.Add(data, skill);
        }

        if (GameDataManager.Instance != null)
            GameDataManager.Instance.SaveSkill(data.skillName);
    }

    public int GetSkillLevel(SkillData data)
    {
        if (data == null) return 0;
        if (skillDict.ContainsKey(data)) return skillDict[data].GetLevel();
        foreach (var pair in skillDict)
        {
            if (pair.Key != null && pair.Key.skillName == data.skillName)
                return pair.Value.GetLevel();
        }
        return 0;
    }

    public void LevelUpSkill(SkillData data)
    {
        if (data == null) return;
        if (skillDict.ContainsKey(data))
        {
            skillDict[data].LevelUp();
            // ⭐ 레벨 저장 (씬 전환 시 유지)
            if (GameDataManager.Instance != null)
                GameDataManager.Instance.SaveSkillLevel(data.skillName, skillDict[data].GetLevel());
            return;
        }
        foreach (var pair in skillDict)
        {
            if (pair.Key != null && pair.Key.skillName == data.skillName)
            {
                pair.Value.LevelUp();
                // ⭐ 레벨 저장
                if (GameDataManager.Instance != null)
                    GameDataManager.Instance.SaveSkillLevel(data.skillName, pair.Value.GetLevel());
                return;
            }
        }
    }

    private SkillBase CreateSkill(SkillData data, SkillInstance instance)
    {
        GameObject player = this.gameObject;

        // ⭐ 활 패시브 (독/얼음/폭발/관통 화살) — 마커 컴포넌트로 등록해 레벨 관리
        //    실제 효과는 BowSkill이 ArrowPassiveData.skillInstance를 읽어 적용한다.
        if (data is ArrowPassiveData arrowPassive)
        {
            ArrowPassiveSkill skill = player.AddComponent<ArrowPassiveSkill>();
            skill.Init(arrowPassive, instance);
            return skill;
        }

        if (data is FireballData fireball)
        {
            FireballSkill skill = player.AddComponent<FireballSkill>();
            skill.Init(fireball, instance);
            return skill;
        }

        if (data is SlashData slash)
        {
            SlashSkill skill = gameObject.AddComponent<SlashSkill>();
            skill.effectPrefab = slashEffectPrefab;
            skill.Init(instance);
            return skill;
        }

        if (data is RotatingSlashData rotatingSlash)
        {
            // ⭐ 이미 있으면 재사용 — 중복 컴포넌트로 칼날 여러개 도는 버그 방지
            RotatingSlashSkill skill = player.GetComponent<RotatingSlashSkill>();
            if (skill == null) skill = player.AddComponent<RotatingSlashSkill>();
            skill.effectPrefab = RotatingSlashPrefab;
            skill.Init(rotatingSlash, instance);
            return skill;
        }

        if (data is SlamSkillData slam)
        {
            SlamSkill skill = player.AddComponent<SlamSkill>();
            skill.effectPrefab = slamPrefab;
            skill.Init(slam, instance);
            return skill;
        }

        if (data is SwordWaveData swordWave)
        {
            SwordWaveSkill skill = player.AddComponent<SwordWaveSkill>();
            skill.effectPrefab = SwordWavePrefab;
            skill.Init(instance);
            return skill;
        }

        if (data is ShieldData shieldDat)
        {
            ShieldSkill skill = player.AddComponent<ShieldSkill>();
            skill.Init(shieldDat, instance);
            return skill;
        }

        if (data is ChainLightningData chainData)
        {
            ChainLightningSkill skill = player.AddComponent<ChainLightningSkill>();
            skill.enemyManager = this.enemyManager;
            skill.lightningEffectPrefab = ChainLightningPrefab;
            skill.Init(instance);
            return skill;
        }

        if (data is MeteorData meteorData)
        {
            MeteorSkill skill = player.AddComponent<MeteorSkill>();
            skill.enemyManager = this.enemyManager;
            skill.meteorVisualPrefab = meteorFireFieldPrefab;
            skill.fireFieldPrefab = FireFielfPrefab;
            skill.warningCirclePrefab = meteorWarningCirclePrefab;
            skill.Init(instance);
            return skill;
        }

        if (data is IceRainData iceData)
        {
            IceRainSkill skill = player.AddComponent<IceRainSkill>();
            skill.enemyManager = this.enemyManager;
            skill.iceRainAreaPrefab = IceRainPrefab;
            skill.Init(instance);
            return skill;
        }

        if (data is ArrowRainData arrowRainData)
        {
            ArrowRainSkill skill = player.AddComponent<ArrowRainSkill>();
            skill.enemyManager = this.enemyManager;
            skill.rainAreaPrefab = arrowRainPrefab;
            skill.Init(instance);
            return skill;
        }

        if (data is BowSkillData bowData)
        {
            BowSkill skill = player.AddComponent<BowSkill>();
            skill.arrowPrefab = bowArrowPrefab;
            skill.iceCard = iceCardAsset;
            skill.explosionCard = explosionCardAsset;
            skill.poisonCard = poisonCardAsset;
            skill.pierceCard = pierceCardAsset;
                skill.explosionEffectPrefab = bowExplosionEffectPrefab;

            skill.Init(instance);
            return skill;
        }

        return null;
    }

    public void ResetSkills()
    {
        foreach (var skill in skillDict.Values)
        {
            Destroy(skill);
        }
        skillDict.Clear();

        if (GameDataManager.Instance != null)
        {
            foreach (var data in equippedSkills)
                GameDataManager.Instance.RemoveSkill(data.skillName);
        }
    }

    private void RestoreSavedSkills()
    {
        if (GameDataManager.Instance == null) return;
        var savedSkills = GameDataManager.Instance.EquippedSkills;
        if (savedSkills.Count == 0) return;

        foreach (string skillName in savedSkills)
        {
            SkillData found = saveSkills.Find(s => s.skillName == skillName);
            if (found == null) continue;
            if (skillDict.ContainsKey(found)) continue;
            AddNewSkill(found);

            // ⭐ 저장된 레벨까지 복구
            int savedLevel = GameDataManager.Instance.GetSavedSkillLevel(skillName);
            if (savedLevel > 1 && skillDict.ContainsKey(found))
            {
                int safety = 0;
                while (skillDict[found].GetLevel() < savedLevel && !IsMaxLevel(found) && safety < 20)
                {
                    skillDict[found].LevelUp();
                    safety++;
                }
            }
        }
    }
}