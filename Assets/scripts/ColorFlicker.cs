namespace Airmail {

  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;

  public class ColorFlicker : MonoBehaviour {
    public Color colorTo;
    public Color emissionTo;
    public float dutyCycle;
    public float time;

    private Color colorFrom;
    private Color emissionFrom;
    private MeshRenderer _renderer;
    private bool _isFlickering;

    void Awake() {
      _renderer = GetComponent<MeshRenderer>();
      colorFrom = _renderer.material.color;
      emissionFrom = _renderer.material.GetColor("_EmissionColor");
    }

    public void StartFlicker() {
      if (_isFlickering) return;

      StartCoroutine("doFlicker");
    }

    public void StopFlicker() {
      StopCoroutine("doFlicker");
    }

    IEnumerator doFlicker() {
      _isFlickering = true;
      var elapsedTime = 0f;

      // TODO: This doesn't seem to work.
      _renderer.material.color = colorTo;
Debug.Log("Flickering!");
      while (elapsedTime < 5f) {
        var delay = Random.Range(0.1f, 1f);
        yield return new WaitForSeconds(delay);
        elapsedTime += delay;
        _renderer.material.color = _renderer.material.color == colorFrom ? colorTo : colorFrom;
        var oldEmission = _renderer.material.GetColor("_EmissionColor");
        var newEmission = oldEmission == emissionFrom ? emissionTo : emissionFrom;
        _renderer.material.SetColor("_EmissionColor", newEmission);
      }
Debug.Log("Not flickering!");
      _isFlickering = false;
    }
  }

}
