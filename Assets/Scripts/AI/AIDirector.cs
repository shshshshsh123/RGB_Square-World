using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AIDirector : MonoBehaviour
{
    public static AIDirector Instance;
    public float CurrentMonsterSpacingRadius { get; private set; } = 0.3f;  // 기본값 0.3입니다다다.


    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // 미션 시작 시 초기화
    public void InitializeAIState()
    {
        CurrentMonsterSpacingRadius = 0.3f; // 기본값으로 초기화
    }

    /// <summary>
    /// 원본 스폰 정보를 받아서, AI 분석 결과가 반영된 새로운 스폰 정보를 반환합니다.
    /// </summary>
    public MonsterSpawnInfo GetAdjustedSpawnInfo(MonsterSpawnInfo originalInfo)
    {
        // 1. 원본 데이터 보호를 위해 깊은 복사(Deep Copy) 생성
        MonsterSpawnInfo adjustedInfo = CloneSpawnInfo(originalInfo);

        // 2. 플레이어 데이터 분석
        int meleeKills = DataManager.Instance.GetKillCount(WeaponType.Melee);
        int rangedKills = DataManager.Instance.GetKillCount(WeaponType.Ranged);
        int magicKills = DataManager.Instance.GetKillCount(WeaponType.Magic);

        int totalRangedBasedKills = rangedKills + magicKills;   // 원거리 기반 공격(활 + 마법) 합산
        int totalKills = meleeKills + totalRangedBasedKills;    // 전체 킬 수

        if (totalKills < 5) return adjustedInfo; // 데이터가 너무 적으면 변동 없음

        // 3. 성향 파악 및 가중치 조작
        // (A) 플레이어가 원거리(활/마법) 위주로 플레이 중 -> 근접 돌진 몬스터 비중 대폭 증가 (카운터)
        if (totalRangedBasedKills > totalKills * 0.6f)
        {
            Debug.Log("[AI 디렉터] 플레이어가 원거리 공격을 선호합니다. 근접 몬스터 비율을 높입니다.");
            ModifyWeight(adjustedInfo, PoolType.MeleeMonster, 2.0f); // 가중치 2배
            // (A-1) 마법무기라면 몬스터간의 간격을 벌려서 범위공격에 쓸리지 않도록 함
            if (magicKills > rangedKills)
            {
                Debug.Log("[AI 디렉터] 플레이어가 마법 무기를 선호합니다. 몬스터 간격을 벌립니다.");
                CurrentMonsterSpacingRadius = 1.0f; // 마법 무기 선호 시 간격 1.0f로 증가
            }
        }
        // (B) 플레이어가 근접(칼) 위주로 플레이 중 -> 도망 다니는 원거리 몬스터 비중 증가 (카운터)
        else if (meleeKills > totalKills * 0.6f)
        {
            Debug.Log("[AI 디렉터] 플레이어가 근접 공격을 선호합니다. 도망치는 원거리 몬스터 비율을 높입니다.");
            ModifyWeight(adjustedInfo, PoolType.RangedMonster_Defensive, 2.5f); // 가중치 2.5배
            ModifyWeight(adjustedInfo, PoolType.RangeMonster, 1.5f); // 일반 원거리도 1.5배
        }

        return adjustedInfo;
    }

    /// <summary>
    /// 특정 몬스터 타입의 가중치를 배율만큼 증가시킵니다.
    /// </summary>
    private void ModifyWeight(MonsterSpawnInfo info, PoolType targetType, float multiplier)
    {
        for (int i = 0; i < info.monsterWeights.Count; i++)
        {
            if (info.monsterWeights[i].monsterType == targetType)
            {
                MonsterWeight mw = info.monsterWeights[i];
                mw.weight = Mathf.RoundToInt(mw.weight * multiplier);
                info.monsterWeights[i] = mw; // 리스트에 다시 할당
            }
        }
    }

    /// <summary>
    /// MonsterSpawnInfo는 클래스이고 내부는 리스트이므로, 원본 훼손 방지를 위해 깊은 복사를 수행합니다.
    /// </summary>
    private MonsterSpawnInfo CloneSpawnInfo(MonsterSpawnInfo original)
    {
        MonsterSpawnInfo clone = new MonsterSpawnInfo();
        clone.spawnInterval = original.spawnInterval;
        clone.maxSpawnCount = original.maxSpawnCount;

        // 리스트 내부의 구조체들도 새로 복사해서 리스트 생성
        clone.monsterWeights = new List<MonsterWeight>(original.monsterWeights);

        return clone;
    }
}