using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class CharacterSlotUI : MonoBehaviour
{
    public Image IconImage;
    public TMP_Text NameText;
    public Button MainButton;
    public Button ActionButton;
    public TMP_Text ActionText;

    CharacterData _data;
    bool _isAssigned;
    Action<CharacterData, bool> _onAction;
    bool _actionVisible;

    public void Setup(CharacterData data, bool isAssigned, Action<CharacterData, bool> onAction)
    {
        _data = data;
        _isAssigned = isAssigned;
        _onAction = onAction;
        _actionVisible = false;

        IconImage.sprite = data.icon;
        NameText.text = data.displayName;
        ActionText.text = isAssigned ? "Remove" : "Select";
        ActionButton.gameObject.SetActive(false);

        MainButton.onClick.RemoveAllListeners();
        MainButton.onClick.AddListener(ToggleActionButton);

        ActionButton.onClick.RemoveAllListeners();
        ActionButton.onClick.AddListener(() => _onAction(_data, _isAssigned));
    }

    void ToggleActionButton()
    {
        _actionVisible = !_actionVisible;
        ActionButton.gameObject.SetActive(_actionVisible);
    }
}
