namespace Airmail {

  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using UnityEngine;

  public class MailboxController : MonoBehaviour {
    public GameObject doorHinge;
    public GameObject rearRockerHinge;
    public GameObject frontRockerHinge;
    public Vector3 doorOpenAngle;
    public float doorOpenSpeed;
    public float doorCloseSpeed;
    public int maxSimulCaptives;
    public float trapForce;
    public GameObject trapField;
    public TrapController trap;
    public GameObject killBox;

    [Header("Audio")]
    public AudioBucket openAudio;
    public AudioBucket closeAudio;


    // private List<Rigidbody> _captiveBirds;
    public bool _isOpen;
    private bool _isTweening;

    private class CaptiveBird {
      public Rigidbody body;
      public FlockAndLoad controller;
      public float captureDist;
      public Vector3 captureScale;
      public bool isDead;

      public CaptiveBird(Rigidbody body, FlockAndLoad controller, float captureDist = 0f, Vector3 captureScale = new Vector3(), bool isDead = false) {
        this.body = body;
        this.controller = controller;
        this.captureDist = captureDist;
        this.captureScale = captureScale;
        this.isDead = isDead;
      }
    }
    private List<CaptiveBird> _captiveBirds;

    void Awake() {
      _captiveBirds = new List<CaptiveBird>();
    }

    void Start() {
      trap.OnCapture.AddListener(OnCapture);
    }

    void Update() {
      ReelInCaptiveBirds();
    }

    void ReelInCaptiveBirds() {
      var deadBirds = false;

      for (var i = _captiveBirds.Count; i-- > 0;) {
        var bird = _captiveBirds[i];

        // No capture distance? Bird is still decelerating. Ignore.
        if (bird.captureDist <= 0f) continue;

        // Just ignore dead birds, don't clear the array. Yeah, this would be
        // terrible practice in a production game. Here, it doesn't matter
        // because the scene will be reloaded between plays.
        if (bird.body == null) {
          bird.isDead = true;
          deadBirds = true;
          continue;
        }

        var killBoxDir = killBox.transform.position - bird.body.transform.position;
        var trapDir = trapField.transform.position - bird.body.transform.position;
        bird.body.AddForce(trapForce * killBoxDir.normalized + trapForce/2f * trapDir.normalized);
        bird.body.transform.forward = -killBoxDir;

        // Shrink bird as it approaches killbox.
        var birdTrapDist = (bird.body.transform.position - trap.transform.position).sqrMagnitude;
        bird.body.transform.localScale = bird.captureScale * birdTrapDist / bird.captureDist;
      }

      if (deadBirds) {
        _captiveBirds.RemoveAll(bird => bird.isDead);
      }
    }

    public void OnCapture(GameObject bird) {
      // Don't capture if the trap stream is full.
      if (!_isOpen || _captiveBirds.Count >= maxSimulCaptives) return;

      Debug.Log("CAPTURING a BOID");
      var birdBody = bird.GetComponent<Rigidbody>();
      var controller = bird.GetComponent<FlockAndLoad>();
      var initialVelocity = birdBody.velocity;

      var captive = new CaptiveBird(body: birdBody, controller: controller, captureDist: -1f, captureScale: birdBody.transform.localScale);

      // Activate capture behavior, like disabling colliders.
      // If capture fails (because it's the last bird), ignore bird.
      if (!controller.StartCapture()) return;

      // Start tracking captive bird.
      _captiveBirds.Add(captive);

      // Slow to a stop, then start reeling it in.
      LeanTween
        .value(bird, val => birdBody.velocity = initialVelocity * val, 1f, 0f, 0.5f)
        .setEaseOutSine()
        .setOnComplete(() => {
          var sqrDistToTrap = (bird.transform.position - trap.transform.position).sqrMagnitude;
          captive.captureDist = sqrDistToTrap;
        });
    }

    public void OnSelected() {
      OpenDoor();
    }

    public void OnDeselected() {
      CloseDoor();
    }

    public void OpenDoor() {
      if (BossManager.Instance.isFinale) return;

      openAudio.Play();
      closeAudio.FadeOut();

      // Logic-wise, open immediately. Avoids open/closed state race
      //   condition with simultaneous tweens.
      _isOpen = true;

      LeanTween
        .rotateLocal(doorHinge, doorOpenAngle, doorOpenSpeed)
        .setEaseOutBounce();
      if (_isTweening) return;
      _isTweening = true;
      // Rock forward & back
      LeanTween
        .rotateAround(gameObject, Vector3.right, -7f, 0.09f)
        .setPoint(frontRockerHinge.transform.localPosition)
        .setLoopPingPong(1)
        .setEaseOutSine()
        .setDelay(doorOpenSpeed*0.25f)
        .setOnComplete(() => {
          LeanTween
            .rotateAround(gameObject, Vector3.right, 2f, 0.04f)
            .setPoint(rearRockerHinge.transform.localPosition)
            .setLoopPingPong(1)
            .setEaseOutSine()
            .setOnComplete(() => _isTweening = false);
        });
      // Squash
      LeanTween
        .scaleY(gameObject, 0.98f, doorOpenSpeed/2f - 0.6f)
        .setEaseOutSine()
        .setLoopPingPong(1)
        .setDelay(doorOpenSpeed/3f);
    }

    public void CloseDoor() {
      if (BossManager.Instance.isFinale) return;

      openAudio.FadeOut();
      closeAudio.PlayDelayed(0.1f);

      // Logic-wise, close immediately. Avoids open/closed state race
      //   condition with simultaneous tweens.
      _isOpen = false;

      LeanTween
        .rotateLocal(doorHinge, Vector3.zero, doorCloseSpeed)
        .setEaseOutBounce();
        // .setEaseInBack();
      if (_isTweening) return;
      _isTweening = true;
      // Rock back & forward
      LeanTween
        .rotateAround(gameObject, Vector3.right, 5f, 0.2f)
        .setPoint(rearRockerHinge.transform.localPosition)
        .setLoopPingPong(1)
        .setEaseOutCubic()
        .setDelay(doorCloseSpeed/2f)
        .setOnComplete(() => {
          LeanTween
            .rotateAround(gameObject, Vector3.right, -2.55f, 0.1f)
            .setPoint(frontRockerHinge.transform.localPosition)
            .setLoopPingPong(1)
            .setEaseOutCubic()
            .setOnComplete(() => _isTweening = false);
        });
      // Stretch
      LeanTween
        .scaleY(gameObject, 1.025f, doorCloseSpeed/2f + 0.25f)
        .setLoopPingPong(1)
        .setEaseOutSine()
        .setDelay(0f);
    }
  }

}
