namespace airmail {

  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;
  using UnityEngine.Serialization;

  public class FlockAndLoad : MonoBehaviour {
    public static List<FlockAndLoad> birds;

    public GameObject leader;
    public CenterAmong centroid;
    public float maxVelocity;
    public float acceleration;
    public float clusterAcceleration;
    public List<GameObject> waypoints;

    [Header("Flock Dispersal")]
    public float disperseAccel;
    [Range(0.75f, 2f)]
    public float maxFrequencyModifier;
    [Range(1f, 100f)]
    public float maxOffsetModifier;
    [Range(0f, 1f)]
    public float disperseMaxExpand;
    // Use this for initialization
    // [Header("Foo!")]
    // [Button("Hello", "Foo")]
    // public string foo;

    private int _wpIdx;
    private int _birdLayer;
    private Rigidbody _body;
    private SphereCollider _disperseCollider;
    private MeshCollider _collider;
    private float _disperseRadius;
    private float _disperseFreq;
    private float _disperseOffset;
    private bool _isCaptive;

    void Start () {
      _birdLayer = LayerMask.NameToLayer("birds");
      _body = GetComponent<Rigidbody>();
      _disperseCollider = GetComponent<SphereCollider>();
      _collider = GetComponent<MeshCollider>();
      _disperseRadius = _disperseCollider.radius;
      _disperseFreq = Random.Range(0.75f, maxFrequencyModifier);
      _disperseOffset = Random.Range(0f, maxOffsetModifier);
      if (birds == null) birds = new List<FlockAndLoad>();
      birds.Add(this);
      centroid.AddObject(gameObject);
    }

    // Update is called once per frame
    void Update () {
      fly();
      Debug.DrawRay(transform.position, _body.velocity, Color.cyan);
      _disperseCollider.radius = _disperseRadius + 1f + Mathf.Sin(Time.time * _disperseFreq + _disperseOffset) * disperseMaxExpand;
    }

    public void StartCapture() {
      _isCaptive = true;
      birds.Remove(this);
      centroid.RemoveObject(gameObject);
      _disperseCollider.enabled = false;
      _collider.isTrigger = true;

      // Grow the rest of the flock
      foreach (var bird in birds) {
        bird.transform.localScale = bird.transform.localScale * 1.5f;
      }
    }

    void OnDestroy() {
      birds.Remove(this);
      centroid.RemoveObject(gameObject);
    }

    void OnTriggerStay(Collider c) {
      if (isBird(c)) {
        var accelVector = (transform.position - c.transform.position).normalized * disperseAccel;
        addAccel(accelVector);
      }
    }

    GameObject nextWaypoint() {
      _wpIdx++;
      if (_wpIdx >= waypoints.Count) _wpIdx = 0;
      return waypoints[_wpIdx];
    }

    bool isBird(Collider c) {
      return c.gameObject.layer == _birdLayer;
    }

    void fly() {
      var leaderDir = leader.transform.position - transform.position;
      if (!_isCaptive) {
        transform.forward = leaderDir;
      }

      // Fly toward leader.
      addAccel(leaderDir.normalized * acceleration);
      Debug.DrawRay(transform.position, leaderDir, Color.magenta);

      // Limit max velocity, hard cap.
      if (_body.velocity.magnitude > maxVelocity) {
        _body.velocity = _body.velocity.normalized * maxVelocity;
      }

      if (centroid.Count > 1) {
        addAccel((centroid.transform.position - transform.position).normalized * clusterAcceleration);
      }
    }

    void addAccel(Vector3 force) {
      _body.AddForce(force * (_isCaptive ? 0f : 1f));
    }
  }

}
