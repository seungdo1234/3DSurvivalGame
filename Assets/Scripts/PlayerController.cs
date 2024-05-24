using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("# Movement")] 
    public float moveSpeed;
    public float jumpPower;
    private Vector2 curMovementInput;
    public LayerMask groundLayerMask;
    [Header("# Look")] 
    public Transform cameraContainer;
    public float minXLook; // 마우스 X의 최대 최소 회전 값
    public float maxXLook;
    private float camCurXRot; // 마우스의 델타 값 저장
    public float lookSensitivity; // 회전 민감도
    private Vector2 mouseDelta;
    
    private Rigidbody rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        // 마우스 커서를 안보이게 함
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void LateUpdate()
    {
        CameraLook();
    }

    private void FixedUpdate()
    {
        Move();
    }
    
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed) // 키가 계속 눌리는 중
        {
            curMovementInput = context.ReadValue<Vector2>();
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            curMovementInput = Vector2.zero;
        }
    }
    private void Move()
    {
        // 앞뒤 + 좌우로 플레이어를 움직임
        Vector3 dir = transform.forward * curMovementInput.y + transform.right * curMovementInput.x;
        dir *= moveSpeed;
        // 점프를 했을 때만 위아래로 움직어여되기 때문에 미세한 값을 유지시키기위해 velocity.y를 넣음
        dir.y = rigid.velocity.y;
        rigid.velocity = dir;
    }
    
    public void OnLook(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
    }
    private void CameraLook() // 위 아래는 카메라 x축 회전, 좌우는 플레이어 y축 회전
    {
        // 좌우로 돌릴 때 y축을 회전시켜줘야 좌우로 움직이게됨
        camCurXRot += mouseDelta.y * lookSensitivity;
        // 최대 최소 회전 값을 넘지 않게 함
        camCurXRot = Mathf.Clamp(camCurXRot, minXLook, maxXLook);
        // 카메라의 로컬 좌표를 돌려줌  => 위 아래 (x 회전이 -일 때 위를 바라봄)
        cameraContainer.localEulerAngles = new Vector3(-camCurXRot, 0, 0);

        // 플레어이어 y축 회전 -> 좌우 회전
        transform.eulerAngles += new Vector3(0, mouseDelta.x * lookSensitivity, 0);
    }

    
    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Started && IsGrounded())
        {
            rigid.AddForce(Vector2.up * jumpPower, ForceMode.Impulse);
        }
    }

    private bool IsGrounded()
    {
        Ray[] rays = new Ray[4]
        {
            new Ray(transform.position + (transform.forward * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (-transform.forward * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (transform.right * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (-transform.right * 0.2f) + (transform.up * 0.01f), Vector3.down)
        };

        for (int i = 0; i < rays.Length; i++)
        {
            if (Physics.Raycast(rays[i], 0.1f, groundLayerMask))
            {
                return true;
            }
        }
        return false;
    }
}
