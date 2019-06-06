using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class MenuBehaviour : MonoBehaviour
{
    public UnityEngine.UI.Text TextUsername;
    public UnityEngine.UI.Text TextPassword;
    public GameObject loginCanvas;
    public static UserInfo userInfo;
    private bool loggedIn = false;

    public void PlayGame() {
        if(loggedIn)
            SceneManager.LoadScene("ClientScene");
    }

    public void HostGame() {
        SceneManager.LoadScene("ServerScene");
    }

    public void Login() {
        string request = "https://studenthome.hku.nl/~daniel.bergshoeff/KGDEV4/login.php?username=" + TextUsername.text + "&password=" + TextPassword.text;
        StartCoroutine(Communication.GetRequest(request, (returnedString) => {
            if (returnedString == "") {
                Uncode(returnedString);
            }
        }));
    }

    public void Register() {
        string request = "https://studenthome.hku.nl/~daniel.bergshoeff/KGDEV4/register.php?username=" + TextUsername.text + "&password=" + TextPassword.text;
        StartCoroutine(Communication.SetRequest(request));
    }

    private void Uncode(string json) {
        UserInfo ui = JsonUtility.FromJson<UserInfo>(json);
        if (ui != null) {
            if (ui.sessid != "0") {
                userInfo = ui;
            }
        }
    }

    public class UserInfo {
        public string sessid;
        public string username;
    }
}
