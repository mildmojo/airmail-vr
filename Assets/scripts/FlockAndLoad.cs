namespace airmail {

  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;
  using UnityEngine.Serialization;

  public class FlockAndLoad : MonoBehaviour {
    public static List<FlockAndLoad> birds;

    public GameObject leader;
    public GameObject mailbox;
    public CenterAmong centroid;
    public float maxVelocity;
    public float velocityVariance;
    public float acceleration;
    public float clusterAcceleration;
    public List<GameObject> waypoints;

    public float steeringPower;
    public float steeringVariance;

    [Header("Flock Dispersal")]
    public SphereCollider disperseCollider;
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
    [System.NonSerialized]
    public float trapRangeAtCapture;

    private int _wpIdx;
    private int _birdLayer;
    private Vector3 _heading;
    private Rigidbody _body;
    private MeshCollider _collider;
    private float _disperseRadius;
    private float _disperseFreq;
    private float _disperseOffset;
    private bool _isCaptive;
    private bool _isFinale;

    void Start () {
      _birdLayer = LayerMask.NameToLayer("birds");
      _body = GetComponent<Rigidbody>();
      // disperseCollider = GetComponent<SphereCollider>();
      _collider = GetComponent<MeshCollider>();
      _disperseRadius = disperseCollider.radius;
      _disperseFreq = Random.Range(0.75f, maxFrequencyModifier);
      _disperseOffset = Random.Range(0f, maxOffsetModifier);
      maxVelocity += Random.Range(0f, velocityVariance) - velocityVariance / 4f;
      steeringPower += Random.Range(0f, steeringVariance) - steeringVariance / 2f;
      if (birds == null) birds = new List<FlockAndLoad>();
      birds.Add(this);
      centroid.AddObject(gameObject);
      var mesh = transform.Find("default");

      // Bob up and down with wing flapping.
      LeanTween.moveLocalY(mesh.gameObject, 0.25f, Random.Range(0.15f, 0.3f))
        .setEaseInSine()
        .setLoopPingPong()
        .setDelay(Random.value * 0.7f);

      // Occasionally change heading toward target.
      StartCoroutine(Steering());
    }

    // Update is called once per frame
    void Update () {
      fly();
      Debug.DrawRay(transform.position, _body.velocity, Color.cyan);
      // disperseCollider.radius = _disperseRadius + 1f + Mathf.Sin(Time.time * _disperseFreq + _disperseOffset) * disperseMaxExpand;
    }

    public bool StartCapture() {
      // Don't capture the last bird; it's special.
      // Just one bird left? Start the final animation.
      if (birds.Count == 1) {
        StartFinale();
        return false;
      }

      _isCaptive = true;
      birds.Remove(this);
      centroid.RemoveObject(gameObject);
      disperseCollider.enabled = false;
      _collider.isTrigger = true;

      // Grow the rest of the flock
      foreach (var bird in birds) {
        bird.transform.localScale *= 1.25f;
        bird.transform.Find("FlockCollider").localScale *= 0.95f;
      }

      return true;
    }

    void StartFinale() {
      Debug.Log("FINALE!!!!");
      return;
      _isFinale = true;

      // Come to a stop, physics-wise
      LeanTween.value(gameObject, val => _body.velocity = val, _body.velocity, Vector3.zero, 0.5f);

      // Tell boss controller to animate us?
      // Or get a reference to mailbox and do it here?
      var path = new LTBezierPath();
      // TODO: this wants transforms, figure out how to make transforms or hand-place them.
      // path.place(transform.position + transform.up * 2f);
      // path.place(mailbox.transform.position + mailbox.transform.up * 2f);
      var flyToMailbox = LeanTween.move(gameObject, path, 2f);

      flyToMailbox.setOnComplete(() => {
        Debug.Log("We got there!");
      });
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
      if (_isFinale) return;

      // Use busted tweening via Slerp to turn toward target heading
      transform.forward = Vector3.Slerp(transform.forward, _heading, Time.deltaTime * steeringPower);

      // Fly toward leader.
      addAccel(_heading * acceleration);
      Debug.DrawRay(transform.position, leader.transform.position - transform.position, Color.magenta);

      // Limit max velocity, hard cap.
      if (_body.velocity.magnitude > maxVelocity) {
        _body.velocity = _body.velocity.normalized * maxVelocity;
      }

      // Accelerate toward flock center AFTER velocity cap so stragglers can catch up.
      if (centroid.Count > 1) {
        addAccel((centroid.transform.position - transform.position).normalized * clusterAcceleration);
      }
    }

    IEnumerator Steering() {
      while (true) {
        if (!_isCaptive) {
          _heading = leader.transform.position - transform.position + randomVec(-1f, 1f);
          Debug.DrawRay(transform.position, _heading * 2f, Color.red, 0.1f);
        }
        yield return new WaitForSeconds(Random.value * 0.2f + 0.2f);
      }
    }

    void addAccel(Vector3 force) {
      _body.AddForce(force * (_isCaptive ? 0f : 1f));
    }

    Vector3 randomVec(float min, float max) {
      var range = max - min;
      return new Vector3(
        Random.value * range + min,
        Random.value * range + min,
        Random.value * range + min
      );
    }
  }

}
