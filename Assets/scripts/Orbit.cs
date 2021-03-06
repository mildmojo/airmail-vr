﻿namespace Airmail {

  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;

  public class Orbit : MonoBehaviour {
    public List<Transform> waypoints;
    public float distance;
    public float speed;
    public LayerMask birdLayerMask;

    private int currentWaypointIdx;
    private bool isMoving;

    void Start () {
      // StartCoroutine(RotateLoop());
    }

    void Update () {
      if (isMoving) {
        var targetDir = waypoints[currentWaypointIdx].position - transform.position;
        transform.Translate(targetDir.normalized * speed);
        if (targetDir.magnitude < speed) {
          isMoving = false;
          Debug.Log("stopped");
        }
      }
    }

    void OnTriggerEnter(Collider c) {
      if (!isMoving && maskMatch(birdLayerMask, c.gameObject.layer)) {
        Debug.Log("moving");
        isMoving = true;
        currentWaypointIdx++;
        if (currentWaypointIdx >= waypoints.Count) currentWaypointIdx = 0;
      }
    }

    IEnumerator RotateLoop () {
      var delay = new WaitForSeconds(5);
      while (true) {
        if (isMoving) {
          yield return delay;
          isMoving = false;
        } else {
          yield return true;
        }
      }
    }

    // If there's a 1 in the mask at the layer num's bit position, it's a match.
    bool maskMatch(LayerMask mask, int layerNum) {
      return mask == (mask | (1 << layerNum));
    }
  }

}
