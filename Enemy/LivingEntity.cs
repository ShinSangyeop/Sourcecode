using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InterfaceSet;
//using System;
public class LivingEntity : MonoBehaviour, IAttack, IDamaged
{
    public enum eCharacterState
    {
        Trace,
        Attack,
        Die
    }

    public float startHp = 100f; // 시작 체력
    public float currHP;
    public float armour;
    public float damage;
    public bool down;
    public bool dead;
    public eCharacterState state;

    public GameObject startTarget;

    protected float _exp;
    //public event Action OnDeath;
    protected virtual void OnEnable()  // 클래스가 생성될때 리셋되는 상태
    {
        down = false;
        dead = false;  // 사망상태가 아님
        currHP = startHp; // 현재 체력은 시작 체력이랑 같음
        this.GetComponent<Collider>().enabled = true;
        StartCoroutine(FindBunker());
        state = eCharacterState.Trace;

    }

    IEnumerator FindBunker()
    {
        while (GameManager.instance.bunkerDoor == null) { yield return null; }
        startTarget = GameManager.instance.bunkerDoor.gameObject;
    }

    public void Attack()  // 공격시 실행될 함수
    {

    }

    public virtual float Damaged(float damage, Vector3 hitPoint, Vector3 hitNormal) // 피헤 받을시 실행될 함수 (데미지, 피격 위치, 피격 방향)
    {
        currHP -= (damage - armour) >= 0 ? (damage - armour) : 0; // 현재체력에 데미지 만큼 감소
        if (currHP <= 0 && !dead) // 현재체력이 0보다 작고 사망 상태가 아닐떄
        {
            Down(); // DIE 함수 실행

            return _exp;
        }

        return 0;
    }

    protected virtual void Down()
    {
        if (down == false)
        {
            OnDeath(); // 사망 애니메이션 만
        }
    }

    public void Die() // 사망함수
    {
        // 사망 처리

        dead = true;  // 상태를 사망으로
        // 제거하든 disable처리하든
        if (SpwanManager.Instance.enemies.Contains(this))
        {
            SpwanManager.Instance.totalCount -= 1;
            SpwanManager.Instance.enemies.Remove(this);
        }

        //Destroy(this.gameObject, 5f);
        ObjectPooling.ReturnObject(gameObject);


    }
    public virtual void OnDeath()
    {
        // 사망 애니매이션 실행
        state = eCharacterState.Die;
        this.GetComponent<Collider>().enabled = false;
        dead = true;
    }

    protected virtual IEnumerator WaitForDieAnimation(float _animTime)
    {
        yield return new WaitForSeconds(_animTime);

        Die();
    }


}
