using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The <c>GridInputMethodMouseKeyboard</c> class determines how grid tile selection is handled with the MouseKeyboard control scheme.
/// </summary>
[Serializable]
public class GridInputMethodMouseKeyboard : IGridInputMethod
{
    /// <summary>
    /// The name of the control scheme we're checking for input.
    /// </summary>
    [SerializeField] private string _controlSchemeName = "MouseKeyboard";

    /// <summary>
    /// A reference to the <c>GameTileGrid</c>.
    /// </summary>
    [SerializeField] private GameTileGrid _grid; 

    /// <summary>
    /// A reference to the current selection.
    /// </summary>
    [SerializeField] private GridSelection _selection;

    /// <summary>
    /// The camera from which we want to shoot a raycast and check the mouse position.
    /// </summary>
    [SerializeField] private Camera raycastCamera; 
    public string ControlSchemeName { get => _controlSchemeName; }
    public GridSelection Selection { get => _selection; }

    public Vector3Int OnMove(InputAction.CallbackContext context)
    {
        Cursor.visible = true;
        if (!raycastCamera)
        {
            return _selection.CurrentTile.Key;
        }

        Ray ray = raycastCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo, Mathf.Infinity))
        {
            if (hitInfo.transform.gameObject.TryGetComponent(out GameTile tile))
            {
                return _grid.WorldToCell(hitInfo.transform.position);
            }
        }
       
        return _selection.CurrentTile.Key;
    }
}
