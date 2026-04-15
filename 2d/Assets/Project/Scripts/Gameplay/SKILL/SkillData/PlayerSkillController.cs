using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using static UnityEngine.Rendering.DebugUI.Table;
using UnityEngine.UIElements;


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


    [Header("References")]
    public EnemyManager enemyManager; // EnemyManager의 activeEnemies를 활용하기 위함
    public List<SkillData> equippedSkills = new();

    private Dictionary<SkillData, SkillBase> skillDict = new();

    [SerializeField] private List<SkillData> saveSkills;

    private bool isInvincible = false;


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
        return skillDict.ContainsKey(data);
    }

    public bool IsMaxLevel(SkillData data)
    {
        if (!skillDict.ContainsKey(data)) return false;

        return skillDict[data].IsMaxLevel();
    }

    public void AddNewSkill(SkillData data)
    {
        if (data == null || skillDict.ContainsKey(data)) return;

        SkillInstance instance = new SkillInstance(data);

        SkillBase skill = CreateSkill(data, instance); // ⭐ instance 같이 넘김

        if (skill != null)
        {
            skillDict.Add(data, skill);
        }
        if (GameDataManager.Instance != null)
            GameDataManager.Instance.SaveSkill(data.skillName);
    }
    public int GetSkillLevel(SkillData data)
    {
        if (!skillDict.ContainsKey(data)) return 0;

        return skillDict[data].GetLevel();
    }

    public void LevelUpSkill(SkillData data)
    {
        if (!skillDict.ContainsKey(data)) return;

        skillDict[data].LevelUp();
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
            SlashSkill skill = gameObject.AddComponent<SlashSkill>(); // ⭐ 핵심

            skill.effectPrefab = slashEffectPrefab; // ⭐ 이거 반드시

            skill.Init(slash, instance);

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
            skill.Init(swordWave, instance);
            return skill;
        }
        if (data is ShieldData shield)
        {
            ShieldSkill skill = player.AddComponent<ShieldSkill>();
            skill.Init(shield, instance);
            return skill;
        }
        // 1. 연쇄 번개
        if (data is ChainLightningData chainData)
        {
            ChainLightningSkill skill = player.AddComponent<ChainLightningSkill>();
            skill.enemyManager = this.enemyManager; // 적 탐색을 위해 매니저 전달
            skill.lightningEffectPrefab = ChainLightningPrefab;
            skill.Init(chainData, instance);
            return skill;
        }

        // 2. 메테오
        if (data is MeteorData meteorData)
        {
            MeteorSkill skill = player.AddComponent<MeteorSkill>();
            skill.enemyManager = this.enemyManager;
            skill.meteorVisualPrefab = meteorFireFieldPrefab;
            skill.fireFieldPrefab = FireFielfPrefab;
            skill.Init(meteorData, instance);
            return skill;
        }

        // 3. 얼음 비
        if (data is IceRainData iceData)
        {
            IceRainSkill skill = player.AddComponent<IceRainSkill>();
            skill.enemyManager = this.enemyManager;
            skill.iceRainEffectPrefab = IceRainPrefab;
            skill.Init(iceData, instance);
            return skill;
        }
        return null;
    }

    // 👉 죽었을 때 초기화
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
            if (found == null)
            {
                Debug.LogWarning($"[PlayerSkillController] 저장된 스킬 '{skillName}'을 찾을 수 없습니다.");
                continue;
            }

            if (skillDict.ContainsKey(found)) continue;

            AddNewSkill(found);
        }
    }

}