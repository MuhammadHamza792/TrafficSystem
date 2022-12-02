using System.Collections.Generic;
using UnityEngine;

namespace _Project.Scripts.TrafficSystem
{
    public interface ICarSpawner
    {
        public void SpawnCars(List<Vector3> wayPoint);
    }
}