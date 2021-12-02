using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ClutchSC : LivingEntity
{
    public LayerMask target;
    private GameObject targetEntity;
    public GameObject attackColl;

    float traceRange = 10f;
    float attackDistance = 2.5f;

    private NavMeshAgent pathFinder;
    private Animator enemyAnimator;

    [SerializeField]
    private bool isTrace = false;
    private bool isAttacking = false;
    private bool isBleed = false;

    Coroutine co_updatePath;
    Coroutine co_changeTarget;

    List<GameObject> list = new List<GameObject>();

    LayerMask targetLayer;
    Vector3 targetPosition;
    Vector3 targetSize;
    int targetValue = 5;

    private void Awake()
    {
        pathFinder = GetComponent<NavMeshAgent>();
        enemyAnimator = GetComponent<Animator>();
        Setup();

        LayerMask playerLayer = 1 << LayerMask.NameToLayer("PLAYER");
        LayerMask defensiveGoodsLayer = 1 << LayerMask.NameToLayer("DEFENSIVEGOODS");

        targetLayer = playerLayer | defensiveGoodsLayer;

        _exp = 2f;
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

    public void Setup(float newHP = 100f, float newAP = 5f, float newSpeed = 6f, float newDamage = 9f)
    {
        startHp = newHP;
        currHP = newHP;
        armour = newAP;
        damage = newDamage;
        pathFinder.speed = newSpeed;
    }

    public float MoveDuration(eCharacterState moveType)
    {
        string name = string.Empty;
        switch (moveType)
        {
            case eCharacterState.Trace:
                name = "Armature_Walk_Cycle_2";
                break;
            case eCharacterState.Attack:
                name = "Armature_Attack_4";
                break;
            case eCharacterState.Die:
                name = "Armature_Die";
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

        if (state == eCharacterState.Trace && Vector3.Distance(targetEntity.transform.position, this.transform.position) <= attackDistance && !isAttacking)
        {
            NowAttack();
        }

        if (isAttacking == true)
        {
            Quaternion LookRot = Quaternion.LookRotation(new Vector3(targetPosition.x, (targetPosition.y - (targetSize.y / 2)), targetPosition.z) - new Vector3(this.transform.position.x, 0, this.transform.position.z));
            transform.rotation = Quaternion.RotateTowards(transform.rotation, LookRot, 60f * Time.deltaTime);
        }
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

                int random = Random.Range(0, 10);

                switch (random)
                {
                    case 3:
                    case 4:
                        // 출혈
                        StartCoroutine(Bleeding(targetEntity));
                        break;
                    default:
                        break;
                }
                other.GetComponent<LivingEntity>().Damaged(damage, hitPoint, hitNormal);

            }
            else
                return;
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
        isTrace = true;
        enemyAnimator.SetBool("IsTrace", isTrace);
    }
    /// <summary>
    /// 추적함수
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
    /// 공격함수
    /// </summary>
    void NowAttack()
    {
        isAttacking = true;

        state = eCharacterState.Attack;

        pathFinder.isStopped = true;
        pathFinder.speed = 0f;
        enemyAnimator.SetTrigger("IsAttack");
        float attacktime = 0.4f;
        StartCoroutine(StartAttacking(attacktime));
        attacktime = 0.5f;
        StartCoroutine(NowAttacking(attacktime));
        float attackdelayTime = MoveDuration(eCharacterState.Attack);
        StartCoroutine(EndAttacking(attackdelayTime));

        Debug.Log(MoveDuration(eCharacterState.Attack));
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
                //Debug.Log($"Position {new Vector3(targetPosition.x, (targetPosition.y - (targetSize.y / 2)), targetPosition.z)}");
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

                //if (colliders[0].gameObject.layer == LayerMask.NameToLayer("DEFENSIVEGOODS"))
                //{
                //    if (colliders[0].gameObject.CompareTag("FENCE"))
                //    {
                //        targetEntity = colliders[0].gameObject;
                //    }
                //}
                //else
                //    targetEntity = colliders[0].gameObject;
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

    IEnumerator Bleeding(GameObject target)
    {
        PlayerCtrl _player = target.GetComponent<PlayerCtrl>();

        float damageTime = 0.5f;
        for (float i = 0; i < damageTime * 5; i += damageTime)
        {
            _player.Damaged(2f, Vector3.zero, Vector3.zero);
            yield return new WaitForSeconds(damageTime);
        }

    }

    IEnumerator StartAttacking(float _delaytime)
    {
        yield return new WaitForSeconds(_delaytime); ;
        pathFinder.enabled = false;
    }

    IEnumerator NowAttacking(float _delaytime)
    {
        yield return new WaitForSeconds(_delaytime);
        ClearList();
    }

    IEnumerator EndAttacking(float _delaytime)
    {
        yield return new WaitForSeconds(_delaytime * 0.8f);
        isAttacking = false;
        pathFinder.enabled = true;
        NowTrace();
    }
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
        Debug.Log(MoveDuration(eCharacterState.Die));

        StartCoroutine(WaitForDieAnimation(MoveDuration(eCharacterState.Die)));
    }
}
