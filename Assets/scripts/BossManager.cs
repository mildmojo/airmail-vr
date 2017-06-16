using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour {
  public LayerMask interactables;

  void Start () {
    var reticle = Camera.main.GetComponent<GvrPointerPhysicsRaycaster>();
    reticle.eventMask = interactables;
  }

  void Update () {
    // Quit on Escape or Back (mobile)
    if (Input.GetKeyDown(KeyCode.Escape)) Application.Quit();
  }
}
