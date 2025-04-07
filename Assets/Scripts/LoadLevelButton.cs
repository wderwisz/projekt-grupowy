using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public class LoadLevelButton : MonoBehaviour
{
    public LoadController loadController;
    public TMP_Text levelText;

    public void LoadFromFile()
    {
        string path = Path.Combine(Application.persistentDataPath ,"saves", levelText.text +".json");
        //Debug.Log(levelText.text);
        loadController.Load(path);
    }
   
}
