namespace airmail {

  using System.Collections;
  using System.Collections.Generic;
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

    private List<Rigidbody> _captiveBirds;
    private List<float> _captureDistances;
    private int _birdLayer;
    public bool _isOpen;
    private bool _isTweening;

    void Awake() {
      _captiveBirds = new List<Rigidbody>();
      _captureDistances = new List<float>();
      _birdLayer = LayerMask.NameToLayer("birds");
    }

    void Start() {
      Debug.Log(trap.name);
      trap.OnCapture.AddListener(OnCapture);
    }

    void Update() {
      var scaleOne = new Vector3(1f, 1f, 1f);

      for (var i = _captiveBirds.Count; i-- > 0;) {
        var bird = _captiveBirds[i];
        var birdCaptureDist = _captureDistances[i];

        // Just ignore dead birds, don't clear the array. Yeah, this would be
        // terrible practice in a production game. Here, it doesn't matter
        // because the scene will be reloaded between plays.
        if (bird == null) {
          // _captiveBirds.Remove(bird);
          continue;
        }

        var forceDir = killBox.transform.position - bird.transform.position;
        var trapDir = trapField.transform.position - bird.transform.position;
        bird.AddForce(trapForce * forceDir.normalized + trapForce/2f * trapDir.normalized);
        bird.transform.forward = -forceDir;

        var birdTrapDist = (bird.transform.position - trap.transform.position).sqrMagnitude;
        bird.transform.localScale = Vector3.one * birdTrapDist / birdCaptureDist;
      }
    }

    public void OnCapture(GameObject bird) {
      if (_isOpen && _captiveBirds.Count < maxSimulCaptives) {
        Debug.Log("CAPTURING a BOID");
        var controller = bird.GetComponent<FlockAndLoad>();
        controller.StartCapture();
        var birdBody = bird.GetComponent<Rigidbody>();
        var initialVelocity = birdBody.velocity;
        LeanTween
          .value(bird, val => birdBody.velocity = initialVelocity * val, 1f, 0f, 0.5f)
          .setEaseOutSine()
          .setOnComplete(() => {
            _captiveBirds.Add(birdBody);
            var sqrDistToTrap = (bird.transform.position - trap.transform.position).sqrMagnitude;
            _captureDistances.Add(sqrDistToTrap);
          });
      }
    }

    public void OnSelected() {
      OpenDoor();
    }

    public void OnDeselected() {
      CloseDoor();
    }

    public void OpenDoor() {
      // if (_isTweening) return;
      // LeanTween.cancel(gameObject);

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
      // LeanTween.cancel(gameObject);

      // Logic-wise, close immediately. Avoids open/closed state race
      //   condition with simultaneous tweens.
      _isOpen = false;

      LeanTween
        .rotateLocal(doorHinge, Vector3.zero, doorCloseSpeed)
        .setEaseInBack();
      if (_isTweening) return;
      _isTweening = true;
      // Rock back & forward
      LeanTween
        .rotateAround(gameObject, Vector3.right, 10f, 0.4f)
        .setPoint(rearRockerHinge.transform.localPosition)
        .setLoopPingPong(1)
        .setEaseOutCubic()
        .setDelay(doorCloseSpeed)
        .setOnComplete(() => {
          LeanTween
            .rotateAround(gameObject, Vector3.right, -5f, 0.1f)
            .setPoint(frontRockerHinge.transform.localPosition)
            .setLoopPingPong(1)
            .setEaseOutCubic()
            .setOnComplete(() => _isTweening = false);
        });
      // Stretch
      LeanTween
        .scaleY(gameObject, 1.05f, doorCloseSpeed/2f + 0.25f)
        .setLoopPingPong(1)
        .setEaseOutSine()
        .setDelay(0.75f*doorCloseSpeed);
    }
  }

}
