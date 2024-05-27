using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

public class UIInventroy : MonoBehaviour
{
    private ItemSlot[] slots;

    public GameObject inventoryWindow;
    public Transform slotPanel;
    public Transform dropPos;
    
    [Header("# Select Item")]
    public TextMeshProUGUI selectedItemName;
    public TextMeshProUGUI selectedItemDesc;
    public TextMeshProUGUI selectedStatName;
    public TextMeshProUGUI selectedStatValue;
    public GameObject useBtn;
    public GameObject equipBtn;
    public GameObject unEquipBtn;
    public GameObject dropBtn;

    private PlayerCondition condition;
    private PlayerController controller;

    private ItemData selectedItem;
    private int selectedItemIdx;

    private int curEquipIndex;
    private void Start()
    {
        controller = CharacterManager.Instacne.Player.controller;
        condition = CharacterManager.Instacne.Player.condition;
        dropPos = CharacterManager.Instacne.Player.dropPos;
        controller.onInventory += Toggle;
        CharacterManager.Instacne.Player.adddItem += AddItem;
        
        inventoryWindow.SetActive(false);
        slots = new ItemSlot[slotPanel.childCount];

        for (int i = 0; i < slots.Length; i++) // 아이템 슬롯 초기화
        {
            slots[i] = slotPanel.GetChild(i).GetComponent<ItemSlot>();
            slots[i].index = i;
            slots[i].inventroy = this;
        }

        ClearSelectedItemWindow();
    }

    private void ClearSelectedItemWindow()
    {
        selectedItemDesc.text = String.Empty;
        selectedItemName.text = String.Empty;
        selectedStatValue.text = String.Empty;
        selectedStatName.text = String.Empty;
        
        useBtn.SetActive(false);
        equipBtn.SetActive(false);
        unEquipBtn.SetActive(false);
        dropBtn.SetActive(false);
    }

    public void Toggle()
    {
        if (IsOpen())
        {
            inventoryWindow.SetActive(false);
        }
        else
        {
            inventoryWindow.SetActive(true);
        }
    }

    public bool IsOpen()
    {
        return inventoryWindow.activeInHierarchy;
    }

    private void AddItem()
    {
        ItemData data = CharacterManager.Instacne.Player.itemData;
        
        // 아이템이 중복 가능한지 검사
        if (data.canStack)
        {
            ItemSlot slot = GetItemStack(data);
            if (slot != null)
            {
                slot.quantity++;
                UpdateUI();
                CharacterManager.Instacne.Player.itemData = null;
                return;
            }
        }
        
        // 비어있는 슬롯을 가져온다.
        ItemSlot emptySlot = GetEmptySlot();
        
        // 있다면
        if (emptySlot != null)
        {
            emptySlot.item = data;
            emptySlot.quantity = 1;
            UpdateUI();
            CharacterManager.Instacne.Player.itemData = null;
            return;
        }
        
        // 없다면
        ThrowItem(data);
        CharacterManager.Instacne.Player.itemData = null;
    }

    private void UpdateUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item != null)
            {
                slots[i].Set();
            }
            else
            {
                slots[i].Clear();
            }
        }
    }
    private ItemSlot GetItemStack(ItemData data)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            // 같은 아이템을 찾고 최대 갯수보다 적게 보유하고있다면
            if (slots[i].item == data && slots[i].quantity < data.maxStackAmount)
            {
                return slots[i];
            }
        }
        return null;
    }    
    
    private ItemSlot GetEmptySlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            // 같은 아이템을 찾고 최대 갯수보다 적게 보유하고있다면
            if (slots[i].item == null)
            {
                return slots[i];
            }
        }
        return null;
    }

    private void ThrowItem(ItemData data) // 아이템 버리기
    {
        Instantiate(data.dropPrefab, dropPos.position, Quaternion.Euler(Vector3.one * Random.value * 360));
    }

    public void SelectItem(int index)
    {
        if (slots[index].item ==null) return;

        selectedItem = slots[index].item;
        selectedItemIdx = index;

        selectedItemName.text = selectedItem.displayName;
        selectedItemDesc.text = selectedItem.description;

        selectedStatName.text = string.Empty;
        selectedStatValue.text = string.Empty;

        for (int i = 0; i < selectedItem.consumables.Length; i++)
        {
            selectedStatName.text += selectedItem.consumables[i].type.ToString() + "\n";
            selectedStatValue.text += selectedItem.consumables[i].value.ToString() + "\n";
        }
        
        useBtn.SetActive(selectedItem.type == ItemType.Consumable);
        equipBtn.SetActive(selectedItem.type == ItemType.Equipable && !slots[index].equipped);
        unEquipBtn.SetActive(selectedItem.type == ItemType.Equipable && slots[index].equipped);
        dropBtn.SetActive(true);
    }

    public void OnUseButton()
    {
        if (selectedItem.type == ItemType.Consumable)
        {
            for (int i = 0; i < selectedItem.consumables.Length; i++)
            {
                switch (selectedItem.consumables[i].type)
                {
                    case ConsumableType.Health:
                        condition.Heal(selectedItem.consumables[i].value);
                        break;
                    case ConsumableType.Hunger:
                        condition.Eat(selectedItem.consumables[i].value);
                        break;
                }
            }
            RemoveSelectedItem();
        }
    }
    public void OnDropButton()
    {
        ThrowItem(selectedItem);
        RemoveSelectedItem();
    }

    private void RemoveSelectedItem()
    {
        slots[selectedItemIdx].quantity--;

        if (slots[selectedItemIdx].quantity <= 0)
        {
            selectedItem = null;
            slots[selectedItemIdx].item = null;
            selectedItemIdx = -1;
            ClearSelectedItemWindow();
        }
        
        UpdateUI();
    }

    public void OnEquipButton()
    {
        if (slots[curEquipIndex].equipped)
        {
            UnEquip(curEquipIndex);
        }

        slots[selectedItemIdx].equipped = true;
        curEquipIndex = selectedItemIdx;
        CharacterManager.Instacne.Player.equip.EquipNew(selectedItem);
        UpdateUI();
        
        SelectItem(selectedItemIdx);
    }

    private void UnEquip(int index)
    {
        slots[index].equipped = false;
        CharacterManager.Instacne.Player.equip.UnEquip();
        UpdateUI();

        if (selectedItemIdx == index)
        {
            SelectItem(selectedItemIdx);
        }
    }

    public void OnUnEquipButton()
    {
        UnEquip(selectedItemIdx);
    }
}
