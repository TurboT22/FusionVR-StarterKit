using TMPro;
using UnityEngine;

namespace Fusion.VR.StarterKit
{
    /// <summary>On-world keyboard that lets the player type and save a new username.</summary>
    [AddComponentMenu("FusionVR/Starter Kit/Name Station")]
    public class FusionVRNameStation : MonoBehaviour
    {
        public TMP_Text NameText;
        public TMP_Text StatusText;
        [Range(1, 15)] public int MaxLength = 12;

        private readonly FusionVRKeyBindings _bindings = new FusionVRKeyBindings();
        private string _name = string.Empty;
        private bool _lowercase;
        private bool _cursor;
        private float _blink;

        private void OnEnable()
        {
            _name = FusionVRManager.LocalUsername;
            _bindings.Bind(transform, HandleKey);
            SetStatus("TYPE YOUR NAME");
            Refresh();
        }

        private void OnDisable() => _bindings.Unbind();

        private void Update()
        {
            if (Time.unscaledTime < _blink) return;
            _blink = Time.unscaledTime + 0.5f;
            _cursor = !_cursor;
            Refresh();
        }

        private void HandleKey(string key)
        {
            switch (key)
            {
                case "BACKSPACE":
                    if (_name.Length > 0) _name = _name.Substring(0, _name.Length - 1);
                    break;
                case "CLEAR":
                    _name = string.Empty;
                    break;
                case "SHIFT":
                    _lowercase = !_lowercase;
                    SetStatus(_lowercase ? "LOWERCASE" : "UPPERCASE");
                    break;
                case "SPACE":
                    Append(" ");
                    break;
                case "ENTER":
                    Save();
                    break;
                default:
                    Append(_lowercase ? key.ToLowerInvariant() : key);
                    break;
            }
            Refresh();
        }

        private void Append(string value)
        {
            if (_name.Length < MaxLength) _name += value;
        }

        private void Save()
        {
            string trimmed = _name.Trim();
            if (trimmed.Length == 0)
            {
                SetStatus("NAME CAN'T BE EMPTY");
                return;
            }
            _name = trimmed;
            FusionVRManager.SetUsername(trimmed);
            SetStatus("SAVED: " + trimmed);
        }

        private void Refresh()
        {
            if (NameText != null) NameText.text = _name + (_cursor ? "_" : " ");
        }

        private void SetStatus(string message)
        {
            if (StatusText != null) StatusText.text = message;
        }
    }
}
