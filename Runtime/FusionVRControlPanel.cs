using TMPro;
using UnityEngine;

namespace Fusion.VR.StarterKit
{
    /// <summary>Voice and room controls: mute, deafen and leave room, with status lamps.</summary>
    [AddComponentMenu("FusionVR/Starter Kit/Control Panel")]
    public class FusionVRControlPanel : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

        public FusionVRInteractableButton MuteButton;
        public FusionVRInteractableButton DeafenButton;
        public FusionVRInteractableButton LeaveButton;
        public Renderer MuteLamp;
        public Renderer DeafenLamp;
        public TMP_Text StatusText;
        public Color OnColour = new Color(0.3f, 0.9f, 0.3f);
        public Color OffColour = new Color(0.9f, 0.2f, 0.15f);

        private MaterialPropertyBlock _block;
        private int _lastState = -1;

        private void OnEnable()
        {
            if (MuteButton != null) MuteButton.OnButtonPressed.AddListener(OnMute);
            if (DeafenButton != null) DeafenButton.OnButtonPressed.AddListener(OnDeafen);
            if (LeaveButton != null) LeaveButton.OnButtonPressed.AddListener(OnLeave);
            _lastState = -1;
        }

        private void OnDisable()
        {
            if (MuteButton != null) MuteButton.OnButtonPressed.RemoveListener(OnMute);
            if (DeafenButton != null) DeafenButton.OnButtonPressed.RemoveListener(OnDeafen);
            if (LeaveButton != null) LeaveButton.OnButtonPressed.RemoveListener(OnLeave);
        }

        private void Update()
        {
            FusionUniVoiceTransport voice = FusionUniVoiceTransport.Instance;
            bool muted = voice != null && voice.IsMuted;
            bool deaf = voice != null && voice.IsDeafened;

            int state = (muted ? 1 : 0) | (deaf ? 2 : 0) | (voice == null ? 4 : 0);
            if (state == _lastState) return;
            _lastState = state;

            SetLamp(MuteLamp, !muted);
            SetLamp(DeafenLamp, !deaf);
            if (StatusText != null)
            {
                StatusText.text = voice == null ? "VOICE OFF" : (deaf ? "DEAFENED" : (muted ? "MIC MUTED" : "MIC LIVE"));
            }
        }

        private void OnMute() => FusionVRManager.ToggleMute();

        private void OnDeafen()
        {
            FusionUniVoiceTransport voice = FusionUniVoiceTransport.Instance;
            if (voice != null) FusionVRManager.SetDeafened(!voice.IsDeafened);
        }

        private void OnLeave() => FusionVRManager.LeaveRoom();

        private void SetLamp(Renderer lamp, bool on)
        {
            if (lamp == null) return;
            if (_block == null) _block = new MaterialPropertyBlock();

            Color c = on ? OnColour : OffColour;
            lamp.GetPropertyBlock(_block);
            _block.SetColor(BaseColorId, c);
            _block.SetColor(ColorId, c);
            _block.SetColor(EmissionId, c * 1.6f);
            lamp.SetPropertyBlock(_block);
        }
    }
}
