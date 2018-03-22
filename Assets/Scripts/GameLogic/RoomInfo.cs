using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Scripts.GameLogic
{
    public class RoomInfo : MonoBehaviour
    {
        public Loadout[] InitialEnemies;
        public Loadout[] SpawnableBadGuys;
        public float Width;
        public float Height;


        private void OnDrawGizmosSelected()
        {
            Color previous = Gizmos.color;
            Gizmos.color = Color.red;
            Gizmos.DrawLine(this.transform.position - Width / 2.0f * Vector3.left - Height/2.0f * Vector3.up,
                this.transform.position + Width / 2.0f * Vector3.left - Height / 2.0f * Vector3.up);
            Gizmos.DrawLine(this.transform.position + Width / 2.0f * Vector3.left - Height/2.0f * Vector3.up,
                this.transform.position + Width / 2.0f * Vector3.left + Height / 2.0f * Vector3.up);
            Gizmos.DrawLine(this.transform.position + Width / 2.0f * Vector3.left + Height/2.0f * Vector3.up,
                this.transform.position - Width / 2.0f * Vector3.left + Height / 2.0f * Vector3.up);
            Gizmos.DrawLine(this.transform.position - Width / 2.0f * Vector3.left + Height/2.0f * Vector3.up,
                this.transform.position - Width / 2.0f * Vector3.left - Height / 2.0f * Vector3.up);
            Gizmos.color = previous;
        }
    }
}
