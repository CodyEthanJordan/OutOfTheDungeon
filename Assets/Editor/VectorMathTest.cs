using UnityEngine;
using UnityEditor;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using Assets.Scripts;
using Assets.Scripts.GameLogic;

public class VectorMathTest {

	[Test]
	public void CardinalDirections()
    {
        Vector3Int a = new Vector3Int(1, 2, 0);
        Vector3Int b = a + new Vector3Int(4,0,0);

        var dir = GameManager.CardinalDirectionTo(a, b);
        Assert.AreEqual(Vector3Int.right, dir);

        Vector3Int c = a + new Vector3Int(0, -5, 0);
        dir = GameManager.CardinalDirectionTo(a, c);
        Assert.AreEqual(Vector3Int.down, dir);
	}

    [Test]
    public void EffectVectorRotation()
    {
        Vector3Int longAttack = new Vector3Int(3, 0, 0);
        Assert.AreEqual(longAttack, Ability.RotateEffectTarget(longAttack, Vector3Int.right));
        Assert.AreEqual(new Vector3Int(0, 3, 0), Ability.RotateEffectTarget(longAttack, Vector3Int.up));
        Assert.AreEqual(new Vector3Int(0, -3, 0), Ability.RotateEffectTarget(longAttack, Vector3Int.down));
        Assert.AreEqual(new Vector3Int(-3, 0, 0), Ability.RotateEffectTarget(longAttack, Vector3Int.left));

        Vector3Int cornerAttack = new Vector3Int(2, 1, 0);
        Assert.AreEqual(new Vector3Int(2, 1, 0), Ability.RotateEffectTarget(cornerAttack, Vector3Int.right));
        Assert.AreEqual(new Vector3Int(-1, -2, 0), Ability.RotateEffectTarget(cornerAttack, Vector3Int.down));
        Assert.AreEqual(new Vector3Int(-2, 1, 0), Ability.RotateEffectTarget(cornerAttack, Vector3Int.left));
        Assert.AreEqual(new Vector3Int(1, 2, 0), Ability.RotateEffectTarget(cornerAttack, Vector3Int.up));
    }
}
