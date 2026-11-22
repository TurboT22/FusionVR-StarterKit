using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Fusion.VR.StarterKit
{
    /// <summary>Parses key object names such as "Key_A" or "Key_Backspace" into key codes.</summary>
    public static class FusionVRKeyNames
    {
        /// <summary>Returns a key code (A-Z, 0-9, BACKSPACE, CLEAR, ENTER, SHIFT, SPACE) or null.</summary>
        public static string Parse(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName)) return null;

            string n = objectName.Trim();
            int sep = n.LastIndexOfAny(new[] { '_', ' ', '-' });
            string k = (sep >= 0 ? n.Substring(sep + 1) : n).ToUpperInvariant();

            switch (k)
            {
                case "DEL":
                case "DELETE":
                case "BACK":
                case "BACKSPACE": return "BACKSPACE";
                case "CLR":
                case "CLEAR": return "CLEAR";
                case "ENTER":
                case "SAVE":
                case "GO": return "ENTER";
                case "SHIFT": return "SHIFT";
                case "SPACE": return "SPACE";
            }
            return k.Length == 1 && k[0] < 128 && char.IsLetterOrDigit(k[0]) ? k : null;
        }
    }

    /// <summary>Binds every child <see cref="FusionVRInteractableButton"/> named "Key_*" to a callback.</summary>
    public sealed class FusionVRKeyBindings
    {
        private readonly List<FusionVRInteractableButton> _buttons = new List<FusionVRInteractableButton>();
        private readonly List<UnityAction> _actions = new List<UnityAction>();

        /// <summary>Binds all key buttons under <paramref name="root"/>.</summary>
        public void Bind(Transform root, Action<string> onKey)
        {
            Unbind();
            foreach (FusionVRInteractableButton button in root.GetComponentsInChildren<FusionVRInteractableButton>(true))
            {
                string key = FusionVRKeyNames.Parse(button.name);
                if (key == null) continue;

                string captured = key;
                UnityAction action = () => onKey(captured);
                button.OnButtonPressed.AddListener(action);
                _buttons.Add(button);
                _actions.Add(action);
            }
        }

        /// <summary>Removes all listeners added by <see cref="Bind"/>.</summary>
        public void Unbind()
        {
            for (int i = 0; i < _buttons.Count; i++)
            {
                if (_buttons[i] != null) _buttons[i].OnButtonPressed.RemoveListener(_actions[i]);
            }
            _buttons.Clear();
            _actions.Clear();
        }
    }
}
