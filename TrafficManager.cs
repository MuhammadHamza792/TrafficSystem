using System.Collections.Generic;
using BansheeGz.BGSpline.Components;
using UnityEngine;

namespace _Project.Scripts.TrafficSystem
{
    public class TrafficManager : MonoBehaviour
    {
        Queue<TrafficCar> trafficCarsQueue; 

        [SerializeField] private List<BGCcSplitterPolyline> polyLines;
        List<Vector3> wayPointPositions;

        private void Awake()
        {
            wayPointPositions = new List<Vector3>();
            trafficCarsQueue = new Queue<TrafficCar>();
        }
    
        public void GetAllWayPoints()
        {
            foreach (var polyLine in polyLines)
            {
                ClearWayPoints();

                var Waypoints = polyLine.Points;

                foreach (var point in Waypoints)
                {
                    wayPointPositions.Add(point.Position);
                }

                SpawnWayPointsCars(polyLine, wayPointPositions);
            }
        }

        private void SpawnWayPointsCars(BGCcSplitterPolyline currentWaypoint, List<Vector3> points)
        {
            currentWaypoint.GetComponent<ICarSpawner>().SpawnCars(points);
        }

        private void ClearWayPoints()
        {
            if(wayPointPositions.Count != 0)
                wayPointPositions = new List<Vector3>();
        }
    }
}
