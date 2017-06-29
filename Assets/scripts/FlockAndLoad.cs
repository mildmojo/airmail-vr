namespace Airmail {

  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;
  using UnityEngine.Audio;

  public class FlockAndLoad : MonoBehaviour {
    public static List<FlockAndLoad> birds;

    public Animator flappingAnimator;
    public AudioBucket deathRattle;
    public AudioBucket flapSound;
    public AudioMixer mainMixer;
    public AudioSource crashSound;
    public GameObject leader;
    public GameObject mailbox;
    public CenterAmong centroid;
    public GameObject finaleTarget;
    public float maxVelocity;
    public float velocityVariance;
    public float acceleration;
    public float clusterAcceleration;

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
    [System.NonSerialized]
    public bool isCaptive;

    private int _wpIdx;
    private Vector3 _heading;
    private Rigidbody _body;
    private MeshCollider _collider;
    private float _disperseRadius;
    private float _disperseFreq;
    private float _disperseOffset;
    private bool _isFinale;
    private float _flapSpeedMult;
    private BossManager _config;

    void Start () {
      _config = BossManager.Instance;
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
      _config.birdCount++;
      centroid.AddObject(gameObject);

      _flapSpeedMult = Random.Range(2f, 2.5f);
      flappingAnimator.SetFloat("FlappingSpeed", _flapSpeedMult);
      flappingAnimator.SetFloat("FlappingCycleOffset", Random.Range(0f, 0.75f));

      // Bob up and down with wing flapping.
      // var mesh = transform.Find("default");
      // LeanTween.moveLocalY(mesh.gameObject, 0.25f, Random.Range(0.15f, 0.3f))
      //   .setEaseInSine()
      //   .setLoopPingPong()
      //   .setDelay(Random.value * 0.7f);

      // Occasionally change heading toward target.
      StartCoroutine(Steering());
    }

    void Update () {
      fly();
      Debug.DrawRay(transform.position, _body.velocity, Color.cyan);
      // disperseCollider.radius = _disperseRadius + 1f + Mathf.Sin(Time.time * _disperseFreq + _disperseOffset) * disperseMaxExpand;
    }

    public bool StartCapture() {
      // Don't capture the last bird; it's special.
      if (birds.Count == 1) return false;

      isCaptive = true;
      birds.Remove(this);
      centroid.RemoveObject(gameObject);
      disperseCollider.enabled = false;
      _collider.isTrigger = true;

      flappingAnimator.SetFloat("FlappingSpeed", Random.Range(3.7f, 4.2f));
      deathRattle.PlayDelayed(0.5f);

      // Grow the rest of the flock
      foreach (var bird in birds) {
        if (bird.isCaptive) continue;
        bird.Grow();
        leader.transform.localScale *= 1.025f;
      }

      // Just one bird left? Start the final animation.
      if (birds.Count == 1) {
        birds[0].StartFinale();
      }

      return true;
    }

    public void Grow() {
      // Scale from 1.0 to `maxBirdScale`
      // Range is `maxBirdScale - 1.0f`
      // 0% along range is `birds.Count == _config.birdCount`
      // 100% along range is `birds.Count == 1`
      // Progress range is `_config.birdCount - 1`
      // Progress is `1f / (birds.Count / (_config.birdCount - 1))`

      // var pctComplete = 1f / (birds.Count / (_config.birdCount - 1));
      // var newScale = 1f + _config.birdScaleCurve.Evaluate(pctComplete) * (_config.maxBirdScale - 1f) * pctComplete;
      // LeanTween.scale(gameObject, Vector3.one * newScale, 0.2f)
      //   .setEaseOutElastic();
      LeanTween.scale(gameObject, transform.localScale * 1.25f, 0.2f)
        .setEaseOutElastic();
      // LeanTween.value(gameObject, val => )
      disperseCollider.transform.localScale *= 0.93f;

      mainMixer.SetFloat("birdFlapCutoff", 1000f - 1000f * transform.localScale.magnitude / (_config.maxBirdScale / 2f));
      flapSound.pitch *= 0.96f;
    }

    public void StartFinale() {
      Debug.Log("FINALE!!!!");
      _isFinale = true;
      BossManager.Instance.isFinale = true;
      _collider.enabled = false;
      disperseCollider.enabled = false;

      finaleDecelerateStop()
        .setOnComplete(() => finaleFlyUp()
          .setOnComplete(() => finaleFlyToMailbox()
            .setOnComplete(() => finalePause()
              .setOnComplete(() => finaleCrushMailbox()
                .setOnComplete(() => finaleLookAround()
                  .setOnComplete(BossManager.Instance.OnQuit)
                )
              )
            )
          )
        );
    }

    LTDescr finaleDecelerateStop() {
      // Come to a stop, physics-wise
      _body.angularVelocity = Vector3.zero;
      return LeanTween.value(gameObject, val => _body.velocity = val, _body.velocity, Vector3.zero, 0.5f);
    }

    LTDescr finaleFlyUp() {
      leader = finaleTarget;
      return LeanTween.moveY(gameObject, finaleTarget.transform.position.y, 1f)
        .setEaseInOutCubic();
    }

    LTDescr finaleFlyToMailbox() {
      var lookAt = Camera.main.transform.position - finaleTarget.transform.position + Camera.main.transform.right * -5f;
      LeanTween.value(gameObject, vec => transform.up = vec, transform.up, Vector3.up, 2f);
      LeanTween.value(gameObject, vec => transform.forward = vec, transform.forward, lookAt, 4f);
      return LeanTween.move(gameObject, finaleTarget.transform.position, 3f)
        .setEaseOutBack();
    }

    LTDescr finaleCrushMailbox() {
      KillBox.Instance.enabled = false;
      LeanTween.scaleY(mailbox, 0.001f, 0.4f)
        .setEaseInQuart()
        .setDelay(0.6f);
      LeanTween.delayedCall(0.9f, crashSound.Play);
      return LeanTween.moveY(gameObject, 0.25f, 1f)
        .setEaseInBack();
    }

    LTDescr finalePause(float delay = 2f) {
      return LeanTween.delayedCall(delay, () => {});
    }

    LTDescr finaleLookAround() {
      _body.velocity = Vector3.zero;
      _body.angularVelocity = Vector3.zero;
      flappingAnimator.SetBool("DoLookAround", true);
      return LeanTween.delayedCall(10f, () => {});
    }

    void OnDestroy() {
      // WTF why is this still called when the component is disabled?
      if (enabled) {
        birds.Remove(this);
        centroid.RemoveObject(gameObject);
      }
    }

    void OnTriggerStay(Collider c) {
      if (isBird(c)) {
        var accelVector = (transform.position - c.transform.position).normalized * disperseAccel;
        addAccel(accelVector);
      }
    }

    // It's a bird if it's on the same layer as us.
    bool isBird(Collider c) {
      return gameObject.layer == c.gameObject.layer;
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

      // Flap wings slower or faster based on velocity.
      // TODO: FIX THIS
      // flappingAnimator.SetFloat("FlappingSpeed", _flapSpeedMult * Mathf.Pow(_body.velocity.magnitude / maxVelocity , 10f));
    }

    IEnumerator Steering() {
      var steeringCount = 0;
      while (true) {
        if (!isCaptive) {
          steeringCount = (steeringCount + 1) % 8;
          _heading = leader.transform.position - transform.position + randomVec(-1f, 1f);
          if (steeringCount == 0) _heading += randomVec(-10f, 10f);
          Debug.DrawRay(transform.position, _heading * 2f, Color.red, 0.1f);
        }
        yield return new WaitForSeconds(Random.value * 0.2f + 0.2f);
      }
    }

    void addAccel(Vector3 force) {
      _body.AddForce(force * (isCaptive ? 0f : 1f));
    }

    Vector3 randomVec(float min, float max) {
      return new Vector3(
        Random.Range(min, max),
        Random.Range(min, max),
        Random.Range(min, max)
      );
    }
  }

}
