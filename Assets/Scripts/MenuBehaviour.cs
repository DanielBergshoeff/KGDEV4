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
    public InputField TextUsername;
    public InputField TextPassword;
    public Text DisplayUsername;

    public InputField RegisterUsername;
    public InputField RegisterPassword;

    public InputField EditPassword;

    public Text TextHighscores;

    public GameObject LoginToPlayText;
    public GameObject LoggedInAsText;
    public GameObject LoginButton;
    public GameObject LoggedIn;

    public Dropdown dobyear;
    public Dropdown dobmonth;
    public Dropdown dobday;

    public GameObject loginCanvas;
    public GameObject registerCanvas;
    public static UserInfo userInfo;
    private bool loggedIn = false;
    private string[] highscores;

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

        SetHighScoresAllTime();

        if(userInfo != null) {
            LoggedIn.SetActive(true);
            loggedIn = true;
            loginCanvas.SetActive(false);
            registerCanvas.SetActive(false);
            DisplayUsername.text = userInfo.username.ToUpper();
            LoginToPlayText.SetActive(false);
            LoginButton.SetActive(false);
        }
    }

    public static void BackToMenu() {
        SceneManager.LoadScene("Menu");
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
        //string request = "https://studenthome.hku.nl/~daniel.bergshoeff/KGDEV4/login.php?username=" + TextUsername.text + "&password=" + TextPassword.text;
        string request = "http://localhost/KGDEV4/login.php?username=" + TextUsername.text + "&password=" + TextPassword.text;
        StartCoroutine(Communication.GetRequest(request, (string returnedString) => {
            Uncode(returnedString);
        }));
    }

    public void Register() {
        string dob = dobyear.options[dobyear.value].text + dobmonth.options[dobmonth.value].text + dobday.options[dobday.value].text;
        DateTime dt;
        if (DateTime.TryParseExact(dob, "yyyyMMdd",
                          CultureInfo.InvariantCulture,
                          DateTimeStyles.None, out dt)) {
            //string request = "https://studenthome.hku.nl/~daniel.bergshoeff/KGDEV4/register.php?username=" + RegisterUsername.text + "&password=" + RegisterPassword.text + "&date_of_birth=" + dt;
            string request = "http://localhost/KGDEV4/register.php?username=" + RegisterUsername.text + "&password=" + RegisterPassword.text + "&date_of_birth=" + dt;
            StartCoroutine(Communication.GetRequest(request, (string returnedstring) => {
                Uncode(returnedstring);
            }));
        }
    }

    public void Edit() {
        string dob = dobyear.options[dobyear.value].text + dobmonth.options[dobmonth.value].text + dobday.options[dobday.value].text;
        DateTime dt;
        if (DateTime.TryParseExact(dob, "yyyyMMdd",
                          CultureInfo.InvariantCulture,
                          DateTimeStyles.None, out dt)) {
            //string request = "https://studenthome.hku.nl/~daniel.bergshoeff/KGDEV4/editinformation.php?sessid="+ userInfo.sessid + "&password=" + EditPassword.text;
            string request = "http://localhost/KGDEV4/editinformation.php?sessid="+ userInfo.sessid + "&password=" + EditPassword.text;
            StartCoroutine(Communication.GetRequest(request));
        }
    }

    public void SetHighScoresLastYear() {
        GetHighscoresBy(0, 0, 1);
    }

    public void SetHighScoresLastMonth() {
        GetHighscoresBy(0, 1, 0);
    }

    public void SetHighScoresLastWeek() {
        GetHighscoresBy(7, 0, 0);
    }

    public void SetHighScoresLastDay() {
        GetHighscoresBy(1, 0, 0);
    }

    public void SetHighScoresAllTime() {
        GetHighscoresBy(0, 0, 1000);
    }

    public void GetHighscoresBy(int days, int months, int years) {
        string request = "http://localhost/KGDEV4/gethighscores.php?gameid=0&daysago=" + days.ToString() + "&monthsago=" + months.ToString() + "&yearsago=" + years.ToString();
        GetHighscores(request);
    }


    private void GetHighscores(string request) {
        StartCoroutine(Communication.GetRequest(request, (String returnedstring) => {
            if (returnedstring != "0") {
                string jsonString = JsonHelper.FixJson(returnedstring);
                highscores = JsonHelper.FromJson<string>(jsonString);
                string s = "";
                for (int i = 0; i < Mathf.Clamp(highscores.Length, 0, 20); i += 3) {
                    s += highscores[i].ToUpper();
                    s += "\t\t" + highscores[i + 1];
                    s += "\t\t" + highscores[i + 2] + "\n";
                }

                TextHighscores.text = s;
            }
            else {
                TextHighscores.text = "There are no scores from this period!";
            }
        }));
    }

    private void Uncode(string json) {
        UserInfo ui = JsonUtility.FromJson<UserInfo>(json);
        if (ui != null) {
            if (ui.sessid != "0") {
                userInfo = ui;
                LoggedIn.SetActive(true);
                loggedIn = true;
                loginCanvas.SetActive(false);
                registerCanvas.SetActive(false);
                DisplayUsername.text = ui.username.ToUpper();
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

public static class JsonHelper {
    public static T[] FromJson<T>(string json) {
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
        return wrapper.Items;
    }

    public static string ToJson<T>(T[] array) {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper);
    }

    public static string ToJson<T>(T[] array, bool prettyPrint) {
        Wrapper<T> wrapper = new Wrapper<T>();
        wrapper.Items = array;
        return JsonUtility.ToJson(wrapper, prettyPrint);
    }

    public static string FixJson(string value) {
        value = "{\"Items\":" + value + "}";
        return value;
    }

    [Serializable]
    private class Wrapper<T> {
        public T[] Items;
    }
}
