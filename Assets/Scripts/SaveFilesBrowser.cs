using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
public class SaveFilesBrowser : MonoBehaviour
{
    public GameObject buttonPrefab;
    public GameObject buttonParent;
    [SerializeField] private LoadController loadController;
    // Start is called before the first frame update
    private void OnEnable()
    {
        Debug.Log("funkcja dzia³a");

        foreach (Transform child in buttonParent.transform)
        {
            Destroy(child.gameObject);
        }

        DirectoryInfo directoryInfo = new DirectoryInfo(Path.Combine(Application.persistentDataPath + "/saves/"));
        FileInfo[] saveFiles = directoryInfo.GetFiles();
        int fileNumber = saveFiles.Length;
        for (int i = 0; i < fileNumber; i++ ) {
            GameObject newButton = Instantiate(buttonPrefab, buttonParent.transform);
            string name = saveFiles[i].Name;
            name = name.Substring(0,name.Length - 5);
            Debug.Log(name);
            newButton.GetComponent<LoadLevelButton>().levelText.text = name;
            newButton.GetComponent<LoadLevelButton>().loadController = loadController;
            //newButton.GetComponent<Button>().onClick.AddListener(() => SelectSave(saveFiles[i].Name));
        }
    }

    private void SelectSave(string saveName)
    {
        Debug.Log("Uda³o sie wybraæ plik " + saveName);
    }



}
