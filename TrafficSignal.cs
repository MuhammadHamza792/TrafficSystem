using UnityEngine;

namespace _Project.Scripts.TrafficSystem
{
    public class TrafficSignal : MonoBehaviour
    {
        [SerializeField] private Collider _collider;
        private ITrafficCarMovement _stoppedCar;
    
        public Collider GetCollider => _collider;

        private void OnTriggerEnter(Collider other)
        {
            if(!other.TryGetComponent(out ITrafficCarMovement carMovement)){ return;}
        
            if(!GetCollider.enabled) { return; }

            _stoppedCar = carMovement;
            _stoppedCar.StopCar();
        }
    
        public void PaceUpCar()
        {
            _stoppedCar?.PaceUpSpeed();
        }
    }
}
