using UnityEngine;

namespace Assets.Scripts.GameLogic.SpecialEffects
{
    [CreateAssetMenu(menuName = "Dungeon/SpecialEffects/SummonUnitGeneric")]
    public class SummonUnit : SpecialEffect
    {
        private Loadout guyToSummon;
        private UnitController.SideEnum side;

        public override void ApplyEffect(GameManager gm, UnitController user, UnitController guyHit, Vector3Int targetLocation)
        {
            if(guyToSummon == null)
            {
                Debug.LogError("Never got set up to summon unit");
                return;
            }

            if (guyHit != null)
            {
                //someone in the way
                guyHit.TakeDamage(1, Effect.DamageTypes.SummoningBlocked);
            }
            else
            {
                gm.SpawnUnit(targetLocation, guyToSummon.LoadoutName, side, guyToSummon);
            }
        }

        public void Setup(Loadout loadout, UnitController.SideEnum sideOn)
        {
            guyToSummon = loadout;
            side = sideOn;
        }
    }
}
