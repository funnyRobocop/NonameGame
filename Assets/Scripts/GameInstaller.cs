using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{

    [SerializeField] private GameUIView _gameUIView; 
    [SerializeField] private CameraSwitcher _cameraSwitcher; 

    public override void InstallBindings()
    {
        Container.Bind<CharacterFactory>().AsSingle();
        Container.Bind<GameUIView>().FromInstance(_gameUIView).AsSingle();
        Container.Bind<CameraSwitcher>().FromInstance(_cameraSwitcher).AsSingle();

        // Говорим: когда кто-то запросит PlayerInit, создай объект из этого префаба, 
        // внедри в него зависимости (заинжекть модель) и верни компонент
        //Container.Bind<PlayerInit>().FromComponentInNewPrefab(_playerPrefab).AsSingle();
    }
}