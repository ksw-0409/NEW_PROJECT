using System.Collections.Generic;
using UnityEngine;
using System.Collections;

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

    [Header("Bow Passive Assets")]
    public ArrowPassiveData iceCardAsset;
    public ArrowPassiveData explosionCardAsset;
    public ArrowPassiveData poisonCardAsset;
    public ArrowPassiveData pierceCardAsset;

    public static PlayerSkillController Instance { get; private set; }

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
            return;
        }
        foreach (var pair in skillDict)
        {
            if (pair.Key != null && pair.Key.skillName == data.skillName)
            {
                pair.Value.LevelUp();
                return;
            }
        }
    }

    private SkillBase CreateSkill(SkillData data, SkillInstance instance)
    {
        GameObject player = this.gameObject;

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
            RotatingSlashSkill skill = player.AddComponent<RotatingSlashSkill>();
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
        }
    }
}