using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stop : MonoBehaviour
{
    public Rigidbody rigid;
    public MovidicSC movidic;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BUNKERDOOR") || other.CompareTag("FENCE"))
        {
            rigid.velocity = Vector3.zero;
            // 러쉬 중일때 돌진 불 값 체크
            movidic.isRush = false;
            transform.parent.GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;

        }
    }
}
