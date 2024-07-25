using System;
using UnityEngine;

[Serializable]
public sealed class ScriptableTextFunction : ScriptableObject
{
    [SerializeReference]
    private ITextFunction _function;
    public void OnTextComplete(ITextAdvancer advancer) => _function?.OnTextComplete(advancer);

}
