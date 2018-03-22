using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Assets.Scripts;

public class VectorMathTest {

	[Test]
	public void CardinalDirections() {
        Vector3Int a = new Vector3Int(1, 2, 0);
        Vector3Int b = a + new Vector3Int(4,0,0);

        var dir = GameManager.CardinalDirectionTo(a, b);
        Assert.AreEqual(Vector3Int.right, dir);

        Vector3Int c = a + new Vector3Int(0, -5, 0);
        dir = GameManager.CardinalDirectionTo(a, c);
        Assert.AreEqual(Vector3Int.down, dir);
	}
}
