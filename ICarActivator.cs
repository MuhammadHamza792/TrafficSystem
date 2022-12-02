using UnityEngine;

public interface ICarActivator
{
    Collider _collider { get; set; }
    
    void ActivateCar();
    void DeactivateCar();
    
}