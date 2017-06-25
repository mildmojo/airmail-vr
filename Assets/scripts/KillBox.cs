namespace Airmail {

  using UnityEngine;

  public class KillBox : MonoBehaviour {
    public LayerMask killLayerMask;

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
