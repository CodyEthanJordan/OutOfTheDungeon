using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour {

    public Tilemap UIHighlights;

    public enum GameState
    {
        BaseState,
        UnitSelected,
        Targeting
    }

	// Use this for initialization
	void Awake () {
	}
	
	// Update is called once per frame
	void Update () {

        if(Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);

            if (hit.collider != null)
            {
                Debug.Log("Target Position: " + hit.collider.gameObject.transform.position);
            }
        }

	}
}
