using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    [SerializeField] public EnemyData enemyData;
    public int currentHp;

    private Rigidbody2D rb;
    private Animator animator; // 애니메이터 변수 선언

    // 플레이어 리스트 선언
    public List<GameObject> players = new List<GameObject>();
    [SerializeField] private Transform target;
    private UnityEngine.AI.NavMeshAgent agent;

    private float lastAttackTime;

    void Start()
    {
        // 현재 체력 초기화
        currentHp = enemyData.MaxHp;

        // 플레이어 목록 가져오기
        GetPlayersList();

        // 가장 가까운 플레이어를 타겟으로 설정
        FindNearestPlayer();

        // 자동 이동 로직
        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        agent.speed = enemyData.Speed; // 이동속도 반영
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        animator = GetComponent<Animator>(); // 애니메이터 가져오기
    }

    // Update is called once per frame
    void Update()
    {
        FindNearestPlayer();

        if (target != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            if (enemyData.IsRanged)
            {
                if (distanceToTarget <= enemyData.AttackRange)
                {
                    StopMoving();
                    if (Time.time - lastAttackTime >= 1f / enemyData.AttackSpeed)
                    {
                        RangedAttack();
                        lastAttackTime = Time.time;
                    }
                }
                else
                {
                    MoveTowardsTarget();
                }
            }
            else
            {
                MoveTowardsTarget();
            }

            LookTargetDirection();
        }
    }

    void MoveTowardsTarget()
    {
        agent.isStopped = false;
        agent.SetDestination(target.position);
    }

    void StopMoving()
    {
        agent.isStopped = true;
    }

    // "Player" 태그를 가진 오브젝트를 리스트에 추가하는 함수
    void GetPlayersList()
    {
        // "Player" 태그를 가진 모든 오브젝트를 찾음
        players.Clear(); // 리스트를 초기화하여 중복 추가를 방지
        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player"))
        {
            players.Add(player);
        }
    }

    void FindNearestPlayer()
    {
        float shortestDistance = Mathf.Infinity; // 무한대로 초기 설정
        GameObject nearestPlayer = null;

        // 모든 플레이어와의 거리 계산
        foreach (GameObject player in players)
        {
            if (player.tag == "Player") // 살아있는 플레이어만 대상으로 함
            {
                float distance = Vector3.Distance(transform.position, player.transform.position);

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

    void LookTargetDirection()
    {
        float xDifference = target.position.x - transform.position.x;

        if (xDifference > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y,
                transform.localScale.z);
        }
        else if (xDifference < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y,
                transform.localScale.z);
        }
    }

    // 충돌 시 공격 애니메이션을 실행, 트리거 신호 딜레이 적용
    void OnCollisionStay2D(Collision2D collision)
    {
        if (!enemyData.IsRanged && collision.gameObject.CompareTag("Player"))
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
