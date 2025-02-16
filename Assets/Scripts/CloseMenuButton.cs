using UnityEngine;

public class CloseMenuOnClick : MonoBehaviour
{
    public MenuController menuController;

    public void OnClick()
    {
        Debug.Log("wciœnieto przycisk");
        menuController.CloseMenu();
    }
}
