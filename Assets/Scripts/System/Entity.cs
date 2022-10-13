using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Entity : MonoBehaviour
{

    public EntityHolder holder;
    public int currHunger;
    public string baseName;

    public event Action<string> OnEntityDied;

    //Healthy Enough for Reproduction
    public bool isHealthy
    {
        get
        {
            return (!holder.profile.consumer || currHunger < holder.profile.maxHungerAllowed);
        }
    }

    public void Init(EntityHolder holder) 
    {
        this.holder = holder;

        holder.system.OnTick += OnTickEvent;
        OnEntityDied += holder.system.OnEntityDeathListener;

        name = holder.profile.name + holder.createdCounter;
        transform.SetParent(holder.transform);
        ++holder.createdCounter;
        ++holder.currCounter;

        if(holder.profile.consumer)
            setName();
    }

    void setName() 
    {
        name = baseName + " (" + currHunger + '/' + holder.profile.maxHungerBeforeDeath + ')';
    }

    private void Update()
    {
        checkForMultiply();
        checkForFood();
    }

    protected void checkForMultiply() 
    {
        if (isHealthy) 
        {
            //eventually check for nearby radius of bunnies, but for now just gonna base it off totals
            
        }    
    }

    protected void checkForFood() 
    {
        if (currHunger >= holder.profile.hungerThreshold) 
        {
            //Search for nearby objects
            //check found objects against foodtype
            //holder.profile.foodSource; //plant or animal (may need to adjust)

        }
    }

    private void OnTickEvent() 
    {
        //Debug.Log("Tick from" + name);
        if (holder.profile.consumer) 
        {
            currHunger += holder.profile.hungerAccumulationVal;

            if (currHunger >= holder.profile.maxHungerBeforeDeath)
            {
                Destroy(gameObject);
            }
            //Add running from predator
            else
            {
                checkForFood();
            }
            setName(); //resets name with huuger
        }
        
    }

    private void OnDestroy()
    {
        --holder.currCounter;
        OnEntityDied?.Invoke(name);
        holder.system.OnTick -= OnTickEvent; //UnSub
    }

}
