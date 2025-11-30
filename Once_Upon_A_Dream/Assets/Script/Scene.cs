using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Scene : MonoBehaviour
{
    public void NewClick()
    {
        SceneManager.LoadScene("NewAccount");
    }

    public void Login()
    {
        SceneManager.LoadScene("Story1");
    }

    public void Skip()
    {
        SceneManager.LoadScene("Login");
    }

}
