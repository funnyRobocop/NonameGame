using UnityEngine;
using Fusion;

public class NetworkObstacleRotator : NetworkBehaviour
{
    [SerializeField] private float speed = 100f;

    public override void FixedUpdateNetwork()
    {
        // Runner.SimulationTime идеально одинаковый на ПК у ВСЕХ игроков в лобби
        float currentAngle = Runner.SimulationTime * speed;
        
        // Вращаем платформу локально. Физика Unity на каждом клиенте сама поймет, 
        // как крутить стоящую сверху корову, без использования тяжелого DOTween
        transform.localRotation = Quaternion.Euler(0, currentAngle, 0);
    }
}
