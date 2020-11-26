using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentNoRootMotion : MonoBehaviour
{
    public AIWaypointNetwork WaypointNetwork = null;
    public int CurrentIndex = 0;
    public bool HasPath;
    public bool PathPending;
    public bool PathStale;
    public NavMeshPathStatus PathStatus = NavMeshPathStatus.PathInvalid;
    public AnimationCurve JumpCurve = new AnimationCurve();
    
    private NavMeshAgent _navAgent = null;
    private Animator _animator = null;
    private float _originalMaxSpeed = 0;
    
    void Start()
    {
        _navAgent = GetComponent<NavMeshAgent>();  
        _animator = GetComponent<Animator>();

        if (_navAgent) _originalMaxSpeed = _navAgent.speed;

        if (WaypointNetwork == null) return;

        SetNextDestination(false);
    }

    void SetNextDestination (bool increment)
    {
        if (!WaypointNetwork) return;

        int incStep = increment?1:0;
        Transform nextWaypointTransform = null;

        int nextWaypoint = (CurrentIndex + incStep >= WaypointNetwork.Waypoints.Count) ? 0 : CurrentIndex + incStep;
        nextWaypointTransform = WaypointNetwork.Waypoints[nextWaypoint];

        if (nextWaypointTransform != null)
        {
            CurrentIndex = nextWaypoint;
            _navAgent.destination = nextWaypointTransform.position;
            return;
        }

        CurrentIndex = nextWaypoint;
    }

    private void Update() 
    {
        int turnOnSpot = 0;

        HasPath = _navAgent.hasPath;
        PathPending = _navAgent.pathPending;
        PathStale = _navAgent.isPathStale;
        PathStatus = _navAgent.pathStatus;

        Vector3 cross = Vector3.Cross(transform.forward, _navAgent.desiredVelocity.normalized);
        float horizontal = (cross.y<0)? -cross.magnitude : cross.magnitude;
        horizontal = Mathf.Clamp(horizontal * 2.32f, -2.32f, 2.32f);

        if (_navAgent.desiredVelocity.magnitude < 1f && Vector3.Angle(transform.forward, _navAgent.desiredVelocity) > 20f)
        {
            _navAgent.speed = 0.1f;
            turnOnSpot = (int)Mathf.Sign(horizontal);
        }
        else
        {
            _navAgent.speed = _originalMaxSpeed;
            turnOnSpot = 0;
        }

        _animator.SetFloat("Horizontal", horizontal, 0.1f, Time.deltaTime);
        _animator.SetFloat("Vertical", _navAgent.desiredVelocity.magnitude, 0.1f, Time.deltaTime);
        _animator.SetInteger("TurnOnSpot", turnOnSpot);

        // if (_navAgent.isOnOffMeshLink)
        // {
        //     StartCoroutine(Jump(1.0f));
        //     return;
        // }

        if ((_navAgent.remainingDistance <= _navAgent.stoppingDistance && !PathPending) || PathStatus == NavMeshPathStatus.PathInvalid)
            SetNextDestination(true);    
        else if (_navAgent.isPathStale)
            SetNextDestination(false);
    }

    IEnumerator Jump(float duration)
    {
        OffMeshLinkData data = _navAgent.currentOffMeshLinkData;
        Vector3 startPos = _navAgent.transform.position;
        Vector3 endPos = data.endPos + (_navAgent.baseOffset * Vector3.up);
        float time = 0.0f;

        while (time <= duration)
        {
            float t = time / duration;
            _navAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + JumpCurve.Evaluate(t) * Vector3.up;
            time += Time.deltaTime;
            yield return null;
        }

        _navAgent.CompleteOffMeshLink();
    }
}
