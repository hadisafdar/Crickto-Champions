using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Collections.Generic;
using Photon.Pun;                // for PhotonNetwork
using ExitGames.Client.Photon;   // for Hashtable

public class CharacterSelectManager : MonoBehaviour
{
    [Header("Data")]
    public List<CharacterData> bowlers;    // expect one entry with characterId == "0" to represent “none”
    public List<CharacterData> batsmen;

    [Header("UI Prefab")]
    public GameObject slotPrefab;

    [Header("Panels")]
    public GameObject bowlerPanel;
    public GameObject batsmanPanel;

    [Header("Tab Buttons")]
    public Button bowlerTabButton;
    public Button batsmanTabButton;

    [Header("Bowler Containers")]
    public Transform bowlerUnassignedGrid;
    public Transform bowlerAssignedGrid;

    [Header("Batsman Containers")]
    public Transform batsmanUnassignedGrid;
    public Transform batsmanAssignedGrid;

    string selectedBowlerId;
    string selectedBatsmanId;

    const string PP_BOWLER_KEY = "SelectedBowlerID";
    const string PP_BATSMAN_KEY = "SelectedBatsmanID";


    public static CharacterSelectManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }


    void Start()
    {
        // --- load from PlayerPrefs (defaults to "0") ---
        selectedBowlerId = PlayerPrefs.GetString(PP_BOWLER_KEY, "0");
        selectedBatsmanId = PlayerPrefs.GetString(PP_BATSMAN_KEY, "0");

        // --- if first-time (== "0"), pick list[0] as your default ---
        if (selectedBowlerId == "0" && bowlers.Count > 0)
        {
            selectedBowlerId = bowlers[0].characterId;
            PlayerPrefs.SetString(PP_BOWLER_KEY, selectedBowlerId);
            PlayerPrefs.Save();
        }
        if (selectedBatsmanId == "0" && batsmen.Count > 0)
        {
            selectedBatsmanId = batsmen[0].characterId;
            PlayerPrefs.SetString(PP_BATSMAN_KEY, selectedBatsmanId);
            PlayerPrefs.Save();
        }

        // --- sync into Photon so others see your default choice too ---
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {
        { "BowlerID",  selectedBowlerId  },
        { "BatsmanID", selectedBatsmanId }
    });

        // --- UI setup as before ---
        bowlerTabButton.onClick.AddListener(() => SwitchPanel(true));
        batsmanTabButton.onClick.AddListener(() => SwitchPanel(false));
        SwitchPanel(true);
        RefreshBowlerUI();
        RefreshBatsmanUI();
    }

    void SwitchPanel(bool showBowler)
    {
        bowlerPanel.SetActive(showBowler);
        batsmanPanel.SetActive(!showBowler);
    }

    #region — Bowler UI —
    void RefreshBowlerUI()
    {
        // clear old
        foreach (Transform t in bowlerUnassignedGrid) Destroy(t.gameObject);
        foreach (Transform t in bowlerAssignedGrid) Destroy(t.gameObject);

        // assigned: only if not "0"
        if (selectedBowlerId != "0")
        {
            var cd = bowlers.First(c => c.characterId == selectedBowlerId);
            var slot = Instantiate(slotPrefab, bowlerAssignedGrid)
                           .GetComponent<CharacterSlotUI>();
            slot.Setup(cd, true, OnBowlerClicked);
        }

        // unassigned: everything except your assigned one
        foreach (var cd in bowlers.Where(c => c.characterId != selectedBowlerId))
        {
            var slot = Instantiate(slotPrefab, bowlerUnassignedGrid)
                           .GetComponent<CharacterSlotUI>();
            slot.Setup(cd, false, OnBowlerClicked);
        }
    }

    void OnBowlerClicked(CharacterData data, bool wasAssigned)
    {
        // toggle between data.characterId and "0"
        selectedBowlerId = wasAssigned ? "0" : data.characterId;
        Debug.Log(selectedBowlerId);
        // 1) Save locally
        PlayerPrefs.SetString(PP_BOWLER_KEY, selectedBowlerId);
        PlayerPrefs.Save();

        // 2) Save to Photon
        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {
            { "BowlerID", selectedBowlerId }
        });

        // 3) Rebuild UI
        RefreshBowlerUI();
    }
    #endregion

    #region — Batsman UI —
    void RefreshBatsmanUI()
    {
        foreach (Transform t in batsmanUnassignedGrid) Destroy(t.gameObject);
        foreach (Transform t in batsmanAssignedGrid) Destroy(t.gameObject);

        if (selectedBatsmanId != "0")
        {
            var cd = batsmen.First(c => c.characterId == selectedBatsmanId);
            var slot = Instantiate(slotPrefab, batsmanAssignedGrid)
                           .GetComponent<CharacterSlotUI>();
            slot.Setup(cd, true, OnBatsmanClicked);
        }

        foreach (var cd in batsmen.Where(c => c.characterId != selectedBatsmanId))
        {
            var slot = Instantiate(slotPrefab, batsmanUnassignedGrid)
                           .GetComponent<CharacterSlotUI>();
            slot.Setup(cd, false, OnBatsmanClicked);
        }
    }

    void OnBatsmanClicked(CharacterData data, bool wasAssigned)
    {
        selectedBatsmanId = wasAssigned ? "0" : data.characterId;

        Debug.Log(selectedBatsmanId);
        PlayerPrefs.SetString(PP_BATSMAN_KEY, selectedBatsmanId);
        PlayerPrefs.Save();

        PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {
            { "BatsmanID", selectedBatsmanId }
        });

        RefreshBatsmanUI();
    }
    #endregion


    /// <summary>
    /// Returns true if both a bowler and a batsman have been selected (i.e. neither is "0").
    /// </summary>
    /// 
    public bool IsSelectionComplete()
    {
        bool bowlerChosen = !string.IsNullOrEmpty(selectedBowlerId) && selectedBowlerId != "0";
        bool batsmanChosen = !string.IsNullOrEmpty(selectedBatsmanId) && selectedBatsmanId != "0";
        Debug.Log(bowlerChosen && batsmanChosen);
        return bowlerChosen && batsmanChosen;
    }
   
}
