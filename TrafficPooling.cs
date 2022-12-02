using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace _Project.Scripts.TrafficSystem
{
    public class TrafficPooling : MonoBehaviour
    {
        [SerializeField] private float radius;

        private List<ICarActivator> _carsInRadius;

        private Transform _transform;
        
        private void Awake()
        {
            _transform = transform;
            _carsInRadius = new List<ICarActivator>();
        }

        private void Start()
        {
            StartCoroutine(DetectTargetWithDelay());
        }

        private IEnumerator DetectTargetWithDelay()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.2f);
                FindVisibleTargets();
            }
        }

        private void FindVisibleTargets()
        {
            var position = _transform.position;
            
            var targetsInRadius = Physics.OverlapSphere(position, radius);

            foreach (var target in targetsInRadius)
            {
                if(!target.TryGetComponent(out ICarActivator car)) continue;
                
                CheckIfCarIsOutOfRadius(targetsInRadius);
                
                AddCarPresentInRadius(car);

                car.ActivateCar();
            }
        }

        private void AddCarPresentInRadius(ICarActivator car)
        {
            if (!_carsInRadius.Contains(car))
                _carsInRadius.Add(car);
        }

        private void CheckIfCarIsOutOfRadius(Collider[] cars)
        {
            if(_carsInRadius.Count == 0) return;

            foreach (var carInRadius in _carsInRadius.Where(carInRadius => !cars.Contains(carInRadius._collider)))
            {
                carInRadius.DeactivateCar();
            }
        }
    }
}
