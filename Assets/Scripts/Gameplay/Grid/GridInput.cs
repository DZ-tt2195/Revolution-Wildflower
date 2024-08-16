using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class GridInput
{
    [SerializeField] private GridSelection _selection;
    [SerializeReference, SubclassSelector] private List<IGridInputMethod> _inputMethods;
    [SerializeField] private int cooldownMilliseconds = 250;
    private GridInputActions _controls;
    private bool _canMove = false;

    private InputAction _move;
    private InputAction _select;

    public void Enable()
    {
        _controls = new GridInputActions();

        _move = _controls.Gameplay.Move;
        _move.performed += OnMove;
        _move.Enable();

        _canMove = true;

        _select = _controls.Gameplay.Select;
        _select.performed += OnSelect;
        _select.Enable();
    }

    public void Disable()
    {

    }

    public void OnMove(InputAction.CallbackContext context)
    {
        if (!_canMove)
        {
            return; 
        }

        string group = context.action.GetBindingForControl(context.control).Value.groups;
        var matches = _inputMethods.Where(x => x.ControlSchemeName == group);
        if (matches.Any())
        {
            Vector3Int position = matches.First().OnMove(context);
            if (position != _selection.CurrentTile.Key)
            {
                _selection.SetSelection(position);
            }
        }

        _canMove = false;
        Cooldown(cooldownMilliseconds);
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        Debug.Log("select");
        _selection.Fire();
    }

    private async void Cooldown(int cooldownMilliseconds)
    {
        await Task.Delay(cooldownMilliseconds);
        _canMove = true;
    }
}
