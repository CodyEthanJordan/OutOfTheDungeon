using Assets.Scripts.GameLogic;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Events
{
    [System.Serializable]
    public class TargetedAbilityEvent : UnityEvent<Ability, Vector3Int>
    {
    }
}
