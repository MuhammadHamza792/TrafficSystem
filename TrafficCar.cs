using System;
using System.Collections;
using System.Collections.Generic;
using _Project.Scripts.Helper;
using ScriptableEvents.Events;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.Scripts.TrafficSystem
{
    public class TrafficCar : MonoBehaviour
    {
        [SerializeField] List<Vector3> wayPoints;
        
        private ITrafficCarMovement _carMovement;
        private NativeArray<float> _result;

        private JobHandle _distanceJob;
        private CalDistanceJob _calDistance;

        private int _currentIndex;
        private bool _isDirty;
        
        public int StartIndex { set; get; }

        private void Awake() => _carMovement = GetComponent<ITrafficCarMovement>();

        private void OnEnable() => _carMovement?.PaceUpSpeed();

        private void Start() => _result = new NativeArray<float>(1, Allocator.Persistent);

        public void AssignWayPoints(List<Vector3> points, int indexToFollow)
        {
            wayPoints = new List<Vector3>();
            wayPoints = points;
            _currentIndex = indexToFollow;
            StartIndex = indexToFollow;
        }

        public void SetCurrentIndex(int index) => _currentIndex = index;

        private void FixedUpdate() => ScheduleDistanceJob();

        private void ScheduleDistanceJob()
        {
            _calDistance = new CalDistanceJob
            {
                CarPosition = transform.position,
                Waypoint = wayPoints[_currentIndex],
                Value = _result
            };

            var dependency = _calDistance.Schedule(1, new JobHandle());
            _distanceJob = _calDistance.ScheduleParallel(1, 64, dependency);
            _isDirty = true;
        }
        
        private void LateUpdate()
        {
            if(!_isDirty) return;
            _distanceJob.Complete();
            CheckDistance(_calDistance.Value[0]);
            _carMovement.Waypoint = wayPoints[_currentIndex];
            _carMovement.IsDirty = true;
            ReturnToStart();
        }
        
        private void ReturnToStart()
        {
            if (_currentIndex <= wayPoints.Count - 1) return;
            _currentIndex = 0;
        }

        private void CheckDistance(float distance)
        {
            if (!(distance < 6.25f)) return;
            _currentIndex++;
        }

        private void OnDisable()
        {
            if (_carMovement == null) return;
            _carMovement.StopCar();
            _carMovement.IsDirty = false;
            _isDirty = false;
        }
        
        private void OnDestroy() => _result.Dispose();
    }

    [BurstCompile]
    public struct CalDistanceJob : IJobFor
    {
        public NativeArray<float> Value;

        public float3 CarPosition;
        public float3 Waypoint;
    
        public void Execute(int index)
        {
            var num1 = Waypoint.x - CarPosition.x;
            var num2 = Waypoint.y - CarPosition.y;
            var num3 = Waypoint.z - CarPosition.z;
            Value[0] = (float)((double)num1 * num1 + (double)num2 * num2 + (double)num3 * num3);
        }
    }
}