namespace _Project.Scripts.TrafficSystem
{
    public interface ITrafficCarWheels
    {
        void Accelerate(float maxTorque, float maxSpeed, float slowDown, bool reverse);
        void Steer(float steerAngle, float maxSteerAngle, bool isAvoiding);
        void SpeedUp(float speed);
        public void ApplyBrake(float maxBreakTorque);
        public void RemoveBrakes();
        void UpdatePosition();
    }
}