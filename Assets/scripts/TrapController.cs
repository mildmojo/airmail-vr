namespace Airmail {

  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;
  using UnityEngine.Events;

  public class TrapController : MonoBehaviour {
    public class CaptureEvent : UnityEvent<GameObject> {};
    public CaptureEvent OnCapture;

    public LayerMask birdLayerMask;

    void Awake () {
      OnCapture = new CaptureEvent();
    }

    void OnTriggerEnter(Collider c) {
      if (maskMatch(birdLayerMask, c.gameObject.layer) && c is MeshCollider) {
        OnCapture.Invoke(c.gameObject);
      }
    }

    // If there's a 1 in the mask at the layer num's bit position, it's a match.
    bool maskMatch(LayerMask mask, int layerNum) {
      return mask == (mask | (1 << layerNum));
    }
  }

}
