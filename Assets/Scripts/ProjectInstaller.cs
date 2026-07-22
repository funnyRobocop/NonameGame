using UnityEngine;
using Zenject;

public class ProjectInstaller : MonoInstaller
{
    [SerializeField] private CharacterDatabase _characterDatabase;
    public override void InstallBindings()
    {
        Container.BindInstance(_characterDatabase).AsSingle();
        Container.Bind<GameDataModel>().AsSingle().NonLazy();
    }
}
