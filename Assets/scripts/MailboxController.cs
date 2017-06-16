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
    public TrapController trap;
    public GameObject killBox;

    private List<Rigidbody> _captiveBirds;
    private int _birdLayer;
    public bool _isOpen;
    private bool _isTweening;

    void Awake() {
      _captiveBirds = new List<Rigidbody>();
      _birdLayer = LayerMask.NameToLayer("birds");
    }

    void Start() {
      Debug.Log(trap.name);
      trap.OnCapture.AddListener(OnCapture);
    }

    void Update() {
      var scaleOne = new Vector3(1f, 1f, 1f);

      foreach (var bird in _captiveBirds.ToArray()) {
        if (bird == null) {
          _captiveBirds.Remove(bird);
          continue;
        }

        var forceDir = killBox.transform.position - bird.transform.position;
        bird.AddForce(trapForce * forceDir.normalized);
        bird.transform.forward = -forceDir;
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
          .setOnComplete(() => _captiveBirds.Add(birdBody));
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
          _isOpen = true;
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
          _isOpen = false;
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
