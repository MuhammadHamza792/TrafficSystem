using System.Collections;
using UnityEngine;

namespace _Project.Scripts.TrafficSystem
{
    public interface ITrafficCarMovement
    {
        public bool CarIsStopped { set; get; }

        public bool IsDirty { get; set; }

        public float CarSpeed { set; get; }

        public Vector3 Waypoint { set; get; }

        void StopCar();

        void CheckCarPosition(Transform otherCar);

        void StopCarWithDelay();

        void ToggleHorn(bool active);

        void Turn(float angle);

        void ToAvoid(bool avoid);

        void SlowDownSpeed( ITrafficCarMovement otherCar );
    
        void PaceUpSpeed();
    }
}
