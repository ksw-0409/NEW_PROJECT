using System.Collections.Generic;
using UnityEngine;

// 역할: 아이템 스탯 랜덤 부여
// - Common: 추가 옵션 없음
// - Rare: CSV 범위 후보 중 2개 랜덤 선택
// - Unique: CSV 범위 후보 중 4개 랜덤 선택
// - Legendary: 후보 전부 고정 (종류 고정, 수치 롤링)

public static class ItemStatRoller
{
    public static void RollStats(InventoryItem target)
    {
        // Common은 추가 옵션 없음
        if (target.Grade == ItemGrade.Common) return;

        // 등급별 최대 옵션 수 (Legendary는 -1 = 전부)
        int maxOptions = target.Grade switch
        {
            ItemGrade.Rare => 2,
            ItemGrade.Unique => 4,
            ItemGrade.Legendary => int.MaxValue,
            _ => 0
        };
        if (maxOptions == 0) return;

        bool IsLocked(string statName)
        {
            if (target.options == null) return false;
            foreach (var opt in target.options)
                if (opt.statName == statName && opt.isLocked) return true;
            return false;
        }

        // 추가 옵션 후보: minAdd/maxAdd 범위가 있는 스탯
        var candidates = new List<string>();
        if (target.minAddPhys != 0 || target.maxAddPhys != 0) candidates.Add("physicalDamage");
        if (target.minAddMagic != 0 || target.maxAddMagic != 0) candidates.Add("magicDamage");
        if (target.minAddCrit != 0 || target.maxAddCrit != 0) candidates.Add("criticalChance");
        if (target.minAddCritDmg != 0 || target.maxAddCritDmg != 0) candidates.Add("criticalDamage");
        if (target.minAddHealth != 0 || target.maxAddHealth != 0) candidates.Add("maxHealth");
        if (target.minAddDef != 0 || target.maxAddDef != 0) candidates.Add("physicalDefense");
        if (target.minAddSpeed != 0 || target.maxAddSpeed != 0) candidates.Add("moveSpeed");

        // 잠긴 옵션은 유지, 나머지 슬롯 랜덤 채움
        var selectedStats = new List<string>();
        var unlocked = new List<string>();

        foreach (var c in candidates)
        {
            if (IsLocked(c)) selectedStats.Add(c);
            else unlocked.Add(c);
        }

        // Legendary가 아닐 때만 셔플 후 제한
        if (target.Grade != ItemGrade.Legendary)
        {
            for (int i = unlocked.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (unlocked[i], unlocked[j]) = (unlocked[j], unlocked[i]);
            }
            int remaining = Mathf.Max(0, maxOptions - selectedStats.Count);
            for (int i = 0; i < remaining && i < unlocked.Count; i++)
                selectedStats.Add(unlocked[i]);
        }
        else
        {
            // Legendary: 후보 전부 선택
            selectedStats.AddRange(unlocked);
        }

        // 선택된 스탯만 롤링, 나머지는 기본값으로
        target.physicalDamage = selectedStats.Contains("physicalDamage")
            ? target.basePhysicalDamage + Random.Range(target.minAddPhys, target.maxAddPhys)
            : target.basePhysicalDamage;
        target.magicDamage = selectedStats.Contains("magicDamage")
            ? target.baseMagicDamage + Random.Range(target.minAddMagic, target.maxAddMagic)
            : target.baseMagicDamage;
        target.criticalChance = selectedStats.Contains("criticalChance")
            ? target.baseCriticalChance + Random.Range(target.minAddCrit, target.maxAddCrit)
            : target.baseCriticalChance;
        target.criticalDamage = selectedStats.Contains("criticalDamage")
            ? target.baseCriticalDamage + Random.Range(target.minAddCritDmg, target.maxAddCritDmg)
            : target.baseCriticalDamage;
        target.maxHealth = selectedStats.Contains("maxHealth")
            ? target.baseMaxHealth + Random.Range(target.minAddHealth, target.maxAddHealth)
            : target.baseMaxHealth;
        target.physicalDefense = selectedStats.Contains("physicalDefense")
            ? target.basePhysicalDefense + Random.Range(target.minAddDef, target.maxAddDef)
            : target.basePhysicalDefense;
        target.moveSpeed = selectedStats.Contains("moveSpeed")
            ? target.baseMoveSpeed + Random.Range(target.minAddSpeed, target.maxAddSpeed)
            : target.baseMoveSpeed;
    }
}