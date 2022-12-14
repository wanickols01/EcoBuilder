using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Netcode;
public class Entity : NetworkBehaviour
{

    [SerializeField] public EntityHolder holder;
    [SerializeField] public int currHunger;
    [SerializeField] public string baseName;
    [SerializeField] public GameObject body;

    private bool searching;

    private void Start()
    {
        Init();
    }

    public event Action<string> OnEntityDied;

    //Healthy Enough for Reproduction
    public bool isHealthy
    {
        get
        {
            return (!holder.profile.consumer || currHunger < holder.profile.maxHungerAllowed);
        }
    }

    public void Init() 
    {

        searching = false;

        holder.system.OnTick += OnTickEvent;
        OnEntityDied += holder.system.OnEntityDeathListener;

        name = holder.profile.name + holder.createdCounter;
        
        holder.createdCounter++;
        holder.currCounter++;

        if(holder.profile.consumer)
            setName();

        if (!IsSpawned)
            GetComponent<NetworkObject>().SpawnWithOwnership(OwnerClientId);
        body.transform.SetParent(transform);
        transform.SetParent(holder.transform);
    }

    void setName() 
    {
        name = baseName + " (" + currHunger + '/' + holder.profile.maxHungerBeforeDeath + ')';
    }

 

    protected void checkForMultiply() 
    {
        if (isHealthy) 
        {
            body.SendMessage("checkMultiply", holder.profile);

            if (this.holder.profile.consumer)
            {
                body.SendMessage("startSearching");
                searching = true;
                //notify other entity
            }

        }    
    }

    protected void checkForFood() 
    {
        if (currHunger >= holder.profile.hungerThreshold) 
        {
            if (!searching)
            {
                body.SendMessage("startSearching");
                searching = true;
            }
        }
    }

    private void OnTickEvent() 
    {
        if (!IsOwner || !body)
            return;

        checkForMultiply();
        //Debug.Log("Tick from" + name);
        if (holder.profile.objectType == ObjectType.Plant)
            return;

            currHunger += holder.profile.hungerAccumulationVal;

            if (currHunger >= holder.profile.maxHungerBeforeDeath)
            {
                onDestroyServerRpc();
            }
            //Add running from predator
            else
            {
                checkForFood();
            }
            setName(); //resets name with huuger
       
        
    }

    public void Fov_OnMultiply()
    {
        //Fix me, change spawn location to near parents
        holder.system.createEntity(holder);
    }


    public void Eat(int nutritionalValue)
    {
        
        currHunger -= nutritionalValue;
        
        if (currHunger < 0)
            currHunger = 0;

        searching = false;
    }

    public Vector3 GetRandomPoint(Transform point = null, float radius = 0)
    {
        return holder.system.points.GetRandomPoint(point, radius);
    }

    [ServerRpc]
    public void onDestroyServerRpc()
    {
        holder.system.OnTick -= OnTickEvent; //UnSubZ
        --holder.currCounter;
        OnEntityDied?.Invoke(name);

        if(IsSpawned)
            GetComponent<NetworkObject>().Despawn();
        Destroy(gameObject);
    }
}
