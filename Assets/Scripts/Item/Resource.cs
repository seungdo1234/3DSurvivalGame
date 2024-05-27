using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Resource : MonoBehaviour
{
    public ItemData itemToGive;
    public int quantityPerHit; // 한번에 등장하는 아이템 갯수
    public int capacy;

    public void Gather(Vector3 hitPoint, Vector3 hitNormal)
    {
        for (int i = 0; i < quantityPerHit; i++)
        {
            if (capacy <= 0) break; // 자원을 다 캤다면
            capacy -= 1;
            Instantiate(itemToGive.dropPrefab, hitPoint + Vector3.up, Quaternion.LookRotation(hitNormal, Vector3.right));
        }
    }
}
