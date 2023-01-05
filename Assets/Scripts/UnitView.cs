using UnityEngine;
using UnityEngine.AI;

public class UnitView : MonoBehaviour
{
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] private Renderer rend;
    [SerializeField] private Transform hp;
    [SerializeField] private ParticleSystem damageEffect;
    [SerializeField] private ParticleSystem attackEffect;

    private UnitModel _unitModel;
    private const float _MAX_HP_SCALE = 0.5f;

    public void Connect(UnitModel unitModel)
    {
        _unitModel = unitModel;
        rend.material.color = PlayerColor.Get(_unitModel.Owner);
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

    public void GetDamageEffect()
    {
        damageEffect.Play();
    }

    public void SetTarget(Vector3 position)
    {
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(position);
    }

    public void Stop()
    {
        navMeshAgent.isStopped = true;
    }

    public void LookAt(Vector3 target)
    {
        transform.LookAt(target);
    }

    public void Attack()
    {
        attackEffect.Play();
    }
}
