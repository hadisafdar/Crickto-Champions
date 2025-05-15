using UnityEngine;

public enum CharacterType { Bowler, Batsman }

[CreateAssetMenu(fileName = "CharacterData", menuName = "Cricket/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string characterId;
    public CharacterType characterType;
    public string displayName;
    public Sprite icon;
    public GameObject prefab;
}
