using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TrapController : MonoBehaviour {
  public class CaptureEvent : UnityEvent<GameObject> {};
  public CaptureEvent OnCapture;

  private int _birdLayer;

  void Awake () {
    OnCapture = new CaptureEvent();
    _birdLayer = LayerMask.NameToLayer("birds");
  }

  // Update is called once per frame
  void Update () {

  }

  void OnTriggerEnter(Collider c) {
    if (c.gameObject.layer == _birdLayer && c.GetType() == typeof(MeshCollider)) {
      OnCapture.Invoke(c.gameObject);
    }

  }
}
