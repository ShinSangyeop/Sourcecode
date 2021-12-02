using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Monster
{
    Zombie,
    Spider,
    Clutch,
    Movidic,
}

public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling Instance;

    [SerializeField]
    private GameObject zombiePrefab;
    [SerializeField]
    private GameObject spiderPrefab;
    [SerializeField]
    private GameObject clutchPrefab;
    [SerializeField]
    private GameObject movidicPrefab;

    private Queue<GameObject> poolingObjectZombie = new Queue<GameObject>();
    private Queue<GameObject> poolingObjectSpider = new Queue<GameObject>();
    private Queue<GameObject> poolingObjectClutch = new Queue<GameObject>();
    private Queue<GameObject> poolingObjectMovidic = new Queue<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        CreatePool(zombiePrefab, 40);
        CreatePool(spiderPrefab, 20);
        CreatePool(clutchPrefab, 10);
        CreatePool(movidicPrefab, 1);
    }
    /// <summary>
    /// 새로운 오브젝트를 생성해주는 함수
    /// </summary>
    /// <returns></returns>
    private ZombieSC CreateNewZombie()
    {
        var newObj = Instantiate(zombiePrefab, transform).GetComponent<ZombieSC>();
        newObj.gameObject.SetActive(false);
        return newObj;
    }

    void CreatePool(GameObject _obj, int _count = 1)
    {
        for (int i = 0; i < _count; i++)
        {
            var obj = Instantiate(_obj, transform);
            obj.SetActive(false);
            obj.name = _obj.name + $"_{i}";

            if (obj.name.Contains("Zombie"))
            {
                poolingObjectZombie.Enqueue(obj);
            }
            else if (obj.name.Contains("Spider"))
            {
                poolingObjectSpider.Enqueue(obj);
            }
            else if (obj.name.Contains("Clutch"))
            {
                poolingObjectClutch.Enqueue(obj);
            }
            else if (obj.name.Contains("Movidic"))
            {
                poolingObjectMovidic.Enqueue(obj);
            }
        }
    }
    /// <summary>
    /// 풀에서 오브젝트를 빌려가는 함수
    /// </summary>
    /// <returns></returns>
    public static GameObject GetObject(Monster _monster)
    {
        GameObject obj;

        switch (_monster)
        {
            case Monster.Zombie:

                if (Instance.poolingObjectZombie.Count > 0)
                {
                    obj = Instance.poolingObjectZombie.Dequeue();
                    return obj;
                }
                else
                {
                    Instance.CreatePool(Instance.zombiePrefab);
                    obj = Instance.poolingObjectZombie.Dequeue();
                    //obj.SetActive(true);
                    return obj;
                }
            case Monster.Spider:
                if (Instance.poolingObjectSpider.Count > 0)
                {
                    obj = Instance.poolingObjectSpider.Dequeue();
                    //obj.SetActive(true);
                    return obj;
                }
                else
                {
                    Instance.CreatePool(Instance.spiderPrefab);
                    obj = Instance.poolingObjectSpider.Dequeue();
                    //obj.SetActive(true);
                    return obj;
                }
            case Monster.Clutch:
                if (Instance.poolingObjectClutch.Count > 0)
                {
                    obj = Instance.poolingObjectClutch.Dequeue();
                    //obj.SetActive(true);
                    return obj;
                }
                else
                {
                    Instance.CreatePool(Instance.clutchPrefab);
                    obj = Instance.poolingObjectClutch.Dequeue();
                    //obj.SetActive(true);
                    return obj;
                }
            case Monster.Movidic:
                if (Instance.poolingObjectMovidic.Count > 0)
                {
                    obj = Instance.poolingObjectMovidic.Dequeue();
                    //obj.SetActive(true);
                    return obj;
                }
                else
                {
                    Instance.CreatePool(Instance.movidicPrefab);
                    obj = Instance.poolingObjectMovidic.Dequeue();
                    //obj.SetActive(true);
                    return obj;
                }
            default:
                print("버그발생");
                return null;
        }
    }
    /// <summary>
    /// 오브젝트를 다시 돌려주는 함수
    /// </summary>
    /// <param name="zombieSC"></param>
    public static void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(Instance.transform);

        if (obj.name.Contains("Zombie"))
        {
            Instance.poolingObjectZombie.Enqueue(obj);
        }
        else if (obj.name.Contains("Spider"))
        {
            Instance.poolingObjectSpider.Enqueue(obj);
        }
        else if (obj.name.Contains("Clutch"))
        {
            Instance.poolingObjectClutch.Enqueue(obj);
        }
        else if (obj.name.Contains("Movidic"))
        {
            Instance.poolingObjectMovidic.Enqueue(obj);
        }
    }
}

