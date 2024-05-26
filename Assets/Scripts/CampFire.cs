using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampFire : MonoBehaviour
{
    public int damage;
    public float damageRate;

    // 
    private List<IDamagable> things = new List<IDamagable>();
    private void Start()
    {
        InvokeRepeating(nameof(DealDamage), 0 , damageRate);
    }

    private void DealDamage() // 데미지를 줌
    {
        for (int i = 0; i < things.Count; i++)
        {
            things[i].TakePhysicalDamage(damage);
        }    
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IDamagable damagable))
        {
            things.Add(damagable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent(out IDamagable damagable))
        {
            things.Remove(damagable);
        }
    }
}
