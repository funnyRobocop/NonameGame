using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Configs/CharacterDatabase")]
public class CharacterDatabase : ScriptableObject
{
    public List<CharacterTypeData> AllCharacterTypes;

    public CharacterTypeData GetCharacterTypeData(CharacterType charType)
    {
        return AllCharacterTypes.Find(c => c.CharacterType == charType);
    }
}
