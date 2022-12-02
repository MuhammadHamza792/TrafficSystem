using System;
using _Project.Scripts.Game;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.TrafficSystem
{
    public class TrafficCarDetection : MonoBehaviour
    {
        private List<ITrafficCarMovement> _cars;
        private ITrafficCarMovement _carMovement;
        private bool _isSlowedDown;
        private WaitForSeconds _secondsToWait = new WaitForSeconds(2);
        public static event Action<ITrafficCarMovement> OnObstacaleEncounter;

        private void Awake()
        {
            _carMovement = GetComponentInParent<ITrafficCarMovement>();
            _cars = new List<ITrafficCarMovement>();
        }

        private void OnTriggerEnter(Collider obstacle)
        {
            if (!obstacle.TryGetComponent(out ITrafficCarMovement otherCarMovement)
                && !obstacle.TryGetComponent(out GoalPos goalPos)) return;
            
            if (otherCarMovement == null)
            {
                _carMovement.StopCar();
                OnObstacaleEncounter?.Invoke(_carMovement);
            }
            else
            {
                _cars.Add(otherCarMovement);
                _isSlowedDown = true;
                _carMovement.SlowDownSpeed(otherCarMovement);
            }
        }
        
        private void OnTriggerExit(Collider obstacle)
        {
            if (!obstacle.TryGetComponent(out ITrafficCarMovement car)
                && !obstacle.TryGetComponent(out GoalPos goalPos)) return;

            if (_cars.Count == 0 && _carMovement.CarIsStopped)
            {
                _carMovement.PaceUpSpeed();
            } 

            if (_cars.Contains(car)) _cars.Remove(car); 

            if (_cars.Count != 0) return;
        
            _isSlowedDown = false;
            _carMovement.PaceUpSpeed();
        }
        
        public void StopTheFollowingVehicle()
        {
            if(!_isSlowedDown){ return;}
            _carMovement.StopCar();
        }
    }
}


