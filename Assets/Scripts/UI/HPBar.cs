using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Scripts.UI
{
    public class HPBar : MonoBehaviour
    {
        [SerializeField] private GameObject HPTick;

        private List<GameObject> ticks = new List<GameObject>();

        private int max;
        private int current;

        public void SetMaxHP(int maxHP)
        {
            foreach (var tick in ticks)
            {
                Destroy(tick);
            }
            ticks.Clear();

            max = maxHP;
            current = max;

            for (int i = 0; i < max; i++)
            {
                var newTickObject = Instantiate(HPTick, this.transform);
                ticks.Add(newTickObject);
            }
        }

        public void UpdateHP(int hp)
        {
            if (hp == current)
            {
                //don't need to do anything
                return;
            }

            if (hp < current)
            {
                for (int i = hp; i < current; i++)
                {
                    this.transform.GetChild(i).GetComponent<Animator>().SetTrigger("FadeOutTrigger");
                }

                current = hp;
            }
            else
            {
                for (int i = hp; i > current; i--)
                {
                    this.transform.GetChild(i).GetComponent<Animator>().SetTrigger("FadeInTrigger");
                }

                current = hp;
            }
        }
    }
}
