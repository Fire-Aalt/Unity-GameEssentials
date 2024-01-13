using Cysharp.Threading.Tasks;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

namespace RenderDream.GameEssentials
{
    public class HDRColorComponent : MonoBehaviour
    {
        public readonly string[] TEXTURE_PROPERTY_NAMES = new string[2] { "_MainTex", "_TexelSize" };

        [Title("Settings")]
        [SerializeField] private ControllerMode _controllerMode = ControllerMode.Automatic;
        [SerializeField, ReadOnly, ShowIf("@this._renderer != null")] private Renderer _renderer;
        [SerializeField, ReadOnly, ShowIf("@this._image != null")] private Image _image;
        [SerializeField, ReadOnly] private Material _material;

        [Title("Automatic Property Names")]
        [SerializeField, ReadOnly, ShowIf("_controllerMode", ControllerMode.Automatic)] 
        private List<string> _colorPropertyNames = new();

        [Title("Manual Property Names")]
        [SerializeField, ValidateInput("HDRColorsExist"), ShowIf("_controllerMode", ControllerMode.Manual)]
        private List<string> _manualColorPropertyNames = new();

        private MaterialColorProperty[] _colorProperties;
        private CancellationTokenSource _cts;

        public List<string> ColorPropertyNames {
            get {
                    if (_controllerMode == ControllerMode.Automatic)
                        return _colorPropertyNames;
                    else
                        return _manualColorPropertyNames;
                }
            }

        private void OnValidate()
        {
            if (_material == null ||
                (_renderer != null && _renderer.sharedMaterial != _material) 
                || (_image != null && _image.material != _material))
            {
                if (_renderer != null || TryGetComponent(out _renderer))
                    _material = _renderer.sharedMaterial;
                else if (_image != null || TryGetComponent(out _image))
                    _material = _image.material;
                else
                    Debug.LogWarning("RendererComponent was not found on " + gameObject);
            }
            if (_material != null && _controllerMode == ControllerMode.Automatic)
                GetMaterialColorProperties(_material);
        }

        private void Awake()
        {
            if (_renderer != null)
            {
                _renderer.material = new(_material);
                _material = _renderer.material;
            }
            else if (_image != null)
            {
                _image.material = new(_material);
                _material = _image.material;
            }

            _colorProperties = new MaterialColorProperty[ColorPropertyNames.Count];
            for (int i = 0; i < ColorPropertyNames.Count; i++)
            {
                _colorProperties[i] = new MaterialColorProperty(_material, ColorPropertyNames[i]);
            }
        }

        #region Checks + Init
        public void GetMaterialColorProperties(Material material)
        {
            _colorPropertyNames.Clear();

            foreach (var property in material.GetPropertyNames(MaterialPropertyType.Vector))
            {
                if (HDRColorExists(property))
                {
                    _colorPropertyNames.Add(property);
                }
            }
        }

        private bool HDRColorExists(string propertyName)
        {
            if (propertyName.Contains("Color") && _material.HasColor(propertyName))
            {
                for (int i = 0; i < TEXTURE_PROPERTY_NAMES.Length; i++)
                {
                    if (propertyName.Contains(TEXTURE_PROPERTY_NAMES[i]))
                        return false;
                }

                Color color = _material.GetColor(propertyName);
                if (color.maxColorComponent > 1f)
                {
                    return true;
                }
            }
            return false;
        }

        private bool HDRColorsExist(List<string> propertyNames)
        {
            foreach (var property in propertyNames) 
            {
                if (!HDRColorExists(property))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion


        public async UniTaskVoid LerpIntensityValue(TimescaleModes timescale, FinalIntensity lerpMode, float duration)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            float[] startValues = new float[_colorProperties.Length];
            float[] endValues = new float[_colorProperties.Length];
            for (int i = 0; i < _colorProperties.Length; i++)
            {
                startValues[i] = _colorProperties[i].Intensity;
                switch (lerpMode)
                {
                    case FinalIntensity.Zero:
                        if (_image != null)
                        {
                            endValues[i] = -10f;
                        }
                        else
                        {
                            endValues[i] = 0f;
                        }
                        break;
                    case FinalIntensity.Default:
                        endValues[i] = _colorProperties[i].defaultIntensity;
                        break;
                }
            }

            float elapsedTime = 0f;
            while (elapsedTime <= duration)
            {
                if (timescale == TimescaleModes.Unscaled)
                    elapsedTime += Time.unscaledDeltaTime;
                else
                    elapsedTime += Time.deltaTime;

                for (int i = 0; i < _colorProperties.Length; i++)
                {
                    float value = Mathf.Lerp(startValues[i], endValues[i], elapsedTime / duration);
                    _colorProperties[i].Intensity = value;
                }

                await UniTask.Yield(_cts.Token);
            }
        }

        private void OnEnable()
        {
            HDRColorManager.HDRControllers.Add(this);
        }

        private void OnDisable()
        {
            HDRColorManager.HDRControllers.Remove(this);
        }

        public enum ControllerMode
        {
            Automatic,
            Manual
        }

        public enum FinalIntensity
        {
            Zero,
            Default
        }
    }
}
