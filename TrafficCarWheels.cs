using UnityEngine;

namespace _Project.Scripts.TrafficSystem
{
    public class TrafficCarWheels : MonoBehaviour, ITrafficCarWheels
    {
        [SerializeField] private WheelCollider wheelCollider;
        [SerializeField] private Transform wheelMesh;
        [SerializeField] private bool isSteerable;

        private float _targetAngle;
        private Vector3 _pos;
        private Quaternion _rot;
        private bool _isAvoiding;

        public float CurrentSpeed { get; private set; }


        public void Steer(float steerAngle, float maxSteerAngle, bool isAvoiding)
        {
            if (isAvoiding) { return;}

            if (!isSteerable) return;
        
            _targetAngle = steerAngle * maxSteerAngle;
            wheelCollider.steerAngle = Mathf.Lerp(wheelCollider.steerAngle, _targetAngle, Time.deltaTime * 5);
        }
    
        public void Accelerate(float maxTorque, float maxSpeed, float slowDown, bool reverse)
        {
            if(_isAvoiding) return;
            
            if (isSteerable) return;
        
            CurrentSpeed = 2 * Mathf.PI * wheelCollider.radius * wheelCollider.rpm * (60000 / 1000) * slowDown;

            if (CurrentSpeed < maxSpeed)
                wheelCollider.motorTorque = maxTorque;
            else 
                wheelCollider.motorTorque = 0;
        }

        public void SpeedUp(float speed)
        {
            if (isSteerable) return;
            wheelCollider.motorTorque = speed;
        }
    
        public void UpdatePosition()
        {
            wheelCollider.GetWorldPose(out _pos, out _rot);
            wheelMesh.SetPositionAndRotation(_pos, _rot);
        }

        public void ApplyBrake(float maxBreakTorque)
        {
            if (isSteerable) return;
        
            if (wheelCollider.motorTorque > 0) wheelCollider.motorTorque = 0;

            wheelCollider.brakeTorque = maxBreakTorque > 5000 ? float.MaxValue: maxBreakTorque;
        }

        public void RemoveBrakes()
        {
            if (!(wheelCollider.brakeTorque > 0)) return;
            wheelCollider.brakeTorque = 0;
        }

        public void TurnToAvoid(float steerAngle, float maxTurnAngle)
        {
            if(isSteerable)
                wheelCollider.steerAngle = steerAngle * maxTurnAngle;
        }
    }
}
