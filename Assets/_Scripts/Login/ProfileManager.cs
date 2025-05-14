using ExitGames.Client.Photon;
using Photon.Pun;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    [Header("UI Elements")]
    public TMP_InputField usernameInputField;
    public Button confirmUsernameButton;
    public Image playerAvatarImage;
    public Image menuAvatarImage;
    public TMP_Text usernameDisplayText; // TextMeshPro UI to show the username
    public TMP_Text menuDisplayText; // TextMeshPro UI to show the username

    [Header("Avatar Selection")]
    public GameObject avatarPrefab; // Prefab containing Image + Button
    public Transform avatarScrollViewContent; // Parent transform for avatar buttons
    public List<Sprite> avatarSprites; // List of avatars assigned in Inspector
    public Dictionary<int, int> playerAvatarIndices = new Dictionary<int, int>(); // Stores player avatars
    public Dictionary<int, string> playerNames = new Dictionary<int, string>(); // Stores player names
    public int selectedAvatarIndex = 0;

    public GameObject usernamePanel, avatarPanel;

    private void Start()
    {
        LoadProfile();
        confirmUsernameButton.onClick.AddListener(UpdateUsername);
    }

    /// <summary>
    /// Loads the current username and avatar from PlayFab.
    /// </summary>
    public void LoadProfile()
    {
        PlayFabClientAPI.GetAccountInfo(new GetAccountInfoRequest(), OnGetAccountInfoSuccess, OnError);
        PlayFabClientAPI.GetUserData(new GetUserDataRequest(), OnUserDataReceived, OnError);
        GenerateAvatars();
    }

    private void OnGetAccountInfoSuccess(GetAccountInfoResult result)
    {
        if (!string.IsNullOrEmpty(result.AccountInfo.TitleInfo.DisplayName))
        {
            usernameInputField.text = result.AccountInfo.TitleInfo.DisplayName;
            usernameDisplayText.text = result.AccountInfo.TitleInfo.DisplayName; // Update UI text
            menuDisplayText.text = result.AccountInfo.TitleInfo.DisplayName;
            PhotonNetwork.LocalPlayer.NickName = result.AccountInfo.TitleInfo.DisplayName;
        }
    }

    private void OnUserDataReceived(GetUserDataResult result)
    {
        if (result.Data.ContainsKey("AvatarIndex"))
        {
            selectedAvatarIndex = int.Parse(result.Data["AvatarIndex"].Value);
            Hashtable playerProperties = new Hashtable();
            playerProperties["AvatarSelectedIndex"] = selectedAvatarIndex;

            // Set the custom properties for the local player.
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProperties);
            UpdateAvatarUI();
        }
    }

    /// <summary>
    /// Generates avatar selection buttons in the ScrollView.
    /// </summary>
    private void GenerateAvatars()
    {
        foreach (Transform child in avatarScrollViewContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < avatarSprites.Count; i++)
        {
            GameObject avatarButton = Instantiate(avatarPrefab, avatarScrollViewContent);
            Image avatarImage = avatarButton.transform.GetChild(0).GetComponent<Image>(); // Get child Image component
            Button button = avatarButton.GetComponent<Button>();

            if (avatarImage != null)
            {
                avatarImage.sprite = avatarSprites[i];
            }

            int index = i; // Prevent closure issue
            button.onClick.AddListener(() => SelectAvatar(index));
        }
    }

    /// <summary>
    /// Selects an avatar when clicked.
    /// </summary>
    public void SelectAvatar(int index)
    {
        if (index >= 0 && index < avatarSprites.Count)
        {
            selectedAvatarIndex = index;
            UpdateAvatarUI();
            usernamePanel.SetActive(false);
            avatarPanel.SetActive(false);
            SaveProfileChanges();
        }
    }

    /// <summary>
    /// Updates the avatar display.
    /// </summary>
    private void UpdateAvatarUI()
    {
        playerAvatarImage.sprite = avatarSprites[selectedAvatarIndex];
        menuAvatarImage.sprite = avatarSprites[selectedAvatarIndex];
    }

    /// <summary>
    /// Saves the updated username and avatar selection.
    /// </summary>
    public void SaveProfileChanges()
    {
        UpdateAvatar();
    }

    /// <summary>
    /// Updates the username in PlayFab.
    /// </summary>
    public void UpdateUsername()
    {
        string newUsername = usernameInputField.text;

        var request = new UpdateUserTitleDisplayNameRequest
        {
            DisplayName = newUsername
        };

        PopupManager.Instance.ShowLoadingBar();
        PopupManager.Instance.UpdateLoadingBarText("Updating name");


        PlayFabClientAPI.UpdateUserTitleDisplayName(request, OnUsernameUpdated, OnError);
    }

    private void OnUsernameUpdated(UpdateUserTitleDisplayNameResult result)
    {
        PopupManager.Instance.HideLoadingBar(() =>
        {
            Debug.Log("Username successfully updated.");
            usernameDisplayText.text = usernameInputField.text; // Update the TextMeshPro UI
            menuDisplayText.text = usernameInputField.text;
            PopupManager.Instance.ShowNotificationPopup("Username updated successfully!");
            usernamePanel.SetActive(false);
            avatarPanel.SetActive(false);
        });

    }

    /// <summary>
    /// Saves the selected avatar to PlayFab.
    /// </summary>
    public void UpdateAvatar()
    {
        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string>
            {
                { "AvatarIndex", selectedAvatarIndex.ToString() }
            }
        };
        PopupManager.Instance.ShowLoadingBar();
        PopupManager.Instance.UpdateLoadingBarText("Changing Avatar");
        PlayFabClientAPI.UpdateUserData(request, OnAvatarUpdated, OnError);
    }

    private void OnAvatarUpdated(UpdateUserDataResult result)
    {
        PopupManager.Instance.HideLoadingBar();
        Debug.Log("Avatar successfully updated.");
    }

    private void OnError(PlayFabError error)
    {
        Debug.LogError("Error: " + error.GenerateErrorReport());
        PopupManager.Instance.ShowNotificationPopup("Error: " + error.GenerateErrorReport());
    }
    /// <summary>
    /// Sets the player's avatar index in the dictionary.
    /// </summary>
    public void SetPlayerAvatar(int playerActorNumber, int avatarIndex)
    {
        if (!playerAvatarIndices.ContainsKey(playerActorNumber))
            playerAvatarIndices.Add(playerActorNumber, avatarIndex);
        else
            playerAvatarIndices[playerActorNumber] = avatarIndex;
    }

    /// <summary>
    /// Retrieves the correct avatar for a given player.
    /// </summary>
    public Sprite GetAvatarForPlayer(int playerActorNumber)
    {
        if (playerAvatarIndices.ContainsKey(playerActorNumber))
        {
            int avatarIndex = playerAvatarIndices[playerActorNumber];
            return avatarSprites[avatarIndex]; // ✅ Return correct sprite
        }
        return null;
    }

    /// <summary>
    /// Sets the player's name in the dictionary.
    /// </summary>
    public void SetPlayerName(int playerActorNumber, string playerName)
    {
        if (!playerNames.ContainsKey(playerActorNumber))
            playerNames.Add(playerActorNumber, playerName);
        else
            playerNames[playerActorNumber] = playerName;
    }

    /// <summary>
    /// Gets the player's name from the dictionary.
    /// </summary>
    public string GetPlayerName(int playerActorNumber)
    {
        if (playerNames.ContainsKey(playerActorNumber))
            return playerNames[playerActorNumber];
        return "Unknown Player";
    }

}
