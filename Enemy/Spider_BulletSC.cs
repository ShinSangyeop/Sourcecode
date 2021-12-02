using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spider_BulletSC : MonoBehaviour
{
    public Vector3 dir;
    public SphereCollider sphereCollider;

    float fireSpeed = 12f;
    public float damage = 12f;

    Rigidbody rigid;

    private void Awake()
    {
        dir = transform.forward;
        rigid = GetComponent<Rigidbody>();
    }
    void Start()
    {
        rigid.AddForce(new Vector3(0, 5, 3), ForceMode.Impulse);
        Destroy(this.gameObject, 10f);
    }
    void Update()
    {
        transform.Translate(Vector3.forward * fireSpeed * Time.deltaTime, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PLAYER") || other.CompareTag("BUNKERDOOR") || other.CompareTag("FENCE"))
        {
            Debug.Log("=== PLAYER HIT ===");
            Vector3 hitPoint = other.ClosestPoint(gameObject.GetComponent<Collider>().bounds.center);
            Vector3 hitNormal = new Vector3(hitPoint.x - sphereCollider.bounds.center.x,
                                            hitPoint.y - sphereCollider.bounds.center.y,
                                            hitPoint.z - sphereCollider.bounds.center.z);

            Spider_BulletSC bullet = other.GetComponent<Spider_BulletSC>();
            other.GetComponent<LivingEntity>().Damaged(damage, hitPoint, hitNormal);

            Destroy(this.gameObject);
        }

    }
}
