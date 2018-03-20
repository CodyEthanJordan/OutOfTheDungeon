using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    class MouseoverPopup : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        public GameObject DetailsBox;
        public String Details = "";

        public void OnPointerEnter(PointerEventData eventData)
        {
            DetailsBox.SetActive(true);
            DetailsBox.GetComponentInChildren<Text>().text = Details;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            DetailsBox.SetActive(false);
        }
    }
}
