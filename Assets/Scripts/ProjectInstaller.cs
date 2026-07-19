using Zenject;

public class ProjectInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Переносим бинд модели сюда!
        // Теперь она будет создана ОДИН раз при запуске игры 
        // и останется живой между всеми сценами.
        Container
            .Bind<GameDataModel>()
            .AsSingle()
            .NonLazy();
    }
}
