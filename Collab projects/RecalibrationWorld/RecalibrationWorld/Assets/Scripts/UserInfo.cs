using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class UserInfo : MonoBehaviour {

	private GameObject settings;

    public InputField number;
    public InputField age;
    public Dropdown gender;
    public InputField eyeHeight;
    public InputField legLength;
    public InputField weight;
    public Dropdown condition;

    public string filePath;

    private bool validData = false;



	// Use this for initialization
	void Start () {
	
		settings = GameObject.FindGameObjectWithTag ("Settings");

		filePath = settings.GetComponent<SettingsSingleton>().filePath;



	}

    public void loadTrial()
    {
        //Create directory
        if (!Directory.Exists(Application.dataPath + "/Data"))
        {
            Directory.CreateDirectory(Application.dataPath + "/Data");
        }

        //Generate filepath
        filePath = Application.dataPath + "/Data/Subject" + number.text;

        string filename = filePath + ".txt";

        //Check for duplicate participant numbers
        if (System.IO.File.Exists(filename))
        {
            bool exists = true;
            string num = "";

            int duplicate_num = 0;

            while (exists)
            {
                duplicate_num++;
                num = Application.dataPath + "/Data/Subject" + number.text + "_" + duplicate_num + ".txt";

                if (!System.IO.File.Exists(num) || duplicate_num > 1000)
                {
                    exists = false;
                    filePath = num;
                }
            }
        }
        else
        {
            filePath = filename;
        }

        //Get gender to a string
        string sex;
        if (gender.value == 0)
            sex = "Male";
        else if (gender.value == 1)
            sex = "Female";
        else
            sex = "Not Specified";

        //Get condition to string
        SettingsSingleton.Condition cond;
        if (condition.value == 0)
        {
            cond = SettingsSingleton.Condition.Matched;
        }
        else if (condition.value == 1)
        {
            cond = SettingsSingleton.Condition.Slowest;
        }
        else if (condition.value == 2)
        {
            cond = SettingsSingleton.Condition.Slower;
        }
        else if (condition.value == 3)
        {
            cond = SettingsSingleton.Condition.Faster;
        }
        else if (condition.value == 4)
        {
            cond = SettingsSingleton.Condition.Fastest;
        }
        else
            cond = SettingsSingleton.Condition.Matched;

        //Write participant info
        System.IO.File.AppendAllText(filePath, "Number," + number.text + "\r\n" + "Age," + age.text + "\r\n" + "Gender," + sex + "\r\n" + "Eye Height," + eyeHeight.text + "\r\n" + "Leg Length," +  legLength.text + "\r\n" + "Weight," + weight.text + "\r\n" + "Condition," + cond.ToString() + "\r\n");

        //Save to settings singleton
        settings.GetComponent<SettingsSingleton>().number = number.text;
        settings.GetComponent<SettingsSingleton>().age = age.text;
        settings.GetComponent<SettingsSingleton>().gender = sex;
        settings.GetComponent<SettingsSingleton>().eyeHeight = eyeHeight.text;
        settings.GetComponent<SettingsSingleton>().legLength = legLength.text;
        settings.GetComponent<SettingsSingleton>().weight = weight.text;
        settings.GetComponent<SettingsSingleton>().filePath = filePath;
        settings.GetComponent<SettingsSingleton>().currentCondition = cond;

        Application.LoadLevel("Locomotion");
    }

} 
