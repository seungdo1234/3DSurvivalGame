using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipTool : Equip
{
    public float attackRate;
    private bool attacking;
    public float attackDistance;
    public float useStamina;

    [Header("# Resource Gathering")] // 리소스 채취할 수 있는지 ?
    public bool doesGatherResources;
    
    [Header("# Combat")] // 데미지를 줄 수 있는지 ?
    public bool doesDealDamage;
    public int damage;

    private Animator anim;
    private Camera camera;
    private void Awake()
    {
        anim = GetComponent<Animator>();
        camera = Camera.main;
    }

    public override void OnAttackInput()
    {
        if (!attacking)
        {
            if (CharacterManager.Instance.Player.condition.UseStamina(useStamina))
            {
                attacking = true;
                anim.SetTrigger("Attack");
                Invoke("OnCanAttack",attackRate);
            }
        }
    }

    private void OnCanAttack()
    {
        attacking = false;
    }

    public void OnHit()
    {
        Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, attackDistance))
        {
            Debug.Log(hit.collider.name);
            if (doesGatherResources && hit.collider.TryGetComponent(out Resource resource))
            {
                resource.Gather(hit.point, hit.normal);
            }
            else if (doesDealDamage && hit.collider.TryGetComponent(out IDamagable damagable))
            {
                damagable.TakePhysicalDamage(damage);
            }
            
        }
    }
}
