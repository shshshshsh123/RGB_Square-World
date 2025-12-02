using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MonsterSpawner : MonoBehaviour
{
    [Header("# 카메라 바깥 스폰 설정")]
    public float spawnDistanceOutsideCamera = 5f; // 화면 경계에서 얼마나 더 멀리 스폰할지
    public float spawnHeightOffset = 1.0f; // 몬스터가 스폰될 Y축 높이
    public LayerMask groundLayer; // 지면 레이어 (Raycast용)

    private Coroutine _currentSpawnCoroutine;
    private MonsterSpawnInfo _currentSpawnInfo;
    private int _spawnCount;
    private int _totalWeight;
    private Camera _mainCamera;

    // 스폰한 몬스터 저장(킬올용) 딕셔너리쓰는이유: 오브젝트 풀러에서 풀타입도 요구함
    private Dictionary<GameObject, PoolType> _spawnedMonsters = new Dictionary<GameObject, PoolType>();

    private void Awake()
    {
        groundLayer = LayerMask.GetMask("Ground");
        _mainCamera = Camera.main;
    }

    public void StartSpawning(MonsterSpawnInfo info)
    {
        // 이전 스폰 중지하기
        StopSpawning();
        ClearAllMonsters();

        // 스폰 몬스터 없으면 시작안함
        if (info.monsterWeights == null || info.monsterWeights.Count == 0) return;

        _currentSpawnInfo = info;
        _spawnCount = 0;

        // 가중치 총합 계산
        _totalWeight = _currentSpawnInfo.monsterWeights.Sum(mw => mw.weight);
        if (_totalWeight <= 0)
        {
            Debug.LogWarning("몬스터 가중치 총합이 0 이하입니다. 스폰을 시작할 수 없습니다.");
            return;
        }

        _currentSpawnCoroutine = StartCoroutine(SpawnCoroutine());
    }

    public void StopSpawning()
    {
        if (_currentSpawnCoroutine != null)
        {
            StopCoroutine(_currentSpawnCoroutine);
            _currentSpawnCoroutine = null;
        }
    }

    IEnumerator SpawnCoroutine()
    {
        WaitForSeconds wait = new WaitForSeconds(_currentSpawnInfo.spawnInterval);

        while (true)
        {
            // 최대 마릿수 체크
            if (_currentSpawnInfo.maxSpawnCount > 0 && _spawnCount >= _currentSpawnInfo.maxSpawnCount)
            {
                yield break; // 최대치 도달 시 코루틴 종료
            }
            SpawnMonster();

            yield return wait;
        }
    }

    void SpawnMonster()
    {
        // 가중치 기반으로 몬스터 타입을 결정
        PoolType selectedType = PickRandomMonsterType();

        // 카메라 바깥 어딘가로 스폰위치 설정
        Vector3 spawnPosition = GetRandomPositionOutsideCamera();

        // 몬스터 소환
        GameObject monsterObj = ObjectPooler.Instance.SpawnFromPool(selectedType, spawnPosition, Quaternion.identity);

        if (monsterObj != null)
        {
            // 높이 보정 (지면 위로)
            RaycastHit hit;
            // 스폰 위치보다 높은 곳에서 아래로 레이 발사
            if (Physics.Raycast(monsterObj.transform.position + Vector3.up * 10f, Vector3.down, out hit, 20f, groundLayer))
            {
                monsterObj.transform.position = hit.point;
            }
            _spawnedMonsters[monsterObj] = selectedType;
            _spawnCount++;
        }
    }

    PoolType PickRandomMonsterType()
    {
        // 0부터 총 가중치 사이의 랜덤 값 선택 (예: 총합이 100이면 0~99)
        int randomValue = Random.Range(0, _totalWeight);

        // 리스트를 순회하며 랜덤 값이 어느 구간에 속하는지 확인
        foreach (var monsterWeight in _currentSpawnInfo.monsterWeights)
        {
            if (monsterWeight.weight <= 0) continue; // 가중치 0 이하는 스킵

            // 랜덤 값에서 현재 몬스터의 가중치를 뺌
            randomValue -= monsterWeight.weight;

            // 0 미만이 되면 이 몬스터가 당첨된 것임
            if (randomValue < 0)
            {
                return monsterWeight.monsterType;
            }
        }

        // 여기까지 올 일은 거의 없지만, 안전장치로 첫 번째 타입 반환
        return _currentSpawnInfo.monsterWeights[0].monsterType;
    }

    Vector3 GetRandomPositionOutsideCamera()
    {
        if (_mainCamera == null) _mainCamera = Camera.main;

        // 카메라의 6개 평면(좌,우,상,하,근,원) 계산
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_mainCamera);

        // 좌(0), 우(1), 상(3), 하(2) 중 랜덤 선택 (근/원은 제외)
        int sideIndex = Random.Range(0, 4);
        // 선택된 평면의 법선 벡터 (화면 바깥쪽 방향)
        Vector3 planeNormal = planes[sideIndex].normal;

        // 화면 중앙에서 지면까지의 거리 계산 (대략적인 스케일 파악용)
        Ray centerRay = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Plane groundPlane = new Plane(Vector3.up, new Vector3(0, spawnHeightOffset, 0));
        float distanceToGround;
        groundPlane.Raycast(centerRay, out distanceToGround);
        Vector3 centerOnGround = centerRay.GetPoint(distanceToGround);
        float viewportDist = Vector3.Distance(_mainCamera.transform.position, centerOnGround);

        // 기준점: 화면 중앙에서 선택된 평면 방향으로 일정 거리 떨어진 곳
        Vector3 basePoint = centerOnGround + planeNormal * (viewportDist * 0.6f + spawnDistanceOutsideCamera);

        // 기준점에서 옆으로 퍼트리기 위한 벡터 계산
        Vector3 spreadDir;
        float spreadFactor;

        if (sideIndex == 0 || sideIndex == 1) // 좌우측면 선택 시 -> 상하로 퍼트림
        {
            spreadDir = _mainCamera.transform.up;
            // FOV를 기반으로 적절한 퍼짐 범위 계산
            spreadFactor = viewportDist * Mathf.Tan(_mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
        }
        else // 상하측면 선택 시 -> 좌우로 퍼트림
        {
            spreadDir = _mainCamera.transform.right;
            spreadFactor = viewportDist * Mathf.Tan(_mainCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * _mainCamera.aspect;
        }

        // 최종 위치 계산 (Y축은 기본 높이)
        Vector3 finalPos = basePoint + spreadDir * Random.Range(-spreadFactor, spreadFactor);
        finalPos.y = spawnHeightOffset;

        return finalPos;
    }

    /// <summary>
    /// 현재 스포너가 관리하는 모든 몬스터를 풀로 반환합니다.
    /// </summary>
    public void ClearAllMonsters()
    {
        // 딕셔너리에 있는 모든 몬스터를 순회
        foreach (var pair in _spawnedMonsters)
        {
            GameObject monster = pair.Key;
            PoolType type = pair.Value;

            // 몬스터가 존재하고 활성화되어 있다면 반환
            if (monster != null && monster.activeSelf)
            {
                ObjectPooler.Instance.ReturnToPool(type, monster);
            }
        }

        // 딕셔너리 비우기 및 카운트 초기화
        _spawnedMonsters.Clear();
        _spawnCount = 0;
    }
}
