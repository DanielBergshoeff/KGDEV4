using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuBehaviour : MonoBehaviour
{
    public Text TextUsername;
    public Text TextPassword;
    public Text DisplayUsername;
    public GameObject LoginToPlayText;
    public GameObject LoggedInAsText;
    public GameObject LoginButton;

    public Dropdown dobyear;
    public Dropdown dobmonth;
    public Dropdown dobday;

    public GameObject loginCanvas;
    public GameObject registerCanvas;
    public static UserInfo userInfo;
    private bool loggedIn = false;

    private void Start() {
        dobyear.options.Clear();
        for (int i = 2019; i > 1900; i--) {
            dobyear.options.Add(new Dropdown.OptionData(i.ToString()));
        }

        dobmonth.options.Clear();
        for (int i = 1; i < 13; i++) {
            dobmonth.options.Add(new Dropdown.OptionData(i.ToString("00")));
        }

        dobday.options.Clear();
        for (int i = 1; i < 32; i++) {
            dobday.options.Add(new Dropdown.OptionData(i.ToString("00")));
        }
    }

    public void PlayGame() {
        if(loggedIn)
            SceneManager.LoadScene("ClientScene");
    }

    public void HostGame() {
        SceneManager.LoadScene("ServerScene");
    }

    public void OpenLoginMenu() {
        loginCanvas.SetActive(true);
    }

    public void OpenRegisterMenu() {
        loginCanvas.SetActive(false);
        registerCanvas.SetActive(true);
    }

    public void Login() {
        string request = "https://studenthome.hku.nl/~daniel.bergshoeff/KGDEV4/login.php?username=" + TextUsername.text + "&password=" + TextPassword.text;
        StartCoroutine(Communication.GetRequest(request, (string returnedString) => {
            Debug.Log(returnedString);
            Uncode(returnedString);
        }));
    }

    public void Register() {
        string dob = dobyear.options[dobyear.value].text + dobmonth.options[dobmonth.value].text + dobday.options[dobday.value].text;
        DateTime dt;
        if (DateTime.TryParseExact(dob, "yyyyMMdd",
                          CultureInfo.InvariantCulture,
                          DateTimeStyles.None, out dt)) {
            string request = "https://studenthome.hku.nl/~daniel.bergshoeff/KGDEV4/register.php?username=" + TextUsername.text + "&password=" + TextPassword.text + "&date_of_birth=" + dt;
            StartCoroutine(Communication.SetRequest(request));
        }
    }

    private void Uncode(string json) {
        UserInfo ui = JsonUtility.FromJson<UserInfo>(json);
        if (ui != null) {
            if (ui.sessid != "0") {
                userInfo = ui;
                loggedIn = true;
                loginCanvas.SetActive(false);
                DisplayUsername.gameObject.SetActive(true);
                DisplayUsername.text = ui.username.ToUpper();
                LoggedInAsText.SetActive(true);
                LoginToPlayText.SetActive(false);
                LoginButton.SetActive(false);
            }
        }
    }

    public class UserInfo {
        public string sessid;
        public string username;
    }
}
