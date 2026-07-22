using UniRx;
using UnityEngine;

public class GameDataModel
{
    public IntReactiveProperty Score { get; } = new (0);
    public IntReactiveProperty Coins { get; } = new (0);
    public ReactiveCollection<CharacterType> PurchasedCharTypes { get; } = new();
    public ReactiveProperty<CharacterType> CurrentCharType { get; } = new (CharacterType.None);
    public BoolReactiveProperty IsInputLocked { get; } = new (true);    
    public ReactiveProperty<Vector3> LastCheckpointPosition { get; } = new (Vector3.zero);
    public ReactiveProperty<Quaternion> LastCheckpointRotation { get; } = new (Quaternion.identity);
    public BoolReactiveProperty IsLevelFinished { get; } = new(false);

    public GameDataModel()
    {
        Coins.Value = 100; //для теста
    }
}