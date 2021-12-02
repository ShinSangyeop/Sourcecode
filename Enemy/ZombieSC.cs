using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ZombieSC : LivingEntity
{
    public LayerMask target;           // 추적 대상 레이어
    private GameObject targetEntity;   // 추적 대상
    public GameObject player;
    public GameObject attackColl;      // 공격 판정 콜라이더
    float traceRange = 10f;            // 추적 반경
    float attackDistance = 2.1f;       // 공격 거리

    private NavMeshAgent pathFinder;   // 경로 계산 에이전트
    private Animator enemyAnimator;    // 애니매이션

    [SerializeField]
    private bool isTrace = false;
    [SerializeField]
    private bool isAttack = false;
    bool isAttacking = false;

    Coroutine co_updatePath;
    Coroutine co_chageTarget;

    [SerializeField]
    List<GameObject> list = new List<GameObject>();

    LayerMask targetLayer;
    Vector3 targetPosition;
    Vector3 targetSize;
    int targetValue = 5;

    private void Awake()    //초기화
    {
        // 게임 오브젝트로부터 사용할 컴포넌트 가져오기
        pathFinder = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponent<Animator>();

        LayerMask playerLayer = 1 << LayerMask.NameToLayer("PLAYER");
        LayerMask defensiveGoodsLayer = 1 << LayerMask.NameToLayer("DEFENSIVEGOODS");

        targetLayer = playerLayer | defensiveGoodsLayer;

        _exp = 1;
    }


    protected override void OnEnable()
    {
        pathFinder.enabled = true;
        base.OnEnable();
        Setup();
        co_updatePath = StartCoroutine(UpdatePath());
        co_chageTarget = StartCoroutine(ChangeTarget());
        targetPosition = targetEntity.GetComponent<Collider>().bounds.center;
        targetSize = targetEntity.GetComponent<Collider>().bounds.size;

    }

    public void Setup(float newHP = 20f, float newAP = 0f, float newSpeed = 3f, float newDamage = 10f)
    {
        startHp = newHP;
        currHP = newHP;
        armour = newAP;
        damage = newDamage;
        pathFinder.speed = newSpeed;
    }
    /// <summary>
    /// 애니매이션 Duration 값 얻기
    /// </summary>
    /// <param name="move"></param>
    /// <returns></returns>
    public float MoveDuration(eCharacterState moveType)
    {
        string name = string.Empty;
        switch (moveType)
        {
            case eCharacterState.Trace:
                name = "zombieRunning";
                break;
            case eCharacterState.Attack:
                name = "Zombie Attack";
                break;
            case eCharacterState.Die:
                name = "Zombie Dying";
                break;
            default:
                return 0;
        }

        float time = 0;

        RuntimeAnimatorController ac = enemyAnimator.runtimeAnimatorController;

        for (int i = 0; i < ac.animationClips.Length; i++)
        {
            if (ac.animationClips[i].name == name)
            {
                time = ac.animationClips[i].length;
            }

        }
        return time;
    }


    private void Update()
    {
        //플레이어가 사거리에 들어오면 공격
        if (dead)
            return;

        if (state == eCharacterState.Trace && Vector3.Distance(new Vector3(targetPosition.x, (targetPosition.y - (targetSize.y / 2)), targetPosition.z), this.transform.position) <= attackDistance && !isAttacking)
        {
            NowAttack();
        }

        if (isAttacking == true)
        {
            Quaternion lookRot = Quaternion.LookRotation(new Vector3(targetPosition.x, (targetPosition.y - (targetSize.y / 2)), targetPosition.z) - new Vector3(this.transform.position.x, 0, this.transform.position.z));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRot, 60f * Time.deltaTime);
        }
    }

    /// <summary>
    /// 추적 대상을 찾아서 경로를 갱신
    /// </summary>
    /// <returns></returns>
    IEnumerator UpdatePath()
    {
        yield return new WaitForSeconds(0.3f);
        while (!dead)
        {
            if (pathFinder.enabled)
            {
                pathFinder.isStopped = false;
                targetPosition = targetEntity.GetComponent<Collider>().bounds.center;
                targetSize = targetEntity.GetComponent<Collider>().bounds.size;
                pathFinder.SetDestination(new Vector3(targetPosition.x, (targetPosition.y - (targetSize.y / 2)), targetPosition.z));
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    /// <summary>
    /// 추적 대상을 변경
    /// </summary>
    /// <returns></returns>
    IEnumerator ChangeTarget()
    {
        while (!dead)
        {
            // 오버랩 스피어로 범위내에 있는 PLAYER 레이어 콜라이더 추출
            Collider[] colliders = Physics.OverlapSphere(this.transform.position, traceRange, targetLayer);

            if (colliders.Length >= 1)
            {

                foreach (var collider in colliders)
                {
                    // targetValue = 0
                    if (collider.CompareTag("PLAYER"))
                    {
                        targetValue = 0;
                        targetEntity = collider.gameObject;
                        break;
                    }
                    // targetValue = 1 
                    else if (collider.CompareTag("FENCE") && targetValue > 1)
                    {
                        targetValue = 1;
                        targetEntity = collider.gameObject;
                    }
                    // targetValue = 2
                    else if (collider.CompareTag("BUNKERDOOR") && targetValue > 2)
                    {
                        targetValue = 2;
                        targetEntity = collider.gameObject;
                    }

                }

            }
            else
            {
                targetEntity = startTarget;
                targetValue = 2;
            }

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, attackDistance, 1 << LayerMask.NameToLayer("DEFENSIVEGOODS")))
            {
                if (hit.collider.CompareTag("FENCE")) { targetEntity = hit.collider.gameObject; targetValue = 1; }
            }


            yield return new WaitForSeconds(0.1f);
        }
    }

    private void OnTriggerEnter(Collider other)
    {

    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PLAYER"))
        {
            if (!list.Contains(other.gameObject))
            {
                list.Add(other.gameObject);
                isTrace = false;

                Vector3 hitPoint = other.ClosestPoint(gameObject.GetComponent<Collider>().bounds.center);

                Vector3 hitNormal = new Vector3(hitPoint.x, hitPoint.y, hitPoint.z).normalized;

                ZombieSC zombie = other.GetComponent<ZombieSC>();
                other.GetComponent<LivingEntity>().Damaged(damage, hitPoint, hitNormal);

            }

        }
        else if (other.CompareTag("BUNKERDOOR") || other.CompareTag("FENCE"))
        {

            if (!list.Contains(other.gameObject))
            {
                list.Add(other.gameObject);
                isTrace = false;

                Vector3 hitPoint = other.ClosestPoint(gameObject.GetComponent<Collider>().bounds.center);
                Vector3 hitNormal = new Vector3(hitPoint.x, hitPoint.y, hitPoint.z).normalized;

                other.GetComponent<LivingEntity>().Damaged(damage, hitPoint, hitNormal);
            }
            else
                return;

        }
    }
    private void OnTriggerExit(Collider other)
    {
        isAttack = false;
        isTrace = true;
        enemyAnimator.SetBool("IsAttack", isAttack);
        enemyAnimator.SetBool("IsTrace", isTrace);
    }

    /// <summary>
    /// 추적 함수
    /// </summary>
    void NowTrace()
    {
        state = eCharacterState.Trace;
        if (pathFinder.enabled)
        {
            pathFinder.isStopped = false;
            pathFinder.speed = 3f;
            isTrace = true;
            enemyAnimator.SetBool("IsTrace", isTrace);
        }
    }

    /// <summary>
    /// 공격 함수
    /// </summary>
    private void NowAttack()
    {
        isAttacking = true;

        pathFinder.isStopped = true;
        pathFinder.speed = 0f;
        enemyAnimator.SetTrigger("IsAttack");
        float collidertime = 0.999f;
        StartCoroutine(StartAttacking(collidertime));
        collidertime = 1.166f;
        StartCoroutine(NowAttacking(collidertime));
        float attackdelayTime = MoveDuration(eCharacterState.Attack);
        StartCoroutine(EndAttacking(attackdelayTime));
    }
    /// <summary>
    /// 리스트 초기화
    /// </summary>
    public void ClearList()
    {
        list.Clear();
    }
    /// <summary>
    /// 공격 시작시 발동되는 코루틴
    /// </summary>
    /// <param name="_delaytime"></param>
    /// <returns></returns>
    IEnumerator StartAttacking(float _delaytime)
    {
        yield return new WaitForSeconds(_delaytime);
        pathFinder.enabled = false;
    }
    /// <summary>
    /// 공격중일때 발동되는 코루틴
    /// </summary>
    /// <param name="_delaytime"></param>
    /// <returns></returns>
    IEnumerator NowAttacking(float _delaytime)
    {
        yield return new WaitForSeconds(_delaytime);
        ClearList();
    }
    /// <summary>
    /// 공격이 끝날때 발동되는 코루틴
    /// </summary>
    /// <param name="_delaytime"></param>
    /// <returns></returns>
    IEnumerator EndAttacking(float _delaytime)
    {
        yield return new WaitForSeconds(_delaytime * 0.8f);
        isAttacking = false;
        pathFinder.enabled = true;
        NowTrace();
    }
    /// <summary>
    /// 공격도중 Attackcollider 활성화 함수(애니매이션 이벤트에 넣을 함수)
    /// </summary>
    void ColliderON()
    {
        attackColl.SetActive(true);
    }

    void ColliderOFF()
    {
        attackColl.SetActive(false);
    }

    protected override void Down()
    {
        base.Down();
        pathFinder.enabled = false;
        enemyAnimator.SetTrigger("IsDead");

        StartCoroutine(WaitForDieAnimation(MoveDuration(eCharacterState.Die)));
    }

}