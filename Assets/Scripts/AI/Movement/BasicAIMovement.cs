using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
public class BasicAIMovement : NetworkBehaviour
{

    [SerializeField] protected CharacterController controller;
    [SerializeField] protected Transform target;

    [SerializeField] protected FieldOfView fov;
    [SerializeField] protected NavMeshAgent agent;
    public float movementRadius;
    [SerializeField] protected Entity entity;



    // Start is called before the first frame update
    protected void Start()
    {
        if (!entity)
            entity = GetComponent<Entity>();
        if (!fov)
            fov = GetComponent<FieldOfView>();
        
        if (!agent)
            agent = GetComponent<NavMeshAgent>();

        fov.OnTargetFound += Fov_OnTargetFound;
        movementRadius = fov.viewRadius;
    }


    //When target found change transform;
    private void Fov_OnTargetFound()
    {
        target = fov.targetTransform;
    }

    // Update is called once per frame
    protected void Update()
    {
        if (!IsOwner)
            return;

            if (!target || target == null)
                idleMovement();
            else
            {
                agent.SetDestination(target.position);
            }
    }

    protected bool pathComplete()
    {
        if (Vector3.Distance(agent.destination, agent.transform.position) <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                return true;
            }
        }

        return false;
    }

    public void idleMovement() 
    {
        if (!IsOwner)
            return;

        if(!fov.getSearching())
            fov.startSearching();

        if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
        {
            //Debug.Log("I'm movin");
            agent.SetDestination(entity.GetRandomPoint(transform, movementRadius));
        }
        else 
        {
            //Debug.Log(agent.steeringTarget);
        }
    }

    //Predator Event overides everything
#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, movementRadius);
    }

#endif

}
