using Zenject;

public class ProjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<GameDataModel>().AsSingle().NonLazy();
    }
}
