using UniRx;
using UnityEngine;

public class GameDataModel
{
    public IntReactiveProperty Score { get; } = new (0);
    public IntReactiveProperty Coins { get; } = new (0);
    public ReactiveCollection<CharType> PurchasedCharTypes { get; } = new();
    public ReactiveProperty<CharType> CurrentCharType { get; } = new (CharType.None);
    public BoolReactiveProperty IsInputLocked { get; } = new (true);    
    public ReactiveProperty<Vector3> LastCheckpointPosition { get; } = new (Vector3.zero);
    public ReactiveProperty<Quaternion> LastCheckpointRotation { get; } = new (Quaternion.identity);
    public BoolReactiveProperty IsLevelFinished { get; } = new(false);

    public GameDataModel()
    {
        Coins.Value = 100; //для теста
    }
}

public enum CharType
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