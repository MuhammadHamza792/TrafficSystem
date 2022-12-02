using System;
using System.Collections;
using _Project.Scripts.Helper;
using ScriptableEvents.Events;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.Scripts.TrafficSystem
{
    public class TrafficCarMovement : MonoBehaviour, ITrafficCarMovement
    {
        [SerializeField] private TrafficCarProperties trafficCar;
        [SerializeField] private AudioClipScriptableEvent _playHorn;
        [SerializeField] private AudioClip _horn;
        [SerializeField] private TrafficCarWheels [] wheels;

        private Transform _transform;
        private Rigidbody _rb;
        private bool _avoid;
        private bool _isRbNotNull;

        private WaitForSeconds _wait;
        private Coroutine _delayAfterStop;
        
        private NativeArray<float> _inverseTransform;
        private NativeArray<float> _calDotProduct;

        private CalInverseJob _calInverse;
        private CalDotProduct _calDot;

        private JobHandle _scheduleInverseJob;
        private JobHandle _scheduleDotJob;

        private bool _isDirty;

        public float CarSpeed { get; set; }
        
        public bool IsDirty { get; set; }
        
        public Vector3 Waypoint { get; set; }
        
        [field:SerializeField] public bool CarIsStopped { get; set; }

        private void Start ( )
        {
            _isRbNotNull = _rb != null;
            _inverseTransform = new NativeArray<float>(1 , Allocator.Persistent);
            _calDotProduct = new NativeArray<float>(1, Allocator.Persistent);
        }

        private void Awake ( )
        {
            _transform = transform;
            _rb = GetComponent<Rigidbody>();
            _wait = new WaitForSeconds(10f);
        }

        public void ToAvoid ( bool avoid ) => _avoid = avoid;

        private void FixedUpdate ()
        {
            if(!IsDirty) return;
            
            var position = _transform.position;

            _calInverse = new CalInverseJob
            {
                Position = position,
                Rotation = _transform.rotation,
                Scale = _transform.lossyScale,
                Waypoint = Waypoint,
                Value = _inverseTransform,
            };

            var inverseJobDependency = _calInverse.Schedule(1, new JobHandle());
            _scheduleInverseJob = _calInverse.ScheduleParallel(1, 32, inverseJobDependency);

            _calDot = new CalDotProduct
            {
                Position = position,
                Velocity = _rb.velocity,
                Waypoint = Waypoint,
                Value = _calDotProduct
            };
            
            var dotJobDependency = _calDot.Schedule(1, new JobHandle());
            _scheduleDotJob = _calDot.ScheduleParallel(1, 32, dotJobDependency);
            _isDirty = true;
        }

        public void LateUpdate()
        {
            if(!IsDirty) return;
            if(!_isDirty) return;
            
            _scheduleInverseJob.Complete();
            var steerAngle = _calInverse.Value[0];
            _scheduleDotJob.Complete();
            var dotProduct = _calDot.Value[0];
            
            foreach ( var wheel in wheels )
            {
                wheel.Steer(steerAngle , trafficCar.TurnSpeed , _avoid);
                wheel.Accelerate(trafficCar.MaxSpeed , trafficCar.MaxTorque, dotProduct, false);
                CarSpeed = wheel.CurrentSpeed;
                wheel.UpdatePosition();
            }
        }

        public void SlowDownSpeed ( ITrafficCarMovement otherCar )
        {
            if ( otherCar.CarIsStopped )
            {
                StopCarWithDelay();
                return;
            }

            if ( CarSpeed <= otherCar.CarSpeed ) return;

            foreach ( var wheel in wheels )
            {
                _rb.drag = 1;
                wheel.ApplyBrake(trafficCar.MinBrakeTorque);
            }
        }

        public void PaceUpSpeed ( )
        {
            CarIsStopped = false;
            if ( _isRbNotNull )
                _rb.drag = 0;

            foreach ( var wheel in wheels )
            {
                wheel.RemoveBrakes();
            }
        }

        public void StopCar ( )
        {
            CarIsStopped = true;
            _rb.drag = 1;

            foreach ( var wheel in wheels )
            {
                wheel.ApplyBrake(trafficCar.MaxBrakeTorque);
            }
        }

        public void CheckCarPosition(Transform otherCar)
        {
            var carTransform = transform;
            var dir = (otherCar.position - carTransform.position).normalized;
            var pos = Vector3.Dot(carTransform.forward, dir);
            if (pos > 0)
            {
                ToggleHorn(true);
                StopCar();
            }
            else
            {
                foreach ( var wheel in wheels )
                {
                    wheel.SpeedUp(float.MaxValue);
                }
                Turn(transform.right.x);
            }
        }

        public void StopCarWithDelay()
        {
            CarIsStopped = true;
            _rb.drag = 1;

            foreach ( var wheel in wheels )
            {
                wheel.ApplyBrake(trafficCar.MaxBrakeTorque);
            }

            if (_delayAfterStop != null) StopCoroutine(_delayAfterStop);
            _delayAfterStop = StartCoroutine(DelayAfterStop());
        }
        

        public void Turn ( float angle )
        {
            if ( !_avoid ) { return; }
            foreach ( var wheel in wheels )
            {
                wheel.TurnToAvoid(angle , 60);
            }
        }
        
        public void ToggleHorn(bool active)
        {
            if (active)
            {
                StartCoroutine(nameof(PlayHorn));
            }
            else
            {
                StopCoroutine(nameof(PlayHorn));
            }
        }

        public IEnumerator PlayHorn()
        {
            while (true)
            {
                yield return CoroutineHelper.GetWaitSec(.75f); 
                
                _playHorn.Raise(_horn);
            
                yield return CoroutineHelper.GetWaitSec(2f); 
            }
            
        }

        private IEnumerator DelayAfterStop()
        {
            yield return _wait;
            PaceUpSpeed();
        }

        private void OnDisable() => _isDirty = false;

        private void OnDestroy()
        {
            _inverseTransform.Dispose();
            _calDotProduct.Dispose();
        }
    }

    [BurstCompile]
    public struct CalDotProduct : IJobFor
    {
        public float3 Position;
        public float3 Velocity;
        public float3 Waypoint;
        public NativeArray<float> Value; 
        
        public void Execute(int index)
        {
            //Calculating To slowdown car on Turns
            
            var dir = Waypoint - Position;
            
            var dirMag = Mathf.Sqrt((float)(
                (double)dir.x * dir.x +
                (double)dir.y * dir.y +
                (double)dir.z * dir.z));

            var normDir = dir / dirMag;
            
            var dotProduct = normDir.x * Velocity.x 
                             + normDir.y * Velocity.y + 
                             normDir.z * Velocity.z;
            Value[0] = dotProduct;
        }
    }
    
    [BurstCompile]
    public struct CalInverseJob : IJobFor
    {
        public float3 Position;
        public quaternion Rotation;
        public float3 Scale;
        public float3 Waypoint;
        public NativeArray<float> Value;

        private float3 _multipliedResult;

        public void Execute(int index)
        {
            var negatePosition = Waypoint - Position;
            
            //Inverting Rotation
            Rotation.value.x = -Rotation.value.x;
            Rotation.value.y = -Rotation.value.y;
            Rotation.value.z = -Rotation.value.z;

            var norm = Rotation.value.x * Rotation.value.x + Rotation.value.y * Rotation.value.y +
                       Rotation.value.z * Rotation.value.z + Rotation.value.w * Rotation.value.w;

            Rotation.value.x /= norm;
            Rotation.value.y /= norm;
            Rotation.value.z /= norm;
            Rotation.value.w /= norm;

            //================================    

            var negatingScale = new Vector3(1 / Scale.x, 1 / Scale.y, 1 / Scale.z);

            //Multiplying Quaternion and Vector
            var num1 = Rotation.value.x * 2f;
            var num2 = Rotation.value.y * 2f;
            var num3 = Rotation.value.z * 2f;
            var num4 = Rotation.value.x * num1;
            var num5 = Rotation.value.y * num2;
            var num6 = Rotation.value.z * num3;
            var num7 = Rotation.value.x * num2;
            var num8 = Rotation.value.x * num3;
            var num9 = Rotation.value.y * num3;
            var num10 = Rotation.value.w * num1;
            var num11 = Rotation.value.w * num2;
            var num12 = Rotation.value.w * num3;

            _multipliedResult.x = (float)((1.0 - ((double)num5 + (double)num6)) * (double)negatePosition.x +
                                         ((double)num7 - (double)num12) * (double)negatePosition.y +
                                         ((double)num8 + (double)num11) * (double)negatePosition.z);
            _multipliedResult.y = (float)(((double)num7 + (double)num12) * (double)negatePosition.x +
                                         (1.0 - ((double)num4 + (double)num6)) * (double)negatePosition.y +
                                         ((double)num9 - (double)num10) * (double)negatePosition.z);
            _multipliedResult.z = (float)(((double)num8 - (double)num11) * (double)negatePosition.x +
                                         ((double)num9 + (double)num10) * (double)negatePosition.y +
                                         (1.0 - ((double)num4 + (double)num5)) * (double)negatePosition.z);

            //=================================

            //InverseTransform

            var inverseTransform = new float3(negatingScale.x * _multipliedResult.x, negatingScale.y * _multipliedResult.y,
                negatingScale.z * _multipliedResult.z);

            //=================================
        
            var magnitude = Mathf.Sqrt((float)(
                (double)inverseTransform.x * inverseTransform.x +
                (double)inverseTransform.y * inverseTransform.y +
                (double)inverseTransform.z * inverseTransform.z));

            Value[0] = inverseTransform.x / magnitude;
        }
    }
}