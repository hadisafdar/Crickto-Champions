using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Photon.Pun;

public class LoginRegistrationManager : MonoBehaviour
{

    public static LoginRegistrationManager Instance { get; private set; }


    private void Awake()
    {
        Instance = this;
    }
    [Header("Login")]
    public TMP_InputField loginEmail;
    public TMP_InputField loginPassword;
    public Toggle rememberMeToggle;

    [Header("Registration")]
    public TMP_InputField registerName;
    public TMP_InputField registerEmail;
    public TMP_InputField registerPassword;

    [Header("Registration")]
    
    public TMP_InputField forgetEmail;
   

    [Header("UI Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;
    //public GameObject registerButton, forgetButton, loginButton;

    private const string RememberMeKey = "RememberMe";
    private const string SavedEmailKey = "SavedEmail";
    private const string SavedPasswordKey = "SavedPassword";

    private void Start()
    {
        Application.targetFrameRate = 60;
        CheckAutoLogin();
    }

    /// <summary>
    /// Checks if the user has "Remember Me" enabled and auto-logs in.
    /// </summary>
    public void CheckAutoLogin()
    {
        if (PlayerPrefs.GetInt(RememberMeKey, 0) == 1)
        {
            string savedEmail = PlayerPrefs.GetString(SavedEmailKey, "");
            string savedPassword = PlayerPrefs.GetString(SavedPasswordKey, "");

            if (!string.IsNullOrEmpty(savedEmail) && !string.IsNullOrEmpty(savedPassword))
            {
                loginEmail.text = savedEmail;
                loginPassword.text = savedPassword;
                rememberMeToggle.isOn = true;
                Login(); // Auto Login
                registerPanel.SetActive(false);
                loginPanel.SetActive(false);
               // loginButton.SetActive(false);
               // registerButton.SetActive(false);
               // forgetButton.SetActive(false);
                
            }
            else
            {
                
            }
        }
        else if (PlayerPrefs.GetInt("RegisteredSuccess", 0) != 1)
        {
            //navigation.OpenRegisterPanelFirst();
            registerPanel.SetActive(true);
           // loginButton.SetActive(true);
            loginPanel.SetActive(false); 
            //registerButton.SetActive(false);
           //forgetButton.SetActive(false);
        }
        else
        {
            registerPanel.SetActive(false);
            loginPanel.SetActive(true);
            //registerButton.SetActive(true);
            //forgetButton.SetActive(true);
        }
    }

    /// <summary>
    /// Returns whether auto-login is enabled.
    /// </summary>
    public bool IsAutoLoginEnabled()
    {
        return PlayerPrefs.GetInt(RememberMeKey, 0) == 1;
    }

    public void Login()
    {
        if (IsValidEmail(loginEmail.text))
        {
            var request = new LoginWithEmailAddressRequest
            {
                Email = loginEmail.text,
                Password = loginPassword.text
            };

            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnLoginFailure);
            PopupManager.Instance.ShowLoadingBar();
            PopupManager.Instance.UpdateLoadingBarText("Validating");
        }
        else
        {
            Debug.LogError("Invalid email address.");
            PopupManager.Instance.ShowNotificationPopup("Invalid email address.");
        }
    }

    private void OnLoginSuccess(LoginResult result)
    {
        PopupManager.Instance.UpdateLoadingBarText("Login Success");
        PopupManager.Instance.HideLoadingBar(() =>
        {
            Debug.Log("Login Successful!");
            //PopupManager.Instance.ShowNotificationPopup("Login Successful!");
            string playfabId = result.PlayFabId;

            PlayerPrefs.SetString("ID", playfabId);
            registerPanel.SetActive(false);
            loginPanel.SetActive(false);
            //navigation.OpenMainMenuPanel();

            if (rememberMeToggle.isOn)
            {
                PlayerPrefs.SetInt(RememberMeKey, 1);
                PlayerPrefs.SetString(SavedEmailKey, loginEmail.text);
                PlayerPrefs.SetString(SavedPasswordKey, loginPassword.text);
            }
            else
            {
                PlayerPrefs.SetInt(RememberMeKey, 0);
                PlayerPrefs.DeleteKey(SavedEmailKey);
                PlayerPrefs.DeleteKey(SavedPasswordKey);
            }
            //ProfileManager.Instance.LoadProfile();
            PlayerPrefs.Save();
            GetPlayFabDisplayName();
            SceneManager.LoadScene("Main Menu");
        });
        
    }

    private void OnLoginFailure(PlayFabError error)
    {
        PopupManager.Instance.HideLoadingBar();
        Debug.LogError("Login Failed: " + error.ErrorMessage);
        PopupManager.Instance.ShowNotificationPopup("Login Failed: " + error.ErrorMessage);
    }

    public void Register()
    {
        if (IsValidEmail(registerEmail.text))
        {
            var request = new RegisterPlayFabUserRequest
            {
                Email = registerEmail.text,
                Password = registerPassword.text,
                DisplayName = registerName.text,
                RequireBothUsernameAndEmail = false
            };

            PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnRegisterFailure);
            PopupManager.Instance.ShowLoadingBar();
            PopupManager.Instance.UpdateLoadingBarText("Validating");
        }
        else
        {
            Debug.LogError("Invalid email address.");
            PopupManager.Instance.ShowNotificationPopup("Invalid email address.");
        }
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        PopupManager.Instance.UpdateLoadingBarText("Registration Success");
        PopupManager.Instance.HideLoadingBar(() =>
        {
            Debug.Log("Registration Successful!");
            //PopupManager.Instance.ShowNotificationPopup("Registration Successful!");
            PlayerPrefs.SetInt("RegisteredSuccess", 1);
            registerPanel.SetActive(false);
            PlayerPrefs.Save();
            //navigation.OpenMainMenuPanel();
            //loginPanel.SetActive(true);
            SceneManager.LoadScene("Main Menu");

            GetPlayFabDisplayName();
        });
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        PopupManager.Instance.HideLoadingBar();
        Debug.LogError("Registration Failed: " + error.ErrorMessage);
        PopupManager.Instance.ShowNotificationPopup("Registration Failed: " + error.ErrorMessage);
    }

    private bool IsValidEmail(string email)
    {
        string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, emailPattern);
    }

    public void GetPlayFabDisplayName()
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), OnGetAccountInfoSuccess, OnGetAccountInfoError);
    }

    private void OnGetAccountInfoSuccess(GetAccountInfoResult result)
    {
        string displayName = result.AccountInfo.TitleInfo.DisplayName;
        if (!string.IsNullOrEmpty(displayName))
        {
            Debug.Log("PlayFab Display Name: " + displayName);
            PhotonNetwork.LocalPlayer.NickName = displayName;
        }
        else
        {
            Debug.LogWarning("Display Name is not set for this player.");
        }
    }

    private void OnGetAccountInfoError(PlayFabError error)
    {
        Debug.LogError("Failed to get account info: " + error.GenerateErrorReport());
    }

    /// <summary>
    /// Sends a password reset email to the user.
    /// </summary>
    public void ForgotPassword()
    {
        if (IsValidEmail(forgetEmail.text))
        {
            var request = new SendAccountRecoveryEmailRequest
            {
                Email = forgetEmail.text,
                TitleId = PlayFabSettings.TitleId
            };
            PopupManager.Instance.ShowLoadingBar();
            PopupManager.Instance.UpdateLoadingBarText("Sending mail");
            PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordResetSuccess, OnPasswordResetError);
        }
        else
        {
            Debug.LogError("Invalid email address.");
            PopupManager.Instance.ShowNotificationPopup("Invalid email address.");
        }
    }

    private void OnPasswordResetSuccess(SendAccountRecoveryEmailResult result)
    {
        PopupManager.Instance.UpdateLoadingBarText("Mail Sent");
        PopupManager.Instance.HideLoadingBar();
         Debug.Log("Password reset email sent.");
        PopupManager.Instance.ShowNotificationPopup("Password reset email sent. Check your inbox!");
    }

    private void OnPasswordResetError(PlayFabError error)
    {
        PopupManager.Instance.HideLoadingBar();
        Debug.LogError("Failed to send password reset email: " + error.GenerateErrorReport());
        PopupManager.Instance.ShowNotificationPopup("Failed to send password reset email.");
    }

    void OnUserDataReceived(GetUserDataResult result)
    {
        if (result.Data.ContainsKey("DisplayName"))
        {
            PhotonNetwork.LocalPlayer.NickName = result.Data["DisplayName"].Value;
        }
        else
        {
            Debug.LogWarning("DisplayName data not found.");
        }
    }

    void OnUserDataFailed(PlayFabError error)
    {
        Debug.LogError("Failed to get user data: " + error.ErrorMessage);
    }
}
