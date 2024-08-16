using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// The <c>IGridInputMethod</c> defines a set of features necessary to create a new method of input for traversing the grid. 
/// </summary>
public interface IGridInputMethod
{
    public string ControlSchemeName { get; }
    public GridSelection Selection { get; }

    /// <summary>
    /// The function to be called when moving.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public Vector3Int OnMove(InputAction.CallbackContext context);
}