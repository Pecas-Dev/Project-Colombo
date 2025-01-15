using System;
using UnityEngine;


namespace ProjectColombo.Combat
{
    [Serializable]
    public class Attack
    {
        [Header("Animation Values")]
        [field: SerializeField] public string AnimationName { get; private set; }
        [field: SerializeField] public float TransitionDuration { get; private set; }
        [field: SerializeField] public float ComboAttackTime { get; private set; }

        [Header("Index")]
        [field: SerializeField] public int ComboStateIndex { get; private set; } = -1;
    }
}
