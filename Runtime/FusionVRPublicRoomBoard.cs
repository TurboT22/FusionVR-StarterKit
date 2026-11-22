using TMPro;
using UnityEngine;

namespace Fusion.VR.StarterKit
{
    /// <summary>Pick a queue and join a random public room.</summary>
    [AddComponentMenu("FusionVR/Starter Kit/Public Room Board")]
    public class FusionVRPublicRoomBoard : MonoBehaviour
    {
        public string[] Queues = { "Default", "Chill", "Party" };
        public int MaxPlayers = 10;
        public TMP_Text QueueText;
        public TMP_Text StatusText;
        public FusionVRInteractableButton PrevButton;
        public FusionVRInteractableButton NextButton;
        public FusionVRInteractableButton JoinButton;

        private int _index;

        private void OnEnable()
        {
            if (PrevButton != null) PrevButton.OnButtonPressed.AddListener(Prev);
            if (NextButton != null) NextButton.OnButtonPressed.AddListener(Next);
            if (JoinButton != null) JoinButton.OnButtonPressed.AddListener(Join);

            FusionVRManager.OnJoinedRoom += ShowRoom;
            FusionVRManager.OnPlayerJoinedRoom += HandlePlayer;
            FusionVRManager.OnPlayerLeftRoom += HandlePlayer;
            FusionVRManager.OnLeftRoom += HandleLeft;
            FusionVRManager.OnError += HandleError;

            Refresh();
            if (FusionVRManager.IsInRoom) ShowRoom();
            else SetStatus(FusionVRManager.IsConnected ? "READY" : "OFFLINE");
        }

        private void OnDisable()
        {
            if (PrevButton != null) PrevButton.OnButtonPressed.RemoveListener(Prev);
            if (NextButton != null) NextButton.OnButtonPressed.RemoveListener(Next);
            if (JoinButton != null) JoinButton.OnButtonPressed.RemoveListener(Join);

            FusionVRManager.OnJoinedRoom -= ShowRoom;
            FusionVRManager.OnPlayerJoinedRoom -= HandlePlayer;
            FusionVRManager.OnPlayerLeftRoom -= HandlePlayer;
            FusionVRManager.OnLeftRoom -= HandleLeft;
            FusionVRManager.OnError -= HandleError;
        }

        private string Queue => Queues != null && Queues.Length > 0 ? Queues[Mathf.Clamp(_index, 0, Queues.Length - 1)] : "Default";

        private void Prev()
        {
            if (Queues == null || Queues.Length == 0) return;
            _index = (_index + Queues.Length - 1) % Queues.Length;
            Refresh();
        }

        private void Next()
        {
            if (Queues == null || Queues.Length == 0) return;
            _index = (_index + 1) % Queues.Length;
            Refresh();
        }

        private void Join()
        {
            SetStatus("JOINING " + Queue.ToUpperInvariant() + "...");
            FusionVRManager.JoinRandomRoom(Queue, MaxPlayers);
        }

        private void Refresh()
        {
            if (QueueText != null) QueueText.text = Queue.ToUpperInvariant();
        }

        private void ShowRoom() => SetStatus(FusionVRManager.PlayerCount + " PLAYERS IN ROOM");
        private void HandlePlayer(PlayerRef player) { if (FusionVRManager.IsInRoom) ShowRoom(); }
        private void HandleLeft() => SetStatus("LEFT ROOM");
        private void HandleError(string message) => SetStatus("ERROR");

        private void SetStatus(string message)
        {
            if (StatusText != null) StatusText.text = message;
        }
    }
}
