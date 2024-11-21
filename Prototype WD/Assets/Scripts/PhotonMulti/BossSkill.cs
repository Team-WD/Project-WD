using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BossSkill : MonoBehaviour
{
    private Animator animator;
    private float skillCooldown = 7.0f; // 스킬 발동 간격
    private bool isUsingSkill = false;
    private bool isCooldown = false; // 쿨타임 플래그 추가
    private float initialDelay = 5.0f; // 첫 스킬 발동 전 대기 시간

    private NavMeshAgent agent; // 보스 이동 제어를 위한 NavMeshAgent

    // 바닥 스킬 관련 변수
    public GameObject groundSkillPrefab; // 바닥 스킬 범위 표시용 프리팹
    public GameObject skillEndEffectPrefab; // 스킬 종료 애니메이션 프리팹 추가
    public int groundSkillDamage = 9; // 바닥 스킬의 데미지

    // 던지기 스킬 관련 변수
    public GameObject areaSkillPrefab; // 붉은 영역 프리팹
    public float areaSkillRange = 10f; // 보스 주변 스킬 발동 범위
    public int areaSkillDamage = 9; // 던지기 스킬 데미지
    public float areaSkillDelay = 1f; // 투명도 증가 시간 및 데미지 적용까지의 딜레이
    public int areaSkillRepeats = 3; // 스킬 반복 횟수
    public float areaSkillInterval = 1f; // 스킬 반복 간격

    // 폭발 애니메이션 프리팹
    public GameObject explosionPrefab; // 폭발 애니메이션 프리팹

    // 오디오 관련 필드
    public AudioSource audioSource; // 오디오 소스 컴포넌트
    public AudioClip throwSkillSound; // 던지기 스킬 효과음
    public AudioClip groundSkillSound; // 바닥 스킬 효과음

    // 사망 오디오 참조 (중복 방지를 위해 제거)
    // public AudioClip deathSound; // 보스 사망 시 재생할 오디오 클립

    private Transform target; // 스킬 대상 타겟
    private HashSet<GameObject> damagedPlayers = new HashSet<GameObject>(); // 데미지를 이미 받은 플레이어 목록

    // 활성화된 스킬 영역을 추적하기 위한 리스트
    private List<GameObject> activeSkillAreas = new List<GameObject>();

    // Reference to MultiEnemy script for death status
    private MultiEnemy multiEnemy;

    void Start()
    {
        // 보스 오브젝트의 Animator와 NavMeshAgent 컴포넌트 가져오기
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        // 오디오 소스 초기화
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        // MultiEnemy 스크립트 참조
        multiEnemy = GetComponent<MultiEnemy>();
        if (multiEnemy == null)
        {
            Debug.LogWarning("MultiEnemy 스크립트가 보스 오브젝트에 할당되지 않았습니다.");
        }

        // 첫 스킬 발동 전에 대기
        StartCoroutine(InitialDelay());
    }

    // 첫 스킬 발동 전에 대기하는 코루틴
    IEnumerator InitialDelay()
    {
        yield return new WaitForSeconds(initialDelay);
        StartCoroutine(SkillRoutine());
    }

    // 스킬 발동을 관리하는 코루틴
    IEnumerator SkillRoutine()
    {
        while (!multiEnemy.isDead)
        {
            if (!isUsingSkill && !isCooldown)
            {
                FindTarget();

                int randomSkill = Random.Range(0, 2); // 0 또는 1
                if (randomSkill == 0)
                {
                    UseThrowSkill();
                }
                else
                {
                    UseGroundSkill();
                }
            }

            yield return null;
        }
    }

    // 가장 가까운 플레이어를 타겟으로 설정
    void FindTarget()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPlayer = player.transform;
            }
        }

        target = closestPlayer;
    }

    // 던지기 스킬 사용
    void UseThrowSkill()
    {
        Debug.Log("ThrowSkill 발동!");
        isUsingSkill = true;
        isCooldown = true;
        animator.SetTrigger("ThrowSkill");
        agent.isStopped = true;

        // 스킬 코루틴 시작
        StartCoroutine(ThrowSkillRoutine());

        // 쿨타임 시작
        StartCoroutine(CooldownTimer());
    }

    // 던지기 스킬 코루틴
    IEnumerator ThrowSkillRoutine()
    {
        for (int i = 0; i < areaSkillRepeats; i++)
        {
            // 최초 1회만 0.5초 딜레이 추가
            if (i == 0)
            {
                yield return new WaitForSeconds(0.5f);
            }

            if (multiEnemy.isDead)
                yield break; // 보스가 사망하면 스킬 발동 중단

            // 보스 주변의 플레이어 찾기
            Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, areaSkillRange);
            List<Transform> playersInRange = new List<Transform>();
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    playersInRange.Add(hitCollider.transform);
                }
            }

            if (playersInRange.Count > 0)
            {
                // 무작위로 한 명 선택
                int randomIndex = Random.Range(0, playersInRange.Count);
                Transform selectedPlayer = playersInRange[randomIndex];

                // 스킬 위치 설정
                Vector3 skillPosition = selectedPlayer.position;

                // 붉은 영역 프리팹 생성
                GameObject areaSkill = Instantiate(areaSkillPrefab, skillPosition, Quaternion.identity);
                activeSkillAreas.Add(areaSkill); // 활성화된 스킬 영역 리스트에 추가

                // 투명도 증가 및 데미지 적용 코루틴 시작
                StartCoroutine(FadeInAndDamage(areaSkill, areaSkillDelay));
            }

            // 다음 스킬 발동까지 대기
            yield return new WaitForSeconds(areaSkillInterval);
        }

        // 스킬 종료 처리
        isUsingSkill = false;
        agent.isStopped = false;
    }

    // 투명도 증가 및 데미지 적용 코루틴
    IEnumerator FadeInAndDamage(GameObject areaSkill, float delayTime)
    {
        SpriteRenderer spriteRenderer = areaSkill.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;

            float fadeDuration = delayTime;
            float elapsed = 0f;

            // PolygonCollider2D 가져오기
            PolygonCollider2D skillCollider = areaSkill.GetComponent<PolygonCollider2D>();
            if (skillCollider != null)
            {
                // Collider를 Trigger로 설정하여 물리적 충돌 방지
                skillCollider.isTrigger = true;
            }
            else
            {
                Debug.LogError("프리팹에 PolygonCollider2D가 없습니다.");
                yield break; // 콜라이더가 없으면 더 이상 진행하지 않음
            }

            while (elapsed < fadeDuration)
            {
                if (multiEnemy.isDead)
                    yield break; // 보스가 사망하면 스킬 진행 중단

                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                color.a = alpha;
                spriteRenderer.color = color;
                yield return null;
            }

            // 투명도 최대치로 설정
            color.a = 1f;
            spriteRenderer.color = color;

            // 폭발 애니메이션 재생
            PlayExplosionAnimation(areaSkill.transform.position);

            // 즉시 스킬 오브젝트 파괴
            Destroy(areaSkill);
            activeSkillAreas.Remove(areaSkill); // 파괴된 스킬 영역을 리스트에서 제거

            // 데미지 적용
            ApplyAreaSkillDamage(areaSkill.transform.position);
        }
        else
        {
            Debug.LogError("areaSkillPrefab에 SpriteRenderer가 없습니다.");
        }
    }

    // 데미지를 적용하는 메서드
    void ApplyAreaSkillDamage(Vector3 position)
    {
        // Collider의 경계를 이용하여 OverlapCircleAll 사용
        // areaSkillPrefab의 Collider2D가 이미 파괴되었으므로, 위치 기반으로 데미지를 적용
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(position, 1.5f); // 1.5f는 데미지를 적용할 범위. 필요에 따라 조정
        foreach (Collider2D hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Player"))
            {
                Player playerScript = hitCollider.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.ApplyDamage(areaSkillDamage);
                    Debug.Log($"플레이어 {hitCollider.name}에게 던지기 스킬 데미지 적용 완료");
                }
                else
                {
                    Debug.LogError($"플레이어 스크립트가 {hitCollider.name}에서 발견되지 않았습니다.");
                }
            }
        }
    }

    // 폭발 애니메이션 재생 함수
    void PlayExplosionAnimation(Vector3 position)
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, position, Quaternion.identity); // 회전 제거
        }
        else
        {
            Debug.LogError("폭발 프리팹이 할당되지 않았습니다.");
        }
    }

    // 바닥 스킬 사용
    void UseGroundSkill()
    {
        Debug.Log("GroundSkill 발동!");
        isUsingSkill = true;
        isCooldown = true;
        animator.SetTrigger("GroundSkill");
        agent.isStopped = true;

        // 이미 데미지를 받은 플레이어 목록 초기화
        damagedPlayers.Clear(); // 새로운 스킬 발동 시 초기화

        if (target != null)
        {
            // 방향 및 위치 계산
            Vector3 direction = (target.position - transform.position).normalized;

            // 보스가 플레이어를 바라보는 각도 계산
            float angleToPlayer = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 스킬 범위 표시용 각도 계산 (필요에 따라 조정 가능)
            float skillRangeAngle = angleToPlayer;

            // 스킬 범위를 보스 위치에서 일정 거리 떨어진 위치에 배치
            Vector3 skillPosition = transform.position + direction * 2.5f;

            // 스킬 프리팹 생성
            GameObject skillRange = Instantiate(groundSkillPrefab, skillPosition, Quaternion.Euler(0, 0, skillRangeAngle));
            activeSkillAreas.Add(skillRange); // 활성화된 스킬 영역 리스트에 추가

            // 플레이어가 보스의 왼쪽에 있는지 확인하여 이펙트의 좌우 및 상하 반전 결정
            bool flipEffect = target.position.x < transform.position.x;
            bool flipEffectVertical = target.position.x > transform.position.x;

            // 투명도 증가 코루틴 시작 (angleToPlayer, flipEffect, flipEffectVertical을 추가 파라미터로 전달)
            StartCoroutine(FadeInSkillRange(skillRange, angleToPlayer, flipEffect, flipEffectVertical));
        }

        StartCoroutine(CooldownTimer());
    }

    // 투명도를 서서히 증가시키는 코루틴 (수정됨)
    IEnumerator FadeInSkillRange(GameObject skillRange, float angleToPlayer, bool flipEffect, bool flipEffectVertical)
    {
        SpriteRenderer spriteRenderer = skillRange.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = 0f;
            spriteRenderer.color = color;

            float fadeDuration = 1.5f; // 데미지 적용 타이밍과 정확히 일치하도록 설정
            float elapsed = 0f;

            // PolygonCollider2D 가져오기
            PolygonCollider2D skillCollider = skillRange.GetComponent<PolygonCollider2D>();
            if (skillCollider != null)
            {
                // Collider를 Trigger로 설정하여 물리적 충돌 방지
                skillCollider.isTrigger = true;
            }
            else
            {
                Debug.LogError("프리팹에 PolygonCollider2D가 없습니다.");
                yield break; // 콜라이더가 없으면 더 이상 진행하지 않음
            }

            while (elapsed < fadeDuration)
            {
                if (multiEnemy.isDead)
                    yield break; // 보스가 사망하면 스킬 진행 중단

                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
                color.a = alpha;
                spriteRenderer.color = color;
                yield return null;
            }

            // 투명도 최대치로 설정
            color.a = 1f;
            spriteRenderer.color = color;

            // 데미지 적용
            StartCoroutine(ApplySkillDamage(skillCollider));

            // 스킬 종료 이펙트 생성 (즉시 실행)
            Destroy(skillRange, 0f); // 즉시 파괴
            activeSkillAreas.Remove(skillRange); // 파괴된 스킬 영역을 리스트에서 제거
            StartCoroutine(SpawnSkillEndEffect(transform.position, angleToPlayer, 0f, flipEffect, flipEffectVertical)); // delayTime=0f
        }
        else
        {
            Debug.LogError("SkillRange에 SpriteRenderer가 없습니다.");
        }
    }

    // 스킬 데미지를 적용하는 코루틴 (PolygonCollider2D 기준)
    IEnumerator ApplySkillDamage(PolygonCollider2D skillCollider)
    {
        Debug.Log("PolygonCollider2D를 기준으로 스킬 데미지 적용 중...");

        // 모든 플레이어 객체를 찾습니다.
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        foreach (GameObject player in players)
        {
            // 플레이어의 위치를 가져옵니다.
            Vector2 playerPosition = player.transform.position;

            // 플레이어의 위치가 PolygonCollider2D 내에 있는지 확인합니다.
            if (skillCollider.OverlapPoint(playerPosition))
            {
                Player playerScript = player.GetComponent<Player>();
                if (playerScript != null)
                {
                    playerScript.ApplyDamage(groundSkillDamage);
                    Debug.Log($"플레이어 {player.name}에게 바닥 스킬 데미지 적용 완료");
                }
                else
                {
                    Debug.LogError($"플레이어 스크립트가 {player.name}에서 발견되지 않았습니다.");
                }
            }
        }

        yield return null; // 코루틴 종료
    }

    // 스킬 종료 이펙트 생성 코루틴
    IEnumerator SpawnSkillEndEffect(Vector3 position, float angle, float delayTime, bool flipEffect, bool flipEffectVertical)
    {
        yield return new WaitForSeconds(delayTime); // delayTime=0f로 즉시 실행

        // 이펙트의 길이 (스프라이트의 반대 방향으로 이동할 거리)
        float effectLength = 2f; // 필요에 따라 조정

        // 회전 각도를 라디안으로 변환 (180도 추가)
        float angleRad = (angle + 180f) * Mathf.Deg2Rad;

        // 위치 보정을 위한 오프셋 계산
        Vector3 offset = new Vector3(-Mathf.Cos(angleRad), -Mathf.Sin(angleRad), 0) * effectLength;

        // 이펙트의 위치를 보정
        Vector3 effectPosition = position + offset;

        // 이펙트 생성 시 회전 제거
        GameObject effect = Instantiate(skillEndEffectPrefab, effectPosition, Quaternion.identity);
        activeSkillAreas.Add(effect); // 활성화된 스킬 영역 리스트에 추가

        // 좌우 반전 적용
        if (flipEffect)
        {
            Vector3 scale = effect.transform.localScale;
            scale.x *= -1; // 좌우 반전 필요 시 -1로 변경
            effect.transform.localScale = scale;
        }

        // 상하 반전 적용
        if (flipEffectVertical)
        {
            Vector3 scale = effect.transform.localScale;
            scale.y *= -1;
            effect.transform.localScale = scale;
        }
    }

    // 스킬 종료 함수 (애니메이션 이벤트에서 호출)
    public void EndSkill()
    {
        Debug.Log("스킬 종료");
        isUsingSkill = false;
        agent.isStopped = false;
    }

    // 던지기 스킬 효과음 재생 (애니메이션 이벤트에서 호출)
    public void PlayThrowSkillSound()
    {
        if (audioSource != null && throwSkillSound != null)
        {
            audioSource.PlayOneShot(throwSkillSound);
        }
    }

    // 바닥 스킬 효과음 재생 (애니메이션 이벤트에서 호출)
    public void PlayGroundSkillSound()
    {
        if (audioSource != null && groundSkillSound != null)
        {
            audioSource.PlayOneShot(groundSkillSound);
        }
    }

    // 스킬 쿨타임 관리 코루틴
    IEnumerator CooldownTimer()
    {
        yield return new WaitForSeconds(skillCooldown);
        isCooldown = false;
    }

    // 보스 사망 시 스킬 영역 모두 파괴하는 메서드
    public void HandleBossDeath()
    {
        foreach (GameObject skillArea in activeSkillAreas)
        {
            if (skillArea != null)
            {
                Destroy(skillArea);
            }
        }
        activeSkillAreas.Clear(); // 리스트 초기화
    }
}
