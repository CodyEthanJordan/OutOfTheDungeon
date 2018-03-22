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
        public Vector3Int[] HeroStartLocations;
        public float Width;
        public float Height;

        public int xMin
        {
            get { return (int)Math.Round(this.transform.position.x - Width / 2.0f); }
        }
        public int xMax
        {
            get { return (int)Math.Round(this.transform.position.x + Width / 2.0f); }
        }
        public int yMin
        {
            get { return (int)Math.Round(this.transform.position.y - Height / 2.0f); }
        }
        public int yMax
        {
            get { return (int)Math.Round(this.transform.position.y + Height / 2.0f); }
        }


        private void OnDrawGizmosSelected()
        {
            Color previous = Gizmos.color;
            Gizmos.color = Color.red;

            Vector3 topLeft = new Vector3(xMin, yMax, this.transform.position.z);
            Vector3 topRight = new Vector3(xMax, yMax, this.transform.position.z);
            Vector3 bottomLeft = new Vector3(xMin, yMin, this.transform.position.z);
            Vector3 bottomRight = new Vector3(xMax, yMin, this.transform.position.z);

            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(topRight, bottomRight);
            Gizmos.DrawLine(bottomRight, bottomLeft);
            Gizmos.DrawLine(bottomLeft, topLeft);

            Gizmos.color = Color.green;
            Vector3 gridOffset = new Vector3(0.5f, 0.5f, 0);
            foreach (var start in HeroStartLocations)
            {
                Gizmos.DrawWireSphere(this.transform.position + start + gridOffset, 0.4f);
            }

            Gizmos.color = previous;
        }
    }
}
