namespace Airmail {

  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using UnityEngine;
  using UnityEngine.Events;

  public class TriggerEvent : MonoBehaviour {
    public UnityEvent onTriggerEnter;

    void OnTriggerEnter(Collider c) {
      onTriggerEnter.Invoke();
    }
  }

}
