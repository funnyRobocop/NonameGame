using UnityEngine;

[CreateAssetMenu(fileName = "CharacterTypeData", menuName = "Configs/CharacterTypeData")]
public class CharacterTypeData : ScriptableObject
{
    public CharacterType CharacterType;
    public string AdressableKey;
}

public enum CharacterType
{
    None = 0,
    CatRed = 1,
    RoosterRed = 2,
    RacoonBrown = 3,
    CowWhite = 4,
    SheepWhite = 5,
    ZebraWhite = 6,
    BearBrown = 7
}
