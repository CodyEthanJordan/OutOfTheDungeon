using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts.Events
{
    [System.Serializable]
    public class PositionEvent : UnityEvent<Vector3Int>
    {
    }
}
