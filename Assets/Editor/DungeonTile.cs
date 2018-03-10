using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(menuName = "Dungeon/DungeonTile")]
public class DungeonTile : Tile {

    public bool Passable;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

//#if UNITY_EDITOR
//    // The following is a helper that adds a menu item to create a RoadTile Asset
//    [MenuItem("Assets/Create/Dungeon/DungeonTile")]
//    public static void CreateDungeonTile()
//    {
//        string path = EditorUtility.SaveFilePanelInProject("Save Dungeon Tile", "New Dungeon Tile", "Asset", "Save Dungeon Tile", "Assets/Tilemaps");
//        if (path == "")
//            return;
//        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<DungeonTile>(), path);
//    }
//#endif


}

//[CustomEditor(typeof(DungeonTile))]
//public class MyClassEditor : Editor
//{
//    public override void OnInspectorGUI()
//    {
//        TileBase t;
//        t.an
//        DungeonTile tile = (DungeonTile)target;
//        tile.Sprite = EditorGUILayout.ObjectField(source, typeof(Object), true);
//        EditorGUILayout.EndHorizontal();
//    }
//}
