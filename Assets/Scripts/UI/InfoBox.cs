using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class InfoBox : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Text description;
        //TODO: mouseover icons or something

        public void Setup(string titleText, string descriptionText)
        {
            title.text = titleText;
            description.text = descriptionText;
        }
    }
}
