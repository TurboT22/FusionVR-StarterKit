using TMPro;
using UnityEngine;

namespace Fusion.VR.StarterKit
{
    /// <summary>Shows the player's current colour on preview renderers and offers a random-colour button.</summary>
    [AddComponentMenu("FusionVR/Starter Kit/Colour Station")]
    public class FusionVRColourStation : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public Renderer[] PreviewRenderers;
        public TMP_Text LabelText;
        public FusionVRInteractableButton RandomButton;

        private MaterialPropertyBlock _block;
        private Color _shown = new Color(-1f, -1f, -1f, -1f);

        private void OnEnable()
        {
            if (RandomButton != null) RandomButton.OnButtonPressed.AddListener(RandomiseColour);
            Apply(FusionVRManager.LocalColour);
        }

        private void OnDisable()
        {
            if (RandomButton != null) RandomButton.OnButtonPressed.RemoveListener(RandomiseColour);
        }

        private void Update()
        {
            Color current = FusionVRManager.LocalColour;
            if (current != _shown) Apply(current);
        }

        /// <summary>Picks a random vivid colour and applies it to the player.</summary>
        public void RandomiseColour()
        {
            FusionVRManager.SetColour(Color.HSVToRGB(Random.value, Random.Range(0.6f, 1f), Random.Range(0.8f, 1f)));
        }

        private void Apply(Color colour)
        {
            _shown = colour;
            if (_block == null) _block = new MaterialPropertyBlock();

            if (PreviewRenderers != null)
            {
                foreach (Renderer r in PreviewRenderers)
                {
                    if (r == null) continue;
                    r.GetPropertyBlock(_block);
                    _block.SetColor(BaseColorId, colour);
                    _block.SetColor(ColorId, colour);
                    r.SetPropertyBlock(_block);
                }
            }
            if (LabelText != null) LabelText.text = "#" + ColorUtility.ToHtmlStringRGB(colour);
        }
    }
}
