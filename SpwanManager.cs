using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpwanManager : MonoBehaviour
{
    private static SpwanManager instance = null;
    public static SpwanManager Instance { get { return instance; } }

    [SerializeField]
    List<Transform> enemyPoints = new List<Transform>();

    // 현재 소환된 수 카운트 용
    public List<LivingEntity> enemies = new List<LivingEntity>();
    public int totalCount;
    private float waitNextStageTime = 30f;

    private IEnumerator _coEnemySpawn;

    bool skipWaitTime = false;

    #region UI 영역
    [SerializeField]
    UnityEngine.UI.Text remainEnemiesText;
    [SerializeField]
    UnityEngine.UI.Text stageText;
    [SerializeField]
    UnityEngine.UI.Text nextStageTimeText;

    #endregion

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }


    private void Start()
    {
        enemyPoints.AddRange(GameObject.Find("EnemySpawnPoints").GetComponentsInChildren<Transform>());

        StageTextSetting();

        StartCoroutine(WaitNextStage());
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            skipWaitTime = true;
        }
        else if (GameManager.instance.useCheat && Input.GetKeyDown(KeyCode.Keypad1))
        {
            DebugEnemySpawn(Monster.Zombie);
        }
        else if (GameManager.instance.useCheat && Input.GetKeyDown(KeyCode.Keypad2))
        {
            DebugEnemySpawn(Monster.Spider);
        }
        else if (GameManager.instance.useCheat && Input.GetKeyDown(KeyCode.Keypad3))
        {
            DebugEnemySpawn(Monster.Clutch);
        }
        else if (GameManager.instance.useCheat && Input.GetKeyDown(KeyCode.Keypad4))
        {
            DebugEnemySpawn(Monster.Movidic);
        }
        else if (GameManager.instance.useCheat)
        {
            remainEnemiesText.text = string.Format($"{(enemies.Count).ToString()}");
        }
    }

    private IEnumerator WaitNextStage()
    {
        // 다음 스테이지로 넘어갈 때만 다음 스테이지까지 몇 분이 남았는지 표시해주면 된다.
        nextStageTimeText.gameObject.SetActive(true);
        //Debug.Log("___ NEXT Stage ____");
        float _time = waitNextStageTime;

        while (_time > 0)
        {
            nextStageTimeText.text = string.Format($"{_time:N1}");
            yield return new WaitForFixedUpdate();
            _time -= Time.fixedDeltaTime;
            if (skipWaitTime)
            {
                skipWaitTime = false;
                break;
            }
            if (GameManager.instance.useCheat)
            {
                yield break;
            }

        }
        _coEnemySpawn = EnemySpawn();
        nextStageTimeText.gameObject.SetActive(false);
        // 다음 스테이지가 시작되고 그 스테이지의 적들이 생성되면 된다.
        StartCoroutine(_coEnemySpawn);

    }


    public void EnemySpawnFunc()
    {
        if (_coEnemySpawn != null)
        {
            StopCoroutine(_coEnemySpawn);
            _coEnemySpawn = null;
        }
        else
        {
            _coEnemySpawn = EnemySpawn();
            StartCoroutine(_coEnemySpawn);
        }
    }
    /// <summary>
    /// 적 스폰 코루틴 함수 -작성자 : 신상엽-
    /// </summary>
    /// <returns></returns>
    IEnumerator EnemySpawn()
    {

        // 어떤걸 몇 마리 소환하는지 받아옴
        List<int> enemyCount = ObjectCounting.SpwanCounting(GameManager.instance._stage);
        int zombieCount = enemyCount[0];
        int spiderCount = enemyCount[1];
        int clutchCount = enemyCount[2];
        int movidicCount = enemyCount[3];

        totalCount = zombieCount + spiderCount + clutchCount + movidicCount;
        float spawnTime = 0.5f;

        yield return new WaitForSeconds(3f);
        while (true)
        {

            int idx = UnityEngine.Random.Range(2, 8);
            idx += (UnityEngine.Random.Range(0, 3) * 11);

            List<int> _list = new List<int>();
            if (enemyCount[0] > 0)
            {
                _list.Add(0);
            }
            if (enemyCount[1] > 0)
            {
                _list.Add(1);
            }
            if (enemyCount[2] > 0)
            {
                _list.Add(2);
            }
            if (enemyCount[3] > 0)
            {
                _list.Add(3);
            }


            if (enemies.Count < 20 && enemies.Count < totalCount)
            {
                float randomSpeed = UnityEngine.Random.Range(-0.2f, 0.2f);


                int selectEnemy = _list[UnityEngine.Random.Range(0, _list.Count)];
                Monster mob = (Monster)selectEnemy;

                GameObject obj;
                switch (mob)
                {
                    case Monster.Zombie:
                        obj = ObjectPooling.GetObject(Monster.Zombie);
                        obj.transform.position = enemyPoints[idx].position;
                        obj.SetActive(true);
                        obj.GetComponent<UnityEngine.AI.NavMeshAgent>().speed += randomSpeed;

                        break;
                    case Monster.Spider:
                        obj = ObjectPooling.GetObject(Monster.Spider);
                        obj.transform.position = enemyPoints[idx].position;
                        obj.SetActive(true);
                        obj.GetComponent<UnityEngine.AI.NavMeshAgent>().speed += randomSpeed;

                        break;
                    case Monster.Clutch:
                        obj = ObjectPooling.GetObject(Monster.Clutch);
                        obj.transform.position = enemyPoints[idx].position;
                        obj.SetActive(true);
                        obj.GetComponent<UnityEngine.AI.NavMeshAgent>().speed += randomSpeed;

                        break;
                    case Monster.Movidic:
                        obj = ObjectPooling.GetObject(Monster.Movidic);
                        obj.transform.position = enemyPoints[idx].position;
                        obj.SetActive(true);
                        obj.GetComponent<UnityEngine.AI.NavMeshAgent>().speed += randomSpeed;

                        break;
                    default:
                        obj = null;
                        break;
                }

                // 소환한 것 count 감소
                enemyCount[selectEnemy]--;

                if (obj != null)
                {
                    enemies.Add(obj.GetComponent<LivingEntity>());
                }

            }

            //Debug.Log("____ ENEMY COUNT: " + enemies.Count + " ____");

            yield return new WaitForSeconds(spawnTime);

            // int형
            remainEnemiesText.text = totalCount.ToString();

            if (totalCount <= 0)
            {
                // 스테이지 클리어했다는 변수 하나 해가지고
                // 클리어 하면 다음 스테이지까지 여유시간 좀 주고
                // 여유시간 끝나면 바로 다음 스테이지 시작(코루틴 돌리고)
                GameManager.instance._stage++;
                GameManager.instance.StageClear();
                StageTextSetting();
                break;
            }
        }

        if (GameManager.instance._stage > GameManager.instance.maxStage)
        {
            yield break;
        }

        StartCoroutine(WaitNextStage());
        _coEnemySpawn = null;

    }


    private void StageTextSetting()
    {
        // int 형
        stageText.text = string.Format($"<size=65>Stage</size>\n{GameManager.instance._stage.ToString()}");

        // 특전 획득에 성공했는지 확인하는 함수
        AchievementManager.CheckAchievement_Perk();
    }


    private void DebugEnemySpawn(Monster _monster)
    {
        int idx = UnityEngine.Random.Range(2, 8);
        idx += (UnityEngine.Random.Range(0, 3) * 7);


        if (enemies.Count < 30)
        {
            float randomSpeed = UnityEngine.Random.Range(-0.2f, 0.2f);

            //int selectEnemy = UnityEngine.Random.Range(0, 4);
            int selectEnemy = (int)_monster;
            Monster mob = (Monster)selectEnemy;

            GameObject obj;

            switch (_monster)
            {
                case Monster.Zombie:
                    obj = ObjectPooling.GetObject(Monster.Zombie);
                    obj.transform.position = enemyPoints[idx].position;
                    obj.SetActive(true);
                    obj.GetComponent<UnityEngine.AI.NavMeshAgent>().speed += randomSpeed;

                    break;
                case Monster.Spider:
                    obj = ObjectPooling.GetObject(Monster.Spider);
                    obj.transform.position = enemyPoints[idx].position;
                    obj.SetActive(true);
                    obj.GetComponent<UnityEngine.AI.NavMeshAgent>().speed += randomSpeed;

                    break;
                case Monster.Clutch:
                    obj = ObjectPooling.GetObject(Monster.Clutch);
                    obj.transform.position = enemyPoints[idx].position;
                    obj.SetActive(true);
                    obj.GetComponent<UnityEngine.AI.NavMeshAgent>().speed += randomSpeed;

                    break;
                case Monster.Movidic:
                    obj = ObjectPooling.GetObject(Monster.Movidic);
                    obj.transform.position = enemyPoints[idx].position;
                    obj.SetActive(true);
                    obj.GetComponent<UnityEngine.AI.NavMeshAgent>().speed += randomSpeed;

                    break;
                default:

                    obj = null;
                    break;
            }
            if (obj != null)
            {
                enemies.Add(obj.GetComponent<LivingEntity>());
            }

        }

    }

}

