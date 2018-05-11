using UnityEngine;

//Singleton script to allow settings to persist between levels/menus
public class SettingsSingleton : MonoBehaviour {
	
	private static SettingsSingleton instance = null;

	//Save user info in the settings singleton
    [HideInInspector]
	public string filePath = "";
    [HideInInspector]
    public string number = "-1";
    [HideInInspector]
    public string age = "-1";
    [HideInInspector]
    public string eyeHeight = "-1f";
    [HideInInspector]
    public string legLength = "-1f";
    [HideInInspector]
    public string weight = "-1f";
    [HideInInspector]
    public string gender = "Not Specified";

    public enum Condition
    {Slowest, Slower, Matched, Faster, Fastest, None};
    public Condition currentCondition = Condition.None;

    public float matchedScale = 1.0f;
    public float slowestScale = 0.5f;
    public float slowerScale = 0.75f;
    public float fasterScale = 1.5f;
    public float fastestScale = 2.0f;



    public static SettingsSingleton Instance {
		get { return instance; }
	}
	
	void Awake() {
		if (instance != null && instance != this) {
			Destroy(this.gameObject);
			return;
		} else {
			instance = this;
		}
		DontDestroyOnLoad(this.gameObject);
	}
}
