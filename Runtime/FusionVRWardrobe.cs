using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Fusion.VR.StarterKit
{
    /// <summary>
    /// Cosmetic wardrobe. Pick a body part and a cosmetic ID (read from the player's "Cosmetics" containers)
    /// and equip or remove it with <see cref="FusionVRManager.SetCosmetic"/>.
    /// </summary>
    [AddComponentMenu("FusionVR/Starter Kit/Wardrobe")]
    public class FusionVRWardrobe : MonoBehaviour
    {
        public TMP_Text PartText;
        public TMP_Text CosmeticText;
        public TMP_Text StatusText;
        public FusionVRInteractableButton PrevPartButton;
        public FusionVRInteractableButton NextPartButton;
        public FusionVRInteractableButton PrevItemButton;
        public FusionVRInteractableButton NextItemButton;
        public FusionVRInteractableButton EquipButton;
        public FusionVRInteractableButton RemoveButton;
        [Tooltip("Used to list cosmetics before the local player has spawned.")] public GameObject PlayerPrefabTemplate;

        private const string NoneLabel = "(NONE)";
        private int _part;
        private int _item;
        private readonly List<string> _items = new List<string>();

        private void OnEnable()
        {
            Hook(PrevPartButton, PrevPart);
            Hook(NextPartButton, NextPart);
            Hook(PrevItemButton, PrevItem);
            Hook(NextItemButton, NextItem);
            Hook(EquipButton, Equip);
            Hook(RemoveButton, Remove);
            FusionVRManager.OnJoinedRoom += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            Unhook(PrevPartButton, PrevPart);
            Unhook(NextPartButton, NextPart);
            Unhook(PrevItemButton, PrevItem);
            Unhook(NextItemButton, NextItem);
            Unhook(EquipButton, Equip);
            Unhook(RemoveButton, Remove);
            FusionVRManager.OnJoinedRoom -= Refresh;
        }

        private static void Hook(FusionVRInteractableButton b, UnityEngine.Events.UnityAction a)
        {
            if (b != null) b.OnButtonPressed.AddListener(a);
        }

        private static void Unhook(FusionVRInteractableButton b, UnityEngine.Events.UnityAction a)
        {
            if (b != null) b.OnButtonPressed.RemoveListener(a);
        }

        private string CurrentPart => FusionVRCustomization.BodyParts[_part];

        private void PrevPart() { _part = (_part + FusionVRCustomization.BodyParts.Length - 1) % FusionVRCustomization.BodyParts.Length; _item = 0; Refresh(); }
        private void NextPart() { _part = (_part + 1) % FusionVRCustomization.BodyParts.Length; _item = 0; Refresh(); }
        private void PrevItem() { if (_items.Count > 0) _item = (_item + _items.Count - 1) % _items.Count; Refresh(); }
        private void NextItem() { if (_items.Count > 0) _item = (_item + 1) % _items.Count; Refresh(); }

        private void Equip()
        {
            if (_items.Count == 0) return;
            string id = _items[_item];
            FusionVRManager.SetCosmetic(CurrentPart, id == NoneLabel ? string.Empty : id);
            Refresh();
        }

        private void Remove()
        {
            FusionVRManager.SetCosmetic(CurrentPart, string.Empty);
            Refresh();
        }

        /// <summary>Rebuilds the item list and the displays.</summary>
        public void Refresh()
        {
            Transform source = FusionVRManager.LocalPlayer != null
                ? FusionVRManager.LocalPlayer.transform
                : (PlayerPrefabTemplate != null ? PlayerPrefabTemplate.transform : null);

            _items.Clear();
            _items.Add(NoneLabel);
            if (source != null) _items.AddRange(FusionVRCustomization.GetCosmeticIds(source, CurrentPart));
            _item = Mathf.Clamp(_item, 0, _items.Count - 1);

            string equipped = string.Empty;
            FusionVRManager.LocalCosmetics.TryGetValue(CurrentPart, out equipped);
            equipped = equipped ?? string.Empty;

            string shown = _items[_item];
            bool isEquipped = shown == NoneLabel ? equipped.Length == 0 : shown == equipped;

            if (PartText != null) PartText.text = CurrentPart.ToUpperInvariant();
            if (CosmeticText != null) CosmeticText.text = shown.ToUpperInvariant();
            if (StatusText != null)
            {
                StatusText.text = _items.Count <= 1 ? "NO COSMETICS FOUND" : (isEquipped ? "EQUIPPED" : (_item + 1) + " / " + _items.Count);
            }
        }
    }
}
