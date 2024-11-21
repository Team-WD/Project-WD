using System.Collections;
using System.Collections.Generic;
using System.Linq; // 기존 기능을 위해 다시 추가
using Fusion;
using UnityEngine;
using UnityEngine.AI;

public class MultiEnemy : NetworkBehaviour
{
    [SerializeField] public EnemyData enemyData; // 적 데이터 (최대 체력, 속도, 공격력 등)
    
    // 네트워크 동기화된 변수
    [Networked] public int currentHp { get; set; } // 현재 적의 체력
    [Networked] public bool isDead { get; set; } // 사망 상태 플래그

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    public Collider2D enemyCollider; // Collider2D 추가
    private Animator animator; // 애니메이터 변수 선언
    public GameManager gameManager;
    // 플레이어 리스트 선언
    public List<GameObject> players = new List<GameObject>();
    
    [SerializeField] private Transform target; // 타겟 플레이어
    
    public NavMeshAgent agent; // NavMeshAgent를 이용해 자동 이동

    // 오디오 관련 필드
    public AudioSource audioSource; // 오디오 소스 컴포넌트
    public AudioClip throwSkillSound; // 던지기 스킬 효과음
    public AudioClip groundSkillSound; // 바닥 스킬 효과음
    public AudioClip deathSound; // 보스 사망 시 재생할 오디오 클립

    // 스킬 관련 스크립트 참조
    private BossSkill bossSkill; // BossSkill 스크립트 참조

    // Start is called before the first frame update
    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        // 적 데이터가 설정되지 않은 경우 오류 로그 출력
        if (enemyData == null)
        {
            Debug.LogError("enemyData is null. Please assign it in the inspector.");
            return;
        }
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
        currentHp = enemyData.MaxHp; // 적의 체력을 최대 체력으로 초기화
        enemyCollider = GetComponent<Collider2D>(); // Collider2D 컴포넌트 가져오기
        
        // 플레이어 목록 가져오기
        GetPlayersList();
        
        // 가장 가까운 플레이어를 타겟으로 설정
        FindNearestPlayer();
        
        // NavMeshAgent 초기화 및 속도 설정
        agent = GetComponent<NavMeshAgent>();
        agent.speed = enemyData.Speed;  // 이동속도 반영
        agent.updateRotation = false; // 2D 환경에서는 회전 불필요
        agent.updateUpAxis = false; // Y축 회전 제한
        animator = GetComponent<Animator>(); // 애니메이터 가져오기

        // BossSkill 스크립트 참조
        bossSkill = GetComponent<BossSkill>();
        if (bossSkill == null)
        {
            Debug.LogWarning("BossSkill 스크립트가 보스 오브젝트에 할당되지 않았습니다.");
        }
    }
    
    // 매 프레임마다 호출되는 LateUpdate 함수
    public void LateUpdate()
    {
        // 사망 상태일 때는 더 이상 움직이지 않음
        if (isDead)
        {
            return;
        }
        // 가장 가까운 플레이어를 타겟으로 설정
        FindNearestPlayer();
        
        // 타겟 위치로 이동
        if (target != null)
        {
            agent.SetDestination(target.GetChild(0).position);
            LookTargetDirection(); // 플레이어 방향으로 적을 바라봅니다.
        }
    }

    // 적의 체력을 설정하는 RPC 함수 -> 기존 RPC 방식에서 Networked로 전환
    // currentHp 변수를 네트워크 상에서 동기화하여 모든 클라이언트에 반영합니다.
    // 사망 여부도 Networked로 동기화합니다.

    // 적이 사망했을 때 호출되는 함수
    public void Die()
    {
        gameManager.AddExp(5);
        if (isDead)
            return; // 이미 사망 처리된 경우 중복 실행 방지

        isDead = true; // 사망 상태로 설정
        Debug.Log("MultiEnemy.cs Die() called. isDead set to true.");

        if (agent != null)
        {
            agent.speed = 0f; // 이동 속도를 0으로 설정
            Debug.Log("NavMeshAgent speed set to 0.");
        }

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false; // 물리적 충돌을 비활성화
            Debug.Log("Enemy collider disabled.");
        }

        // 사망 애니메이션 트리거 설정
        animator.SetTrigger("Die");
        Debug.Log("Death animation triggered.");

        // 사망 시 오디오 재생
        PlayDeathSound();

        // BossSkill 스크립트가 할당되어 있으면 모든 스킬 영역 파괴
        if (bossSkill != null)
        {
            bossSkill.HandleBossDeath();
        }

        // 추가적인 사망 처리 로직이 필요하면 여기에 추가
    }

    // 사망 시 오디오를 재생하는 메서드
    private void PlayDeathSound()
    {
        if (audioSource != null && deathSound != null)
        {
            audioSource.PlayOneShot(deathSound);
            Debug.Log("Death sound played.");
        }
        else
        {
            // if (audioSource == null)
            //     Debug.LogWarning("AudioSource가 할당되지 않았습니다.");
            // if (deathSound == null)
            //     Debug.LogWarning("DeathSound AudioClip이 할당되지 않았습니다.");
        }
    }

    // 플레이어가 세션에 참가할 때 호출되는 함수
    public void OnPlayerJoined()
    {
        GetPlayersList(); // 플레이어 리스트를 갱신
        FindNearestPlayer(); 
    }

    // 플레이어가 세션에서 나갈 때 호출되는 함수
    public void OnPlayerLeft()
    {
        GetPlayersList(); // 플레이어 리스트를 갱신
        FindNearestPlayer(); 
    }
    
    // 현재 활성화된 플레이어 리스트를 가져오는 함수
    void GetPlayersList()
    {
        // 플레이어 리스트 초기화
        players.Clear();

        // 모든 플레이어를 리스트에 추가
        foreach (PlayerRef playerRef in NetworkManager.runnerInstance.ActivePlayers)
        {
            NetworkObject playerObject = NetworkManager.runnerInstance.GetPlayerObject(playerRef);
            
            if (playerObject != null)
            {
                players.Add(playerObject.gameObject);
            }
            else
            {
                Debug.LogWarning($"Player object is null for PlayerRef: {playerRef}");
            }
        }
    }

    // 가장 가까운 플레이어를 찾는 함수
    void FindNearestPlayer()
    {
        if (isDead) return; // 적이 사망한 상태에서는 타겟을 찾지 않음

        float shortestDistance = Mathf.Infinity; // 무한대로 초기 설정
        GameObject nearestPlayer = null;

        // 모든 플레이어와의 거리 계산
        foreach (GameObject player in players)
        {
            if (player.tag == "Player") // 살아있는 플레이어만 대상으로 함
            {
                float distance = Vector3.Distance(transform.position, player.transform.GetChild(0).position);

                // 가장 가까운 플레이어를 찾음
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestPlayer = player;
                }
            }
        }

        // 가장 가까운 플레이어를 타겟으로 설정
        if (nearestPlayer != null)
        {
            target = nearestPlayer.transform;
        }
        else
        {
            target = null; // 가까운 플레이어가 없으면 타겟을 null로 설정
        }
    }

    // 타겟 방향으로 적을 바라보게 하는 함수
    void LookTargetDirection()
    {
        if (target == null) return;

        float xDifference = target.GetChild(0).position.x - transform.position.x;

        // 타겟이 왼쪽 또는 오른쪽에 있는 경우 적의 방향을 조정
        if (xDifference > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (xDifference < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    // 충돌 시 공격 애니메이션을 실행, 트리거 신호 딜레이 적용
    // 적이 플레이어와 충돌했을 때 지속적으로 호출되는 함수
    void OnCollisionStay2D(Collision2D collision)
    {
        // 적이 살아있을 때만 충돌 처리
        if (!isDead && !enemyData.IsRanged && collision.gameObject.CompareTag("Player"))
        {
            Player player = collision.gameObject.GetComponent<Player>();
            if (player != null && !animator.GetBool("IsAttacking")) // IsAttacking이 false일 때만 실행
            {
                animator.SetBool("IsAttacking", true); // 근접 공격 애니메이션 트리거
                player.ApplyDamage(enemyData.Damage); // 데미지 적용

                // 0.2초 동안 트리거 신호가 다시 발생하지 않도록 딜레이 적용
                StartCoroutine(DelayTrigger(0.25f));

                // 공격이 끝난 후 애니메이션 트리거를 false로 되돌리는 Invoke 설정
                Invoke("StopAttack", 1f / enemyData.AttackSpeed); // 공격 속도에 맞춰 다시 공격 가능
            }
        }
    }
    
    // 딜레이 적용을 위한 코루틴
    IEnumerator DelayTrigger(float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.SetBool("IsAttacking", false); // 트리거 신호 딜레이 후 다시 false로 설정 가능
    }

    // 원거리 공격 처리
    void RangedAttack()
    {
        animator.SetBool("IsAttacking", true); // 원거리 공격 시 애니메이션 트리거
        Debug.Log($"{enemyData.Name} performed a ranged attack!");
        // 원거리 공격 로직 (예: 투사체 발사)
    }

    // 애니메이션 종료를 위한 함수
    void StopAttack()
    {
        animator.SetBool("IsAttacking", false); // 공격 애니메이션 트리거를 해제하여 다시 공격 가능
    }
}
