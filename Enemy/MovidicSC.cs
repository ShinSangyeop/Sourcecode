using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovidicSC : LivingEntity
{
    public LayerMask target;

    private GameObject targetEntity;
    public GameObject attackColl;
    private GameObject pastTarget; // 이전 타겟 저장 변수

    float traceRange = 15f;
    float attackDistance = 7f;
    float rushDistance = 20f;

    private NavMeshAgent pathFinder;
    private Animator enemyAnimator;
    private Rigidbody rigid;

    [SerializeField]
    private bool isTrace = false;
    [SerializeField]
    private bool isAttacking = false;
    private bool canRush = true;
    public bool isRush = false;

    Coroutine co_updatePath;
    Coroutine co_changeTarget;

    List<GameObject> list = new List<GameObject>();

    Vector3 targetPosition;
    Vector3 targetSize;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        pathFinder = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponent<Animator>();
        Setup();

        _exp = 4f;
    }

    protected override void OnEnable()
    {
        pathFinder.enabled = true;
        base.OnEnable();
        NowTrace();
        co_updatePath = StartCoroutine(UpdatePath());
        co_changeTarget = StartCoroutine(ChangeTarget());
        targetPosition = targetEntity.GetComponent<Collider>().bounds.center;
        targetSize = targetEntity.GetComponent<Collider>().bounds.size;
    }
    /// <summary>
    /// 초기 스탯 설정
    /// </summary>
    /// <param name="newHp"></param>
    /// <param name="newAP"></param>
    /// <param name="newSpeed"></param>
    /// <param name="newDamage"></param>
    public void Setup(float newHp = 300f, float newAP = 5f, float newSpeed = 2f, float newDamage = 20f)
    {
        startHp = newHp;
        currHP = newHp;
        armour = newAP;
        damage = newDamage;
        pathFinder.speed = newSpeed;
    }
    /// <summary>
    /// 애니매이션 Duration 값 얻기
    /// </summary>
    /// <param name="moveType"></param>
    /// <returns></returns>
    public float MoveDuration(eCharacterState moveType)
    {
        string name = string.Empty;
        switch (moveType)
        {
            case eCharacterState.Trace:
                name = "Run";
                break;
            case eCharacterState.Attack:
                name = "AttackBite";
                break;
            case eCharacterState.Die:
                name = "DeathHit";
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
    void Start()
    {

    }

    void Update()
    {
        if (dead)
            return;

        if (state == eCharacterState.Trace && Vector3.Distance(new Vector3(targetPosition.x, (targetPosition.y - (targetSize.y / 2)), targetPosition.z), this.transform.position) <= rushDistance && !isAttacking)
        {
            if (canRush == true)
            {
                isRush = true;
                pastTarget = targetEntity.gameObject;
                pathFinder.enabled = false;
                isTrace = false;
                enemyAnimator.SetBool("IsTrace", isTrace);

                canRush = false;
                Invoke("RushAttack", 2f);
            }

            if (Vector3.Distance(targetEntity.transform.position, this.transform.position) <= attackDistance && !isRush)
            {
                NowAttack();
            }
        }

        if (isAttacking == true || isRush == true)
        {
            Quaternion LookRot = Quaternion.LookRotation(new Vector3(targetPosition.x, (targetPosition.y - (targetSize.y / 2)), targetPosition.z) - new Vector3(this.transform.position.x, 0, this.transform.position.z));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, LookRot, 60f * Time.deltaTime);
        }

        if (pastTarget != null && targetEntity.gameObject != pastTarget.gameObject)
        {
            canRush = true;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("BUNKERDOOR"))
        {
            if (!list.Contains(other.gameObject))
            {
                list.Add(other.gameObject);
                isTrace = false;

                Vector3 hitPoint = other.ClosestPoint(gameObject.GetComponent<Collider>().bounds.center);
                Vector3 hitnormal = new Vector3(hitPoint.x, hitPoint.y, hitPoint.z).normalized;

                MovidicSC modivic = other.GetComponent<MovidicSC>();
                other.GetComponent<LivingEntity>().Damaged(damage, hitPoint, hitnormal);
            }
            else
                return;
        }
        else if (other.CompareTag("FENCE"))
        {
            if (!list.Contains(other.gameObject))
            {
                list.Add(other.gameObject);
                isTrace = false;

                Vector3 hitPoint = other.ClosestPoint(gameObject.GetComponent<Collider>().bounds.center);
                Vector3 hitnormal = new Vector3(hitPoint.x, hitPoint.y, hitPoint.z).normalized;

                MovidicSC modivic = other.GetComponent<MovidicSC>();
                other.GetComponent<LivingEntity>().Damaged(damage, hitPoint, hitnormal);
            }
            else
                return;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        isTrace = true;

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
    void NowAttack()
    {
        damage = 20f;
        isAttacking = true;

        state = eCharacterState.Attack;

        pathFinder.enabled = true;
        pathFinder.speed = 0f;
        enemyAnimator.SetTrigger("IsAttack");
        float attackTime = 0.5f;
        StartCoroutine(StartAttacking(attackTime));
        attackTime = 0.6f;
        StartCoroutine(NowAttacking(attackTime));
        float attackdelayTime = MoveDuration(eCharacterState.Attack);
        StartCoroutine(EndAttacking(attackdelayTime));

    }
    /// <summary>
    /// 돌진 공격 함수
    /// </summary>
    void RushAttack()
    {
        // 돌진 데미지 설정 및 돌진 속도 설정
        StartCoroutine(RushColliderSetting());
        damage = 40f;
        rigid.AddForce(this.transform.forward * 40f, ForceMode.Impulse);
        enemyAnimator.SetTrigger("IsAttack");
    }

    [SerializeField]
    Collider bodyCollider1;
    [SerializeField]
    Collider bodyCollider2;

    /// <summary>
    /// 돌진중 오브젝트 Collider 설정변화
    /// </summary>
    /// <returns></returns>
    IEnumerator RushColliderSetting()
    {
        rigid.isKinematic = false;
        bodyCollider1.isTrigger = true;
        bodyCollider2.isTrigger = true;
        yield return new WaitForSeconds(0.5f);
        bodyCollider1.isTrigger = false;
        bodyCollider2.isTrigger = false;
        rigid.isKinematic = true;

    }

    public void ClearList()
    {
        list.Clear();
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

            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.forward, out hit, rushDistance, (1 << LayerMask.NameToLayer("DEFENSIVEGOODS")) | (1 << LayerMask.NameToLayer("WALL"))))
            {

                if (hit.collider.CompareTag("FENCE"))
                {
                    targetEntity = hit.collider.gameObject;
                }
                else
                {
                    targetEntity = startTarget;
                }
            }
            else
                targetEntity = startTarget;

            yield return new WaitForSeconds(0.1f);
        }
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
    /// 공격 중일때 발동되는 코루틴
    /// </summary>
    /// <param name="_delaytime"></param>
    /// <returns></returns>
    IEnumerator NowAttacking(float _delaytime)
    {
        yield return new WaitForSeconds(_delaytime);
        ClearList();
    }
    /// <summary>
    /// 공격이 끝나면 발동되는 코루틴
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
    /// 공격시 Attackcollider 활성화 함수(애니매이션 이벤트에 넣을 함수)
    /// </summary>
    void ColliderOn()
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
