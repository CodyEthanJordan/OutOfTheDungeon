using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


namespace Assets.Scripts.GameLogic
{
    [CreateAssetMenu(menuName = "Dungeon/DungeonTile")]
    public class DungeonTile : Tile
    {

        public bool Passable;
        public string Name;
        public string Description;
        public bool Slippery; //makes units pushed keep sliding
        public bool Deadly;


        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
