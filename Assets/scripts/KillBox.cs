using UnityEngine;

public class KillBox : MonoBehaviour {
  void OnTriggerEnter(Collider c) {
    if (c.GetType() == typeof(MeshCollider)) {
      Destroy(c.gameObject);
    }
  }
}
