using UnityEngine;

namespace _Project.Scripts.TrafficSystem
{
    public class TrafficCarIntersection : MonoBehaviour
    {
        public bool IsPassing { get; private set; }
    
        private void OnTriggerStay(Collider other)
        {
            if(!other.TryGetComponent(out TrafficCar _)){ return;}

            IsPassing = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if(!other.TryGetComponent(out TrafficCar _)){ return;}
        
            IsPassing = false;
        }
    }
}
