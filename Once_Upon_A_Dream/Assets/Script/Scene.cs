using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Scene : MonoBehaviour
{
    private int clicked;
    public void NewClick()
    {
        SceneManager.LoadScene("NewAccount");
    }

    public void Login()
    {
        SceneManager.LoadScene("LogIn");
    }

}
