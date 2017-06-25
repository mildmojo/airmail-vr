namespace airmail {

  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using UnityEngine;

  public class CenterAmong : MonoBehaviour {
    public List<GameObject> objects;
    public float pollInterval;

    public int Count {
      get { return objects.Count; }
    }

    void Start () {
      StartCoroutine(FollowObjects());
    }

    public void AddObject(GameObject obj) {
      Debug.Log("Adding " + obj.name);
      objects.Add(obj);
    }

    public void RemoveObject(GameObject obj) {
      objects.Remove(obj);
    }

    IEnumerator FollowObjects() {
      // var delay = new WaitForSeconds(pollInterval);
      while (true) {
        var delay = new WaitForSeconds(pollInterval);
        yield return delay;
        if (objects.Count > 0) {
          transform.position = VectorAverage(objects.Select(o => o.transform.position).ToList());
        }
      }
    }

    Vector3 VectorAverage(List<Vector3> vectors) {
      return vectors.Aggregate(Vector3.zero, (v, sum) => sum + v) / vectors.Count;
    }
  }

}
