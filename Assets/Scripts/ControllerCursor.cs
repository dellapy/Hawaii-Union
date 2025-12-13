using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class ControllerCursor : MonoBehaviour
{
    [Header("Input")]
    public InputActionProperty moveAction;     // Vector2 – move cursor
    public InputActionProperty fireAction;     // Button – tile left click
    public InputActionProperty altFireAction;  // Button – tile right click
    public InputActionProperty uiAction;       // Button – UI press

    [Header("References")]
    public Canvas canvas;
    public Camera worldCamera;                // main camera used for tiles
    public float moveSpeed = 800f;

    [Header("Tile raycast")]
    public LayerMask tileLayerMask;           // assign tile 3D layer in Inspector

    private RectTransform _rectTransform;
    private Vector2 _screenPos;
    private bool _firePressedLastFrame;
    private bool _altPressedLastFrame;
    private bool _uiPressedLastFrame;

    private readonly List<RaycastResult> _uiResults = new List<RaycastResult>();

    private TileBehavior _lastHoveredTile;
    private Button _lastHoveredButton;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();
        if (worldCamera == null)
            worldCamera = Camera.main;

        // Listen for device changes to enable/disable based on controllers
        InputSystem.onDeviceChange += OnDeviceChange;
        UpdateEnabledFromDevices();
    }

    private void OnDestroy()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnEnable()
    {
        moveAction.action.Enable();
        fireAction.action.Enable();
        altFireAction.action.Enable();
        uiAction.action.Enable();

        _screenPos = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        SetCursorPositionFromScreen();
    }

    private void OnDisable()
    {
        moveAction.action.Disable();
        fireAction.action.Disable();
        altFireAction.action.Disable();
        uiAction.action.Disable();
    }

    private void Update()
    {
        // If disabled due to no controller, do nothing
        if (!enabled)
            return;

        HandleMovement();
        HandleInputs();
        HandleHover();
    }

    // Enable only when a gamepad (or similar) is present
    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        // React only to adds/removals
        if (change == InputDeviceChange.Added || change == InputDeviceChange.Removed ||
            change == InputDeviceChange.Disconnected || change == InputDeviceChange.Reconnected)
        {
            UpdateEnabledFromDevices();
        }
    }

    private void UpdateEnabledFromDevices()
    {
        bool hasGamepad = Gamepad.all.Count > 0;
        enabled = hasGamepad;
    }

    private void HandleMovement()
    {
        Vector2 move = moveAction.action.ReadValue<Vector2>();
        _screenPos += move * moveSpeed * Time.unscaledDeltaTime;

        _screenPos.x = Mathf.Clamp(_screenPos.x, 0, Screen.width);
        _screenPos.y = Mathf.Clamp(_screenPos.y, 0, Screen.height);

        SetCursorPositionFromScreen();
    }

    private void SetCursorPositionFromScreen()
    {
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            _screenPos,
            canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera,
            out localPos);

        _rectTransform.anchoredPosition = localPos;
    }

    private void HandleInputs()
    {
        bool fireNow = fireAction.action.IsPressed();
        bool altNow = altFireAction.action.IsPressed();
        bool uiNow = uiAction.action.IsPressed();

        if (fireNow && !_firePressedLastFrame)
            ClickTile(PointerEventData.InputButton.Left);

        if (altNow && !_altPressedLastFrame)
            ClickTile(PointerEventData.InputButton.Right);

        if (uiNow && !_uiPressedLastFrame)
            ClickUI();

        _firePressedLastFrame = fireNow;
        _altPressedLastFrame = altNow;
        _uiPressedLastFrame = uiNow;
    }

    private void HandleHover()
    {
        // UI hover
        Button hoveredButton = RaycastForButton();
        if (hoveredButton != _lastHoveredButton)
            _lastHoveredButton = hoveredButton;

        // Tile hover
        TileBehavior hoveredTile = GetTileUnderCursor();
        if (hoveredTile != _lastHoveredTile)
            _lastHoveredTile = hoveredTile;
    }

    // === UI ============================================================

    private Button RaycastForButton()
    {
        if (EventSystem.current == null)
            return null;

        PointerEventData data = new PointerEventData(EventSystem.current)
        {
            position = _screenPos
        };

        _uiResults.Clear();
        EventSystem.current.RaycastAll(data, _uiResults);

        foreach (var r in _uiResults)
        {
            Button b = r.gameObject.GetComponent<Button>();
            if (b != null)
                return b;
        }

        return null;
    }

    private void ClickUI()
    {
        if (EventSystem.current == null)
            return;

        Button hoveredButton = RaycastForButton();
        if (hoveredButton == null)
            return;

        PointerEventData data = new PointerEventData(EventSystem.current)
        {
            position = _screenPos,
            button = PointerEventData.InputButton.Left
        };

        GameObject target = hoveredButton.gameObject;

        ExecuteEvents.Execute(target, data, ExecuteEvents.pointerEnterHandler);
        ExecuteEvents.Execute(target, data, ExecuteEvents.pointerDownHandler);
        ExecuteEvents.Execute(target, data, ExecuteEvents.pointerUpHandler);
        ExecuteEvents.Execute(target, data, ExecuteEvents.pointerClickHandler);
    }

    // === TILES =========================================================

    private TileBehavior GetTileUnderCursor()
    {
        if (worldCamera == null)
            return null;

        Ray ray = worldCamera.ScreenPointToRay(_screenPos);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, tileLayerMask))
        {
            return hit.collider.GetComponent<TileBehavior>();
        }

        return null;
    }

    private void ClickTile(PointerEventData.InputButton button)
    {
        if (EventSystem.current == null)
            return;

        TileBehavior tile = GetTileUnderCursor();
        if (tile == null)
            return;

        PointerEventData data = new PointerEventData(EventSystem.current)
        {
            button = button,
            position = _screenPos
        };

        tile.OnPointerDown(data);
    }
}