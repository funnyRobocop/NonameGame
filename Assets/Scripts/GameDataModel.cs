using UniRx;
using UnityEngine;

public class GameDataModel
{
    // ReactiveProperty автоматически уведомляет всех, кто на него подписался, при изменении значения
    public IntReactiveProperty Score { get; } = new (0);
    public IntReactiveProperty Coins { get; } = new (100);

    // Для списков используем ReactiveCollection — она уведомляет о добавлении/удалении элементов
    public ReactiveCollection<string> PurchasedSkins { get; } = new();
    
    // Текущий экипированный скин
    public ReactiveProperty<string> CurrentSkinAddress { get; } = new ("default_skin");
    public BoolReactiveProperty IsInputLocked { get; } = new (true);
    
    public ReactiveProperty<Vector3> LastCheckpointPosition { get; } = new (Vector3.zero);
    public ReactiveProperty<Quaternion> LastCheckpointRotation { get; } = new (Quaternion.identity);

    public GameDataModel()
    {
        // Сразу добавим дефолтный скин в список купленных
        if (!PurchasedSkins.Contains("default_skin"))
        {
            PurchasedSkins.Add("default_skin");
        }
    }
}