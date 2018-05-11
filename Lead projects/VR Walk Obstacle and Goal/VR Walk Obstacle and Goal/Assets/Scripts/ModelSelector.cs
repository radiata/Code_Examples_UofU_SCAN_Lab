////////////////////////////////////////////////////////////////////////////////////
// Experiment: Risk Aversion
// Purpose: Stand alone to itterate through the model array and activate the relevant object
// Code by: Butler, Michael
// Email: msbcoding@gmail.com
////////////////////////////////////////////////////////////////////////////////////
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelSelector : MonoBehaviour {

    public GameObject[] ModelArray;

    public void Selector(int indexNumber)
    {
        int iterI = 0;
        foreach(GameObject i in ModelArray)
        {
            if(iterI == indexNumber)
            {
                ModelArray[iterI].SetActive(true);
            }
            else
            {
                ModelArray[iterI].SetActive(false);
            }
            iterI++;
        }
        
    }
}
