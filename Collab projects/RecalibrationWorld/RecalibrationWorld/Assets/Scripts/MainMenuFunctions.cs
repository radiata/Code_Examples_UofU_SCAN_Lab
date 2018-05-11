using UnityEngine;

public class MainMenuFunctions : MonoBehaviour {



	public void Exit()
	{
        #if UNITY_STANDALONE
            Application.Quit();
        #endif

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

}
