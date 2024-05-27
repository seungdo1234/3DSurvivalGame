using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
   Equipable,
   Consumable,
   Resource
}

public enum ConsumableType
{
   Health,
   Hunger
}

[System.Serializable]
public class ItemDataConsumable // 소비 아이템일 때 어떤 걸 회복 시켜줄 것 인지
{
   public ConsumableType type;
   public float value;
}
[CreateAssetMenu(fileName = "Item", menuName = "New Item")]
public class ItemData : ScriptableObject
{
   [Header("# Info")] 
   public string displayName; // 아이템 이름
   public string description; // 아이템 설명
   public ItemType type; //아이템 타입
   public Sprite icon;
   public GameObject dropPrefab;

   [Header("# Stacking")] // 여러개를 가질 수 있음
   public bool canStack; // 여러개를 가질 수 있는지
   public int maxStackAmount; // 얼마나 가질 수 있는지

   [Header("# Stacking")] 
   public ItemDataConsumable[] consumables;
   
   
   [Header("# Equip")] 
   public GameObject equipPrefab;
}
