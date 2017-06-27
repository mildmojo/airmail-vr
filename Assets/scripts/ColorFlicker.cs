namespace Airmail {

  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;

  public class ColorFlicker : MonoBehaviour {
    public Color colorTo;
    public float dutyCycle;
    public float time;

    private Color colorFrom;
    private MeshRenderer _renderer;
    private bool _isFlickering;

    void Awake() {
      _renderer = GetComponent<MeshRenderer>();
      colorFrom = _renderer.material.color;
    }

    public void Flicker() {
      if (_isFlickering) return;

      StartCoroutine(doFlicker());
    }

    IEnumerator doFlicker() {
      _isFlickering = true;
      var elapsedTime = 0f;

      // TODO: This doesn't seem to work.
      _renderer.material.color = colorTo;
Debug.Log("Flickering!");
      while (elapsedTime < 5f) {
        var delay = Random.Range(0.1f, 1f);
        elapsedTime += delay;
        yield return new WaitForSeconds(delay);
        _renderer.material.color = _renderer.material.color == colorFrom ? colorTo : colorFrom;
      }
Debug.Log("Not flickering!");
      _isFlickering = false;
    }
  }

}
