namespace Airmail {

  using System.Collections;
  using System.Collections.Generic;
  using UnityEngine;
  using UnityEngine.Audio;

  public class ColorFlicker : MonoBehaviour {
    [System.Serializable]
    public class MixerData {
      public AudioMixer mixer;
      public string parameterName;
    }

    [Header("Effect Duration")]
    public float duration;
    public bool playOnAwake;
    public bool loop;

    [Header("Timing (if not following curve)")]
    public float minOffTime;
    public float maxOffTime;
    public float minDelayTime;
    public float maxDelayTime;

    [Header("Curve")]
    public bool followCurve;
    public AnimationCurve transitionCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Audio")]
    public List<AudioSource> audioSources;
    public float volumeTo;
    public List<MixerData> audioMixers;
    public float mixerTo;

    [Header("Lights")]
    public List<Light> lightSources;
    public Color lightTo;

    [Header("Material color")]
    public bool flickerMaterial;
    public Color materialTo;

    [Header("Emission color")]
    public bool flickerEmission;
    public Color emissionTo;


    private Dictionary<AudioSource, float> _audioFromVolumes;
    private Dictionary<MixerData, float> _audioMixerFromValues;
    private Dictionary<Light, Color> _lightFromColors;
    private Color _materialFrom;
    private Color _emissionFrom;
    private MeshRenderer _renderer;
    private float _offPercent;
    private bool _isFlickering;

    void Awake() {
      _renderer = GetComponent<MeshRenderer>();
      _audioFromVolumes = new Dictionary<AudioSource, float>();
      audioSources.ForEach(audio => _audioFromVolumes.Add(audio, audio.volume));
      _audioMixerFromValues = new Dictionary<MixerData, float>();
      audioMixers.ForEach(mixerData => {
        float value;
        mixerData.mixer.GetFloat(mixerData.parameterName, out value);
        _audioMixerFromValues.Add(mixerData, value);
      });
      _lightFromColors = new Dictionary<Light, Color>();
      lightSources.ForEach(light => _lightFromColors.Add(light, light.color));
      _materialFrom = _renderer.material.color;
      _emissionFrom = _renderer.material.GetColor("_EmissionColor");

      if (loop) duration = Mathf.Infinity;

    }

    void OnEnable() {
      if (playOnAwake) Go();
    }

    void OnDisable() {
      StopCoroutine("StartCurve");
      StopCoroutine("StartFlicker");
    }

    public void Go() {
      if (followCurve) {
        StartCoroutine("StartCurve");
      } else {
        StartCoroutine("StartFlicker");
      }
    }

    public void StartFlicker() {
      if (_isFlickering) return;
      StartCoroutine("doFlicker");
    }

    public void StartCurve() {
      if (_isFlickering) return;
      StartCoroutine("doCurve");
    }

    public void StopFlicker() {
      if (_isFlickering) return;
      StopCoroutine("doFlicker");
    }

    public void StopCurve() {
      if (_isFlickering) return;
      StopCoroutine("doCurve");
    }

    IEnumerator doCurve() {
      _isFlickering = true;
      var elapsedTime = 0f;

      while (elapsedTime < duration) {
        yield return null;
        elapsedTime += Time.deltaTime;
        _offPercent = 1f - transitionCurve.Evaluate(elapsedTime / duration);
        updateAll();
      }

      _isFlickering = false;
      _offPercent = 0f;
      updateAll();
    }

    IEnumerator doFlicker() {
      _isFlickering = true;
      var elapsedTime = 0f;

      while (elapsedTime < duration) {
        var minTime = _offPercent > 0f ? minOffTime : minDelayTime;
        var maxTime = _offPercent > 0f ? maxOffTime : maxDelayTime;
        var delay = Random.Range(minTime, maxTime);

        yield return new WaitForSeconds(delay);
        elapsedTime += delay;
        _offPercent = Mathf.Abs(_offPercent - 1f);

        updateAll();
      }

      _isFlickering = false;
      _offPercent = 0f;
      updateAll();
    }

    private void updateAll() { updateAll(_offPercent); }
    private void updateAll(float offPercent) {
      setAllAudioVolumes(offPercent);
      setAllMixerValues(offPercent);
      setAllLightColors(offPercent);
      setMaterialColor(offPercent);
      setEmissionColor(offPercent);
    }

    private void setAllAudioVolumes() { setAllAudioVolumes(_offPercent); }
    private void setAllAudioVolumes(float offPercent) {
      audioSources.ForEach(audio => {
        var fromVolume = _audioFromVolumes[audio];
        audio.volume = Mathf.Lerp(fromVolume, volumeTo, offPercent);
      });
    }

    private void setAllMixerValues() { setAllMixerValues(_offPercent); }
    private void setAllMixerValues(float offPercent) {
      audioMixers.ForEach(mixerData => {
        var fromValue = _audioMixerFromValues[mixerData];
        var newValue = Mathf.Lerp(fromValue, mixerTo, offPercent);
        mixerData.mixer.SetFloat(mixerData.parameterName, newValue);
      });
    }

    private void setAllLightColors() { setAllLightColors(_offPercent); }
    private void setAllLightColors(float offPercent) {
      lightSources.ForEach(light => {
        var fromColor = _lightFromColors[light];
        light.color = Color.Lerp(fromColor, lightTo, offPercent);
      });
    }

    private void setMaterialColor() { setMaterialColor(_offPercent); }
    private void setMaterialColor(float offPercent) {
      if (!flickerMaterial) return;
      _renderer.material.color = Color.Lerp(_materialFrom, materialTo, offPercent);
    }

    private void setEmissionColor() { setEmissionColor(_offPercent); }
    private void setEmissionColor(float offPercent) {
      if (!flickerEmission) return;
      _renderer.material.SetColor("_EmissionColor", Color.Lerp(_emissionFrom, emissionTo, offPercent));
    }
  }

}
