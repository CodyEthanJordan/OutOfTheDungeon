using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.UI
{
    public class MainMenuController : MonoBehaviour
    {
        private PlayableDirector pd;
        private void Awake()
        {
            pd = GetComponent<PlayableDirector>();
        }

        public void StartGame()
        {
            pd.Play();
            StartCoroutine(SceneTransition());
        }

        private System.Collections.IEnumerator SceneTransition()
        {
            yield return new WaitForSeconds((float)pd.duration);
            SceneManager.LoadScene("DungeonScene", LoadSceneMode.Single);
        }
    }
}
