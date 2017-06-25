namespace airmail {
  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;
  using UnityEngine.Audio;
  using UnityEngine.UI;

  public class BossManager : MonoBehaviour {
    public LayerMask interactables;
    public AudioMixer masterMixer;
    public float audioFadeInTime;
    public float audioFadeOutTime;
    public float screenFadeInTime;
    public float screenFadeOutTime;

    private ScreenFader _screenFader;
    private GvrViewer _gvrViewer;

    void Start () {
      var reticle = Camera.main.GetComponent<GvrPointerPhysicsRaycaster>();
      reticle.eventMask = interactables;

      // Fade in audio
      LeanTween.value(gameObject, val => masterMixer.SetFloat("volume", val), -80f, 0f, audioFadeInTime)
        .setEaseOutCirc();

      // Fade in visuals
      _screenFader = ScreenFader.Instance;
      _screenFader.FadeIn(screenFadeInTime);
      _gvrViewer = GvrViewer.Instance;
    }

    void Update () {
      // Quit on Escape or Back (mobile)
      var deviceBackPressed = _gvrViewer != null && _gvrViewer.BackButtonPressed;
      var keyQuitPressed = Input.GetKeyDown(KeyCode.Escape);
      if (keyQuitPressed || deviceBackPressed) OnQuit();
    }

    void OnQuit() {
      LeanTween.value(gameObject, val => masterMixer.SetFloat("volume", val), 0f, -80f, audioFadeOutTime)
        .setEaseInCirc();
      _screenFader.FadeOut(2f, () => Application.Quit());
    }
  }
}
