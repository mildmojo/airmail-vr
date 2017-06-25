namespace Airmail {

  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;

  public class AudioLooper : MonoBehaviour {
    [Tooltip("Time to wait from start of game before beginning to play")]
    public float startDelay;
    [Tooltip("Minimum delay between end of clip and start of next loop")]
    public float minInterval;
    [Tooltip("Maximum delay between end of clip and start of next loop")]
    public float maxInterval;
    [Tooltip("Vary pitch +/- this amount on each loop")]
    public float pitchVariance;

    // Use this for initialization
    IEnumerator Start () {
      var audioSource = GetComponent<AudioSource>();

      yield return new WaitForSeconds(startDelay);

      while (true) {
        audioSource.pitch = 1f + Random.Range(-pitchVariance/2f, pitchVariance/2f);
        audioSource.Play();

        var nextDelay = audioSource.clip.length * audioSource.pitch + Random.Range(minInterval, maxInterval);
        yield return new WaitForSeconds(nextDelay);
      }
    }
  }

}
