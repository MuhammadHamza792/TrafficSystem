using System.Collections;
using UnityEngine;

namespace _Project.Scripts.TrafficSystem
{
    public class CarActivator : MonoBehaviour,ICarActivator
    {
        [SerializeField] private TrafficCar currentCar;
        [SerializeField] private TrafficCarMovement carMovement;
        
        private Vector3 _initialPosition;
        private Quaternion _initialRotation;
        private float _currentSpeed;

        private bool _deactivated;
        private bool _activated;
        
        public Collider _collider { get; set; }
    
        private void Start()
        {
            _collider = GetComponent<Collider>();
            currentCar.enabled = false;
            _initialPosition = currentCar.transform.position;
            _initialRotation = currentCar.transform.rotation;
        }
    
        public void ActivateCar()
        {
            if(_activated) return;
            currentCar.enabled = true;
            RunOnceCheck(true);
            currentCar.SetCurrentIndex(currentCar.StartIndex);
            carMovement.CarSpeed = _currentSpeed;
        }
        
        public void DeactivateCar()
        {
            if(_deactivated) return;
            currentCar.enabled = false;
            RunOnceCheck(false);
            currentCar.transform.SetPositionAndRotation(_initialPosition, _initialRotation);
            _currentSpeed = carMovement.CarSpeed;
        }
        
        private void RunOnceCheck(bool active)
        {
            _deactivated = !active;
            _activated = active;
        }
        
    }
}