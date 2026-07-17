using Zenject;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        // Переводим: "Создай один экземпляр GameDataModel на всю сцену 
        // и внедряй его везде, где его попросят через [Inject]"
        Container
            .Bind<GameDataModel>()
            .AsSingle()
            .NonLazy(); // Создавать сразу при старте сцены, а не при первом запросе
    }
}