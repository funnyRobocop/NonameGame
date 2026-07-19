using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{

    [SerializeField] private PlayerInit _playerInit; 
    [SerializeField] private GameUIView _gameUIView; 

    public override void InstallBindings()
    {
        Container.Bind<GameDataModel>().AsSingle().NonLazy();

        Container.Bind<PlayerInit>().FromInstance(_playerInit).AsSingle();
        Container.Bind<GameUIView>().FromInstance(_gameUIView).AsSingle();

        // Говорим: когда кто-то запросит PlayerInit, создай объект из этого префаба, 
        // внедри в него зависимости (заинжекть модель) и верни компонент
        //Container.Bind<PlayerInit>().FromComponentInNewPrefab(_playerPrefab).AsSingle();
    }
}