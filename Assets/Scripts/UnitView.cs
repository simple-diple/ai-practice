using System;
using UnityEngine;
using UnityEngine.AI;

public class UnitView : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Renderer rend;
    [SerializeField] private Transform hp;

    private UnitModel _unitModel;
    private const float _MAX_HP_SCALE = 0.5f;

    public void Connect(UnitModel unitModel)
    {
        _unitModel = unitModel;
        rend.material.color = PlayerColor.Get(_unitModel.Owner);
        _unitModel.OnSetTarget += OnUnitModelSetTarget;
    }

    private void OnUnitModelSetTarget(Vector3 target)
    {
        navMeshAgent.SetDestination(target);
    }

    void Update()
    {
        _unitModel.SetPosition(transform.position);
        _unitModel.IsTargetReached = navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;
        _unitModel.Tick(Time.deltaTime);
        var localScale = hp.localScale;
        localScale = new Vector3(_MAX_HP_SCALE * _unitModel.HealthNormalized, localScale.y, localScale.z);
        hp.localScale = localScale;
    }

    public void Dispose()
    {
        Destroy(gameObject);
    }
}
