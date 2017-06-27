namespace Airmail {

  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;

  [RequireComponent(typeof(AudioSource))]
  public class GazeAmplifier : MonoBehaviour {
    public float minVolume;
    public float maxVolume;
    public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);

    private AudioSource _audioSource;
    private Transform _camTrans;

    void Start() {
      _audioSource = GetComponent<AudioSource>();
      _camTrans = Camera.main.transform;
    }

    void Update() {
      _audioSource.volume = gazeBetween(minVolume, maxVolume);
    }

    float gazeBetween(float min, float max) {
      // Find percentage looking at object
      // Apply curve to value
      // Apply result to range from min-max
      // Apply result to volume

      // Find angle of camera to this GameObject as a percentage where 1.0 is
      //   directly looking at it and 0.0 is 90 degrees or more away
      var vecCamToObject = (transform.position - _camTrans.position).normalized;
      var lookPct = Mathf.Max(0f, Vector3.Dot(_camTrans.forward, vecCamToObject));
      var curvedLookPct = curve.Evaluate(lookPct);
      var range = max - min;
      return min + curvedLookPct * range;
    }
  }

}
