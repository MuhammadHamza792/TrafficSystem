using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.TrafficSystem
{
    public class CarSpawner : MonoBehaviour, ICarSpawner
    {
        [SerializeField] private TrafficCars trafficCar;

        private TrafficCar _carInstance;

        private int _spawnIndex;

        public void SpawnCars(List<Vector3> wayPoint)
        {
            foreach (var car in trafficCar.Cars)
            {
                _carInstance = Instantiate(car, wayPoint[_spawnIndex], Quaternion.identity);
                
                var dir = wayPoint[_spawnIndex < wayPoint.Count? _spawnIndex + 1 : 0] - wayPoint[_spawnIndex];

                var rot = Quaternion.LookRotation(dir, Vector3.up);
                
                _carInstance.transform.rotation = rot;
                
                _carInstance.AssignWayPoints(wayPoint, ++_spawnIndex);
                _spawnIndex += 3;
            }
        }
    }
}
