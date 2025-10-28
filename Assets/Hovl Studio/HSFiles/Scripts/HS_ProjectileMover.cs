using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HS_ProjectileMover : MonoBehaviour
{
    [SerializeField] protected float speed = 15f;
    [SerializeField] protected float hitOffset = 0f;
    [SerializeField] protected bool UseFirePointRotation;
    [SerializeField] protected Vector3 rotationOffset = new Vector3(0, 0, 0);
    [SerializeField] protected GameObject hit;
    [SerializeField] protected ParticleSystem hitPS;
    [SerializeField] protected GameObject flash;
    [SerializeField] protected Rigidbody rb;
    [SerializeField] protected Collider col;
    [SerializeField] protected Light lightSourse;
    [SerializeField] protected GameObject[] Detached;
    [SerializeField] protected ParticleSystem projectilePS;
    private bool startChecker = false;
    [SerializeField]protected bool notDestroy = false;

    protected virtual void Start()
    {
        if (!startChecker)
        {
            /*lightSourse = GetComponent<Light>();
            rb = GetComponent<Rigidbody>();
            col = GetComponent<Collider>();
            if (hit != null)
                hitPS = hit.GetComponent<ParticleSystem>();*/
            if (flash != null)
            {
                flash.transform.parent = null;
            }
        }
        startChecker = true;
    }

    protected virtual IEnumerator DisableTimer(float time)
    {
        yield return new WaitForSeconds(time);
        if(gameObject.activeSelf)
            gameObject.SetActive(false);
        yield break;
    }

    protected virtual void OnEnable()
    {
        if (startChecker)
        {
            if (flash != null)
            {
                flash.transform.parent = null;
            }
            if (lightSourse != null)
                lightSourse.enabled = true;
            col.enabled = true;
            rb.constraints = RigidbodyConstraints.None;
        }
    }

    protected virtual void FixedUpdate()
    {
        if (speed != 0)
        {
            rb.linearVelocity = transform.forward * speed;      
        }
    }

    //https ://docs.unity3d.com/ScriptReference/Rigidbody.OnCollisionEnter.html
    protected virtual void OnCollisionEnter(Collision collision)
    {
        //Lock all axes movement and rotation
        rb.constraints = RigidbodyConstraints.FreezeAll;
        //speed = 0;
        if (lightSourse != null)
            lightSourse.enabled = false;
        col.enabled = false;
        if (projectilePS)
        {
            projectilePS.Stop();
            projectilePS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        ContactPoint contact = collision.contacts[0];
        Quaternion rot = Quaternion.FromToRotation(Vector3.up, contact.normal);
        Vector3 pos = contact.point + contact.normal * hitOffset;

        //Spawn hit effect on collision
        if (hit != null)
        {
            hit.transform.rotation = rot;
            hit.transform.position = pos;
            if (UseFirePointRotation) { hit.transform.rotation = gameObject.transform.rotation * Quaternion.Euler(0, 180f, 0); }
            else if (rotationOffset != Vector3.zero) { hit.transform.rotation = Quaternion.Euler(rotationOffset); }
            else { hit.transform.LookAt(contact.point + contact.normal); }
            hitPS.Play();
        }

        //Removing trail from the projectile on cillision enter or smooth removing. Detached elements must have "AutoDestroying script"
        foreach (var detachedPrefab in Detached)
        {
            if (detachedPrefab != null)
            {
                ParticleSystem detachedPS = detachedPrefab.GetComponent<ParticleSystem>();
                detachedPS.Stop();
            }
        }
        if (notDestroy)
            StartCoroutine(DisableTimer(hitPS.main.duration));
        else
        {
            if (hitPS != null)
            {
                Destroy(gameObject, hitPS.main.duration);
            }
            else
                Destroy(gameObject, 1);
        }
    }

    // HS_ProjectileMover.cs 내부에 추가

    /// <summary>
    /// 풀링 사용 여부를 설정합니다. (true면 Destroy 대신 DisableTimer 사용)
    /// </summary>
    public void SetDestroyMode(bool usePooling)
    {
        notDestroy = usePooling;
    }

    /// <summary>
    /// 투사체의 움직임과 시각 효과를 즉시 중지합니다. (풀 반납 시 호출됨)
    /// </summary>
    public void StopProjectile()
    {
        // 이동 멈춤
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }
        // 메인 파티클 멈춤 (즉시 제거)
        if (projectilePS != null) projectilePS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        // 콜라이더 비활성화
        if (col != null) col.enabled = false;
        // 라이트 끄기
        if (lightSourse != null) lightSourse.enabled = false;

        // 분리될 파티클 처리 (부모 해제하여 잔상 남기기)
        foreach (var detachedGO in Detached)
        {
            if (detachedGO != null)
            {
                detachedGO.transform.parent = null; // 부모 연결 해제
                ParticleSystem detachedPS = detachedGO.GetComponent<ParticleSystem>();
                if (detachedPS != null)
                {
                    // 자동으로 사라지도록 설정되어 있다고 가정
                    // 만약 자동으로 사라지지 않는다면, 별도의 AutoDestroy 스크립트 필요
                    if (!detachedPS.main.stopAction.HasFlag(ParticleSystemStopAction.Destroy))
                    {
                        // Destroy(detachedGO, detachedPS.main.duration + detachedPS.main.startLifetime.constantMax);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 충돌 지점에 충돌 이펙트(hitPrefab)만 생성하고 재생합니다.
    /// </summary>
    public void TriggerHitEffect(Collider other) // Collision 대신 Collider 사용
    {
        if (hit != null) // hit GameObject 대신 hitPrefab 사용
        {
            Vector3 collisionPoint = other.ClosestPoint(transform.position);
            Quaternion rot = Quaternion.LookRotation(transform.position - other.transform.position); // 충돌 방향 기반 회전
            Vector3 pos = collisionPoint + (transform.position - other.transform.position).normalized * hitOffset; // 오프셋 적용

            // Object Pooler 사용 권장
            GameObject hitInstance = Instantiate(hit, pos, rot); // 히트 이펙트 생성

            // 회전 옵션 적용 (기존 로직과 유사하게)
            if (UseFirePointRotation) { hitInstance.transform.rotation = transform.rotation * Quaternion.Euler(0, 180f, 0); }
            else if (rotationOffset != Vector3.zero) { hitInstance.transform.rotation = Quaternion.Euler(rotationOffset); }
            // else { hitInstance.transform.LookAt(...); } // LookAt은 파티클 방향에 문제 일으킬 수 있음

            // 생성된 이펙트의 파티클 시스템을 가져와 재생 (선택 사항, 보통 PlayOnAwake 사용)
            // ParticleSystem hitInstancePS = hitInstance.GetComponent<ParticleSystem>();
            // if (hitInstancePS != null) hitInstancePS.Play();
        }
    }
}