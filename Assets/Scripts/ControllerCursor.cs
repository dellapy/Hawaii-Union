using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ControllerCursor : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty moveAction;     // Vector2 (stick or d‑pad)
    public InputActionProperty fireAction;     // Button – left click equivalent
    public InputActionProperty altFireAction;  // Button – right click equivalent

    [Header("Cursor Settings")]
    public float moveSpeed = 800f;
    public Camera mainCamera;
    public RectTransform uiBounds; // Optional: for limiting to a UI rect

    private Vector2 _screenPos;
    private bool _firePressedLastFrame;
    private bool _altFirePressedLastFrame;

    private void OnEnable()
    {
        moveAction.action.Enable();
        fireAction.action.Enable();
        altFireAction.action.Enable();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        fireAction.action.Disable();
        altFireAction.action.Disable();
    }

    private void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        _screenPos = mainCamera.WorldToScreenPoint(transform.position);
    }

    private void Update()
    {
        HandleMovement();
        HandleClick();
        UpdateWorldPosition();
    }

    private void HandleMovement()
    {
        Vector2 move = moveAction.action.ReadValue<Vector2>();
        _screenPos += move * moveSpeed * Time.unscaledDeltaTime;

        // Optionally clamp to screen or UI rect
        if (uiBounds != null)
        {
            Vector3[] corners = new Vector3[4];
            uiBounds.GetWorldCorners(corners);
            float minX = corners[0].x;
            float maxX = corners[2].x;
            float minY = corners[0].y;
            float maxY = corners[2].y;
            _screenPos.x = Mathf.Clamp(_screenPos.x, minX, maxX);
            _screenPos.y = Mathf.Clamp(_screenPos.y, minY, maxY);
        }
        else
        {
            _screenPos.x = Mathf.Clamp(_screenPos.x, 0, Screen.width);
            _screenPos.y = Mathf.Clamp(_screenPos.y, 0, Screen.height);
        }
    }

    private void HandleClick()
    {
        bool fireNow = fireAction.action.IsPressed();
        bool altFireNow = altFireAction.action.IsPressed();

        // Left click (press edge)
        if (fireNow && !_firePressedLastFrame)
        {
            RaycastAndClick(PointerEventData.InputButton.Left);
        }

        // Right click (press edge)
        if (altFireNow && !_altFirePressedLastFrame)
        {
            RaycastAndClick(PointerEventData.InputButton.Right);
        }

        _firePressedLastFrame = fireNow;
        _altFirePressedLastFrame = altFireNow;
    }

    private void RaycastAndClick(PointerEventData.InputButton button)
    {
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(_screenPos.x, _screenPos.y, Mathf.Abs(mainCamera.transform.position.z)));
        Vector2 world2D = new Vector2(worldPos.x, worldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(world2D, Vector2.zero);
        if (hit.collider != null)
        {
            TileBehavior tile = hit.collider.GetComponent<TileBehavior>();
            if (tile != null)
            {
                // Simulate pointer down event
                PointerEventData data = new PointerEventData(EventSystem.current)
                {
                    button = button,
                    position = _screenPos
                };
                tile.OnPointerDown(data);
            }
        }
    }

    private void UpdateWorldPosition()
    {
        Vector3 world = mainCamera.ScreenToWorldPoint(new Vector3(_screenPos.x, _screenPos.y, Mathf.Abs(mainCamera.transform.position.z)));
        world.z = 0f;
        transform.position = world;
    }
}
