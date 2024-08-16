using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class GridInputMethodGamepad : IGridInputMethod
{
    /// <summary>
    /// The name of the control scheme we're checking for input.
    /// </summary>
    [SerializeField] private string _controlSchemeName = "Gamepad";

    /// <summary>
    /// A reference to the current selection.
    /// </summary>
    [SerializeField] private GridSelection _selection;

    [SerializeField, Range(0, 1)] private float _deadZone; 
    public string ControlSchemeName { get => _controlSchemeName; }
    public GridSelection Selection { get => _selection; }

    public Vector3Int OnMove(InputAction.CallbackContext context)
    {
        Cursor.visible = false;
        Vector2 input = context.ReadValue<Vector2>();
        Vector3Int position = _selection.CurrentTile.Key;
       
        int xDir = Math.Sign(input.x);
        int zDir = Math.Sign(input.y);

        if (input.x < _deadZone && input.x > -_deadZone)
        {
            xDir = 0;
        }


        if (input.y < _deadZone && input.y > -_deadZone)
        {
            zDir = 0;
        }

        Vector3Int newPosition = new Vector3Int(position.x + xDir, position.y + zDir, 0);
        return newPosition;
    }

}
