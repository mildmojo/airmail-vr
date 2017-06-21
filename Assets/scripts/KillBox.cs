using UnityEngine;

public class KillBox : MonoBehaviour {
  void OnTriggerEnter(Collider c) {
    if (c is MeshCollider) {
      Destroy(c.gameObject);
    }
  }
}
