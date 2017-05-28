using UnityEngine;

public class KillBox : MonoBehaviour {
  void OnTriggerEnter(Collider c) {
    Destroy(c.gameObject);
  }
}
