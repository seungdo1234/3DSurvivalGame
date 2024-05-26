using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

public class Interaction : MonoBehaviour
{
    public float checkRate = 0.05f; // 얼마나 자주 레이를 쏠 것 인가 ?
    private float lastCheckTime;
    public float maxCheckDistance;
    public LayerMask layerMask;

    // 아이템 캐싱 관련 변수들
    public GameObject curInteractGameObject;
    private IInteractable curInteractable;

    public TextMeshProUGUI promptText;
    public Camera camera;

    private void Start()
    {
        camera = Camera.main;
    }

    private void Update()
    {
        if(Time.time - lastCheckTime > checkRate) // 체크레이트마다 오브젝트 탐색
        {
            lastCheckTime = Time.time;
            
            // ScreenToViewportPoint : 터치했을 때 기준
            // ScreenPointToRay : 카메라 기준으로 레이를 쏨
            // new Vector3(Screen.width / 2, Screen.height / 2) => 정 중앙에서 쏘기 위해
            // 카메라가 찍고 있는 방향이 기본적으로 앞을 바라보기 때문에 따로 방향 설정 X
            Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxCheckDistance, layerMask)) // 충돌이 됐을 때
            {
                if (hit.collider.gameObject != curInteractGameObject) // 충돌한 오브젝트가 현재 상호작용하는 오브젝트가 아닐 때
                {
                    curInteractGameObject = hit.collider.gameObject; // 오브젝트 변경
                    curInteractable = hit.collider.GetComponent<IInteractable>();
                    // 프롬프트에 출력
                    SetPromptText();
                }
            }
            else // 충돌한 오브젝트가 없다면 기존에 있던 정보들을 초기화
            {
                InteractionOff();
            }
        }
    }

    private void SetPromptText()
    {
        promptText.gameObject.SetActive(true);
        promptText.text = curInteractable.GetInteractPrompt();
    }

    public void InteractionOff()
    {
        curInteractGameObject = null; 
        curInteractable = null;
        promptText.gameObject.SetActive(false);
    }

    public void OnInteractInput(InputAction.CallbackContext context) // 상호작용
    {
        if (context.phase == InputActionPhase.Started && curInteractGameObject != null)
        {
            curInteractable.OnInteract();
            InteractionOff();
        }
    }
}
