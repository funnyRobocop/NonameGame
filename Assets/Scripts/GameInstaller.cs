using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{

    [SerializeField] private PlayerInit _playerInit; 
    [SerializeField] private FinishZoneTrigger _finishZone; 

    public override void InstallBindings()
    {
        Container.Bind<GameDataModel>().AsSingle().NonLazy();

        Container.Bind<PlayerInit>().FromInstance(_playerInit).AsSingle();
        //Container.Bind<FinishZoneTrigger>().FromInstance(_finishZone).AsSingle();

        // Говорим: когда кто-то запросит PlayerInit, создай объект из этого префаба, 
        // внедри в него зависимости (заинжекть модель) и верни компонент
        //Container.Bind<PlayerInit>().FromComponentInNewPrefab(_playerPrefab).AsSingle();
    }
}