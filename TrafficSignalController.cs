using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityRandom = UnityEngine.Random;

namespace _Project.Scripts.TrafficSystem
{
    public class TrafficSignalController : MonoBehaviour
    {
        [SerializeField] private List<TrafficSignal> signals;
        [SerializeField] private TrafficCarIntersection intersection;
        [SerializeField] private int timeToStop;

        private int _index;

        private void Start()
        {
            _index = UnityRandom.Range(0,signals.Count - 1);
            StartCoroutine(EnableTrafficSignal());
        }
        
        private IEnumerator EnableTrafficSignal()
        {
            while (true)
            {
                signals[_index].GetCollider.enabled = false;
                signals[_index].PaceUpCar();
            
                yield return Helper.CoroutineHelper.GetWaitSec(timeToStop);
                
                yield return new WaitWhile(() => intersection.IsPassing);
                
                signals[_index].GetCollider.enabled = true;
                ++_index;
                
                if (_index > signals.Count - 1)
                {
                    _index = 0;
                }
                yield return null;
            }
        }
    }
}
