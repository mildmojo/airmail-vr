namespace Airmail {

  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using UnityEngine;

  [RequireComponent(typeof(AudioSource))]
  public class AudioBucket : MonoBehaviour {
    public float pitch = 1f;
    public float pitchVariance;
    public bool playOnAwake;
    public bool loop;
    public List<AudioClip> clips;

    private ShuffleDeck _deck;
    private AudioSource _audioSource;
    private bool _loopingAllowed;

    void Start() {
      _deck = new ShuffleDeck(clips);
      _audioSource = GetComponent<AudioSource>();
      if (playOnAwake) Play();
    }

    void Update() {
      if (loop && _loopingAllowed && !_audioSource.isPlaying) Play();
    }

    public void PlayDelayed(float delay) {
      LeanTween.delayedCall(delay, () => Play());
    }

    public void Play() {
      _loopingAllowed = true;
      _audioSource.pitch = pitch + Random.Range(-pitchVariance/2f, pitchVariance/2f);
      if (_audioSource.isPlaying) {
        FadeIn();
      } else {
        // HardStop();
        _audioSource.clip = (AudioClip) _deck.Draw();
        _audioSource.Play();
        _audioSource.volume = 1f;
      }
    }

    public void PlaySilent() {
      if (_audioSource.isPlaying) {
        FadeOut();
      } else {
        Play();
        _audioSource.volume = 0f;
      }
    }

    // TODO: Bug: If FadeStop is called on a looping bucket when clip has less than
    //   0.2s left (fade duration), clip may end and briefly start again at full
    //   volume before fade out completes.
    public void FadeStop(System.Action done = null) {
      FadeOut(() => {
        HardStop();
        if (done != null) done.Invoke();
      });
    }

    public void FadeIn() { FadeIn(null); }
    public void FadeIn(System.Action done) {
      LeanTween.value(gameObject, val => _audioSource.volume = val, _audioSource.volume, 1f, 0.2f)
        .setOnComplete(() => {
          if (done != null) done.Invoke();
        });
    }

    public void FadeOut() { FadeOut(null); }
    public void FadeOut(System.Action done) {
      LeanTween.value(gameObject, val => _audioSource.volume = val, _audioSource.volume, 0f, 0.2f)
        .setOnComplete(() => {
          if (done != null) done.Invoke();
        });
    }

    public void HardStop() {
      _loopingAllowed = false;
      _audioSource.Stop();
    }
  }

}
