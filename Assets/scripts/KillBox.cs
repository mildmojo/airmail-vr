namespace Airmail {

  using UnityEngine;

  public class KillBox : MonoBehaviour {
    public static KillBox Instance;

    public LayerMask killLayerMask;

    void Awake() {
      Instance = this;
    }

    void OnTriggerEnter(Collider c) {
      if (maskMatch(killLayerMask, c.gameObject.layer)) {
        Destroy(c.gameObject);
      }
    }

    // If there's a 1 in the mask at the layer num's bit position, it's a match.
    bool maskMatch(LayerMask mask, int layerNum) {
      return mask == (mask | (1 << layerNum));
    }
  }

}
