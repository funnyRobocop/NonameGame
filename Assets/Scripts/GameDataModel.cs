using UniRx;
using UnityEngine;

public class GameDataModel
{
    public IntReactiveProperty Score { get; } = new (0);
    public IntReactiveProperty Coins { get; } = new (100);
    public ReactiveCollection<string> PurchasedSkins { get; } = new();
    public ReactiveProperty<string> CurrentSkinAddress { get; } = new ("default_skin");
    public BoolReactiveProperty IsInputLocked { get; } = new (true);    
    public ReactiveProperty<Vector3> LastCheckpointPosition { get; } = new (Vector3.zero);
    public ReactiveProperty<Quaternion> LastCheckpointRotation { get; } = new (Quaternion.identity);
    public BoolReactiveProperty IsLevelFinished { get; } = new (false);

    public GameDataModel()
    {
        // Сразу добавим дефолтный скин в список купленных
        if (!PurchasedSkins.Contains("default_skin"))
        {
            PurchasedSkins.Add("default_skin");
        }
    }
}