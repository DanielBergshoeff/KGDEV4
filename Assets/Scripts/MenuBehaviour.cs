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
    public Text TextGameInfo;

    public GameObject LoginToPlayText;
    public GameObject LoggedInAsText;
    public GameObject LoginButton;
    public GameObject LoggedIn;

    public Dropdown DobYear;
    public Dropdown DobMonth;
    public Dropdown DobDay;

    public GameObject LoginCanvas;
    public GameObject RegisterCanvas;
    public static UserInfo userInfo;

    private bool loggedIn = false;
    private string[] highscores;
    private string[] gameinfo;

    private void Start() {
        DobYear.options.Clear();
        for (int i = 2019; i > 1900; i--) {
            DobYear.options.Add(new Dropdown.OptionData(i.ToString()));
        }

        DobMonth.options.Clear();
        for (int i = 1; i < 13; i++) {
            DobMonth.options.Add(new Dropdown.OptionData(i.ToString("00")));
        }

        DobDay.options.Clear();
        for (int i = 1; i < 32; i++) {
            DobDay.options.Add(new Dropdown.OptionData(i.ToString("00")));
        }

        GetHighscoresBy("0,0,1000");
        GetGameInfoBy("0,0,1000");

        if(userInfo != null) {
            LoggedIn.SetActive(true);
            loggedIn = true;
            LoginCanvas.SetActive(false);
            RegisterCanvas.SetActive(false);
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
        LoginCanvas.SetActive(true);
    }

    public void OpenRegisterMenu() {
        LoginCanvas.SetActive(false);
        RegisterCanvas.SetActive(true);
    }

    public void Login() {
        string request = "login.php?username=" + TextUsername.text + "&password=" + TextPassword.text;
        StartCoroutine(Communication.GetRequest(request, (string returnedString) => {
            Uncode(returnedString);
        }));
    }

    public void GetGameInfoBy(string daysmonthsyears) {
        //int days, int months, int years
        string[] splittedParams = daysmonthsyears.Split(',');

        int days = int.Parse(splittedParams[0]);
        int months = int.Parse(splittedParams[1]);
        int years = int.Parse(splittedParams[2]);

        string request = "getgameinfo.php?daysago=" + days.ToString() + "&monthsago=" + months.ToString() + "&yearsago=" + years.ToString();
        GetGameInfo(request);
    }

    private void GetGameInfo(string request) {
        StartCoroutine(Communication.GetRequest(request, (String returnedstring) => {
            if (returnedstring != "0") {
                string jsonString = JsonHelper.FixJson(returnedstring);
                gameinfo = JsonHelper.FromJson<string>(jsonString);
                string s = "";
                for (int i = 0; i < Mathf.Clamp(gameinfo.Length, 0, 20); i += 2) {
                    s += gameinfo[i].ToUpper();
                    s += "\t\t" + gameinfo[i + 1] + "\n";
                }

                TextGameInfo.text = s;
            }
            else {
                TextGameInfo.text = "There are no games from this period!";
            }
        }));
    }

    public void Register() {
        string dob = DobYear.options[DobYear.value].text + "-" + DobMonth.options[DobMonth.value].text + "-" + DobDay.options[DobDay.value].text;
        string request = "register.php?username=" + RegisterUsername.text + "&password=" + RegisterPassword.text + "&date_of_birth=" + dob;
        StartCoroutine(Communication.GetRequest(request, (string returnedstring) => {
            Uncode(returnedstring);
        }));
    }

    public void Edit() {
        string dob = DobYear.options[DobYear.value].text + DobMonth.options[DobMonth.value].text + DobDay.options[DobDay.value].text;
        DateTime dt;
        if (DateTime.TryParseExact(dob, "yyyyMMdd",
                          CultureInfo.InvariantCulture,
                          DateTimeStyles.None, out dt)) {
            string request = "editinformation.php?sessid="+ userInfo.sessid + "&password=" + EditPassword.text;
            StartCoroutine(Communication.GetRequest(request));
        }
    }

    public void GetHighscoresBy(string daysmonthsyears) {
        //int days, int months, int years
        string[] splittedParams = daysmonthsyears.Split(',');
        
        int days = int.Parse(splittedParams[0]);
        int months = int.Parse(splittedParams[1]);
        int years = int.Parse(splittedParams[2]);


        string request = "gethighscores.php?gameid=0&daysago=" + days.ToString() + "&monthsago=" + months.ToString() + "&yearsago=" + years.ToString();
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
                LoginCanvas.SetActive(false);
                RegisterCanvas.SetActive(false);
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
