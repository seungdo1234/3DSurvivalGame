using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public string GetInteractPrompt(); // 화면에다 띄어줄 프롬프트에 관한 함수
    public void OnInteract(); // 상호작용 효과
}

public class ItemObject : MonoBehaviour , IInteractable
{
    public ItemData data;


    public string GetInteractPrompt()
    {
        string str = $"{data.displayName}\n{data.description}";
        return str;
    }

    public void OnInteract()
    {
        CharacterManager.Instance.Player.itemData = data;
        CharacterManager.Instance.Player.adddItem?.Invoke();
        Destroy(gameObject);
    }
}
