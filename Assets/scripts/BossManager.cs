namespace Airmail {

  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;
  using UnityEngine.Audio;
  using UnityEngine.UI;
  using UnityEngine.SceneManagement;

  public class BossManager : MonoBehaviour {
    public static BossManager Instance;

    public LayerMask interactables;
    public AudioMixer masterMixer;
    public float audioFadeInTime;
    public float audioFadeOutTime;
    public float screenFadeInTime;
    public float screenFadeOutTime;

    [System.NonSerialized]
    public bool isFinale;

    private ScreenFader _screenFader;
    private GvrViewer _gvrViewer;

    void Awake() {
      Instance = this;
    }

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

    public void OnQuit() {
      // Fade out audio.
      LeanTween.value(gameObject, val => masterMixer.SetFloat("volume", val), 0f, -80f, audioFadeOutTime)
        .setEaseInCirc();
      // Fade out visuals.
      _screenFader.FadeOut(screenFadeOutTime, DoQuit);
    }

    void DoQuit() {
      // Only quit if this is the only scene loaded. Otherwise load scene 0.
      if (SceneManager.GetSceneAt(0) != SceneManager.GetActiveScene()) {
       SceneManager.LoadScene(0);
      } else {
        Application.Quit();
      }
    }
  }

}
