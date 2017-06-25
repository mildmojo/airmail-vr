using UnityEngine;
using System.Collections;

public class ScreenFader : MonoBehaviour {
  public static ScreenFader Instance;

  private CanvasGroup _cg;

  void Awake() {
    Instance = this;
    _cg = GetComponent<CanvasGroup>();
  }



  public void FadeIn(float time = 0.5f, System.Action onComplete = null) {
    Fade(time, onComplete, 0f);
  }

  public void FadeOut(float time = 0.5f, System.Action onComplete = null) {
    Fade(time, onComplete, 1f);
  }

  private void Fade(float time, System.Action onComplete, float targetAlpha) {
    LeanTween.cancel(gameObject);
    if (time < 0.1f) {
      _cg.alpha = targetAlpha;
    } else {
      var tween = LeanTween.value(gameObject, val => _cg.alpha = val, _cg.alpha, targetAlpha, time)
      .setEase(LeanTweenType.easeInOutExpo);
      if (onComplete != null) {
        tween.setOnComplete(onComplete);
      }
    }
  }
}
