using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum AIState
{
    Idle,
    Wandering,
    Attacking,
    Fleeing
}

public class NPC : MonoBehaviour, IDamagable
{
    [Header("Stats")]
    public int health;
    public float walkSpeed; // 걸을 때 스피드
    public float runSpeed; 
    public ItemData[] dropOnDeath; // 몬스터가 죽었을 때 떨어뜨릴 아이템

    [Header("AI")]
    private AIState aiState;
    public float detectDistance; // 목표 지점까지의 거리
    public float safeDistance; 

    [Header("Wandering")] // 목표지점 설정 후 이동
    public float minWanderDistance; // 최소 탐지 거리
    public float maxWanderDistance; // 최대 탐지 거리
    public float minWanderWaitTime; // 새로운 목표지점을 찍을 때 기다리는 시간
    public float maxWanderWaitTime; 

    [Header("Combat")] // 공격
    public int damage;
    public float attackRate;
    private float lastAttackTime;
    public float attackDistance;

    private float playerDistance; // 플레이어까지의 거리

    public float fieldOfView = 120f; // 시야각 => 120도  -> 플레이어가 시야각 안에 있을 때 공격

    private NavMeshAgent agent;
    private Animator animator;
    private SkinnedMeshRenderer[] meshRenderers;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>(); // 자식의 컴포넌트들을 가져옴 (공격 당했을 때 색깔 바꿔주기 위해)
    }

    private void Start()
    {
        SetState(AIState.Wandering);
    }

    private void Update()
    {
        // playerDistance = NavMesh.SamplePosition(agent.destination, out NavMeshHit _, maxWanderDistance,1 << NavMesh.GetAreaFromName("Walkable") ) ? Vector3.Distance(transform.position, CharacterManager.Instance.Player.transform.position) : ;
        playerDistance = Vector3.Distance(transform.position, CharacterManager.Instance.Player.transform.position);
        
        
        animator.SetBool("Moving", aiState != AIState.Idle);

        switch (aiState)
        {
            case AIState.Idle:
                PassiveUpdate();
                break;
            case AIState.Wandering: 
                PassiveUpdate(); 
                break;
            case AIState.Attacking: 
                AttackingUpdate(); 
                break;
            case AIState.Fleeing: 
                FleeingUpdate(); 
                break;
        }
    }

    private void SetState(AIState state) // NPC의 상황 
    {
        aiState = state;

        switch (aiState)
        {
            case AIState.Idle:
                agent.speed = walkSpeed;
                agent.isStopped = true; // 정지 상태 유무 true -> 정지해 있다.
                break;
            case AIState.Wandering:
                agent.speed = walkSpeed;
                agent.isStopped = false;
                break;
            case AIState.Attacking:
                agent.speed = runSpeed;
                agent.isStopped = false;
                break;
            case AIState.Fleeing:
                agent.speed = runSpeed;
                agent.isStopped = false;
                break;
        }

        animator.speed = agent.speed / walkSpeed; // 애니메이션의 스피드를 walkSpeed에 따라 조절 해줌
    }

    void PassiveUpdate() // Idle이나 Wandering일 때 상태 업데이트
    {
        if(aiState == AIState.Wandering && agent.remainingDistance < 0.1f) // agent.remainingDistance 목표 지점까지의 남은 거리
        {
            SetState(AIState.Idle);
            Invoke("WanderToNewLocation", Random.Range(minWanderWaitTime, maxWanderWaitTime));
        }

        NavMeshPath path = new NavMeshPath();
        if(agent.CalculatePath(CharacterManager.Instance.Player.transform.position, path) &&playerDistance < detectDistance)
        {
            SetState(AIState.Attacking);
        }
    }

    void AttackingUpdate()
    {
        if(playerDistance > attackDistance || !IsPlayerInFieldOfView()) 
        {
            agent.isStopped = false;
            NavMeshPath path = new NavMeshPath();
            // CalculatePath 경로 계산 -> 갈 수 있는 영역 계산
            if(agent.CalculatePath(CharacterManager.Instance.Player.transform.position, path)) // 목표 지점으로 갈 수 있다면
            {
                agent.SetDestination(CharacterManager.Instance.Player.transform.position); // 플레이어를 타겟
            }
            else
            {
                SetState(AIState.Fleeing);
            }
        }
        else // 공격
        {
            agent.isStopped = true;
            if(Time.time - lastAttackTime > attackRate)
            {
                lastAttackTime = Time.time;
                CharacterManager.Instance.Player.controller.GetComponent<IDamagable>().TakePhysicalDamage(damage);
                animator.speed = 1;
                animator.SetTrigger("Attack");
            }
        }
    }

    void FleeingUpdate()
    {
        if(agent.remainingDistance < 0.1f ||  !NavMesh.SamplePosition(agent.destination, out NavMeshHit _, maxWanderDistance,1 << NavMesh.GetAreaFromName("Walkable")))
        {
            SetState(AIState.Wandering);
           // agent.SetDestination(GetFleeLocation());
        }
        else
        {
            SetState(AIState.Wandering);
        }
    }

    void WanderToNewLocation()
    {
        if(aiState != AIState.Idle)
        {
            return;
        }
        SetState(AIState.Wandering);
        agent.SetDestination(GetWanderLocation());
    }

    bool IsPlayerInFieldOfView() // 시야각 검출
    {
        Vector3 directionToPlayer = CharacterManager.Instance.Player.transform.position - transform.position; // 거리
        float angle = Vector3.Angle(transform.forward, directionToPlayer); // 각
        return angle < fieldOfView * 0.5f;
    }

    Vector3 GetFleeLocation()
    {
        NavMeshHit hit;

        NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * safeDistance), out hit, maxWanderDistance, 1 << NavMesh.GetAreaFromName("Walkable"));

        int i = 0;
        while (GetDestinationAngle(hit.position) > 90 || playerDistance < safeDistance)
        {

            NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * safeDistance), out hit, maxWanderDistance, 1 << NavMesh.GetAreaFromName("Walkable"));
            i++;
            if (i == 30)
                break;
        }
        
        Debug.Log(hit.position);
        return hit.position;
    }

    Vector3 GetWanderLocation() // 목표 지점 설정
    {
        NavMeshHit hit;

        // Random.onUnitSphere 반지름이 1인 구 (가상의 구)를 생성해서 이동할 수 있는 영역을 가져옴 (여기서는 랜덤을 이용해서 그때 그때 다르게 설정해줬다)
        // 이동할 수 있는 최단거리에 대한 정보가 hit에 저장됨
        NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderDistance, maxWanderDistance)), out hit, maxWanderDistance, 1 << NavMesh.GetAreaFromName("Walkable"));

        int i = 0;
        while (Vector3.Distance(transform.position, hit.position) < detectDistance) // 도착했을 때 ?
        {
            NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderDistance, maxWanderDistance)), out hit, maxWanderDistance, 1 << NavMesh.GetAreaFromName("Walkable"));
            i++;
            if (i == 30) 
                break;
        }

        return hit.position;
    }

    float GetDestinationAngle(Vector3 targetPos)
    {
        return Vector3.Angle(transform.position - CharacterManager.Instance.Player.transform.position, transform.position + targetPos);
    }

    public void TakePhysicalDamage(int damageAmount)
    {
        health -= damageAmount;
        if (health <= 0)
            Die();

        StartCoroutine(DamageFlash());
    }

    void Die()
    {
        for (int x = 0; x < dropOnDeath.Length; x++) // 아이템 드랍
        {
            Instantiate(dropOnDeath[x].dropPrefab, transform.position + Vector3.up * 2, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    IEnumerator DamageFlash()
    {
        for (int x = 0; x < meshRenderers.Length; x++)
            meshRenderers[x].material.color = new Color(1.0f, 0.6f, 0.6f);

        yield return new WaitForSeconds(0.1f);
        for (int x = 0; x < meshRenderers.Length; x++)
            meshRenderers[x].material.color = Color.white;
    }
}
