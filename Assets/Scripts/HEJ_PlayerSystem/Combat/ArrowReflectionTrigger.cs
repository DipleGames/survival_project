using System;
using UnityEngine;

// 주변 화살만 감지하며, 거리·각도 검사와 가장 가까운 화살 선택은 MeleeHitDetector에서 처리합니다.
[RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
public class ArrowReflectionTrigger : MonoBehaviour
{
    private SphereCollider _area;
    private LayerMask _arrowLayers;
    private Action _onArrowNearby;
    private float _lastCheckTime = float.NegativeInfinity;

    private void Awake()
    {
        _area = GetComponent<SphereCollider>();
        _area.isTrigger = true;
        _area.enabled = false;

        Rigidbody body = GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
    }

    public void Open(float worldRadius, LayerMask arrowLayers, Action onArrowNearby)
    {
        Vector3 scale = transform.lossyScale;
        float largestScale = Mathf.Max(Mathf.Abs(scale.x), Mathf.Abs(scale.y), Mathf.Abs(scale.z));
        _area.radius = worldRadius / Mathf.Max(largestScale, 0.0001f);
        _arrowLayers = arrowLayers;
        _onArrowNearby = onArrowNearby;
        _lastCheckTime = float.NegativeInfinity;
        _area.enabled = true;
    }

    public void Close()
    {
        _onArrowNearby = null;
        if (_area != null) _area.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        NotifyArrow(other);
    }

    private void OnTriggerStay(Collider other)
    {
        // 구형 감지 영역에 들어온 화살이 나중에 부채꼴 공격 범위로 진입하는 경우도 검사합니다.
        NotifyArrow(other);
    }

    private void NotifyArrow(Collider other)
    {
        if (_onArrowNearby == null || (_arrowLayers.value & (1 << other.gameObject.layer)) == 0) return;
        if (other.GetComponentInParent<ArcherArrow>() == null) return;
        // 여러 화살이 겹쳐 있어도 물리 주기마다 가장 가까운 화살을 한 번만 검색합니다.
        if (_lastCheckTime == Time.fixedTime) return;
        _lastCheckTime = Time.fixedTime;
        _onArrowNearby.Invoke();
    }
}
