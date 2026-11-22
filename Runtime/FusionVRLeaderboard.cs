using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace Fusion.VR.StarterKit
{
    /// <summary>Live leaderboard of everyone in the room, ranked by <see cref="FusionVRPlayer.Score"/>.</summary>
    [AddComponentMenu("FusionVR/Starter Kit/Leaderboard")]
    public class FusionVRLeaderboard : MonoBehaviour
    {
        public TMP_Text TitleText;
        public TMP_Text RanksText;
        public TMP_Text NamesText;
        public TMP_Text ScoresText;
        public TMP_Text FooterText;
        [Min(1)] public int MaxRows = 8;
        public float RefreshInterval = 0.5f;
        public string Title = "TOP PLAYERS";

        private struct Row
        {
            public string Name;
            public int Score;
            public Color Colour;
            public bool Local;
        }

        private readonly List<Row> _rows = new List<Row>();
        private readonly StringBuilder _ranks = new StringBuilder();
        private readonly StringBuilder _names = new StringBuilder();
        private readonly StringBuilder _scores = new StringBuilder();
        private float _timer;

        private void OnEnable()
        {
            if (TitleText != null) TitleText.text = Title;
            Refresh();
        }

        private void Update()
        {
            _timer -= Time.unscaledDeltaTime;
            if (_timer > 0f) return;
            _timer = RefreshInterval;
            Refresh();
        }

        /// <summary>Rebuilds the board from the current players.</summary>
        public void Refresh()
        {
            _rows.Clear();
            foreach (KeyValuePair<PlayerRef, FusionVRPlayer> pair in FusionVRManager.Players)
            {
                FusionVRPlayer p = pair.Value;
                if (p == null || p.Object == null || !p.Object.IsValid) continue;
                _rows.Add(new Row { Name = p.Username.ToString(), Score = p.Score, Colour = p.Colour, Local = p.IsLocalPlayer });
            }

            _rows.Sort((a, b) =>
            {
                int c = b.Score.CompareTo(a.Score);
                return c != 0 ? c : string.CompareOrdinal(a.Name, b.Name);
            });

            _ranks.Clear();
            _names.Clear();
            _scores.Clear();

            int count = Mathf.Min(MaxRows, _rows.Count);
            for (int i = 0; i < count; i++)
            {
                Row r = _rows[i];
                Color shown = new Color(r.Colour.r, r.Colour.g, r.Colour.b, 1f);
                _ranks.Append(i + 1).Append('.').Append('\n');
                _names.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(shown)).Append('>');
                if (r.Local) _names.Append("<b>");
                _names.Append(r.Name);
                if (r.Local) _names.Append("</b>");
                _names.Append("</color>\n");
                _scores.Append(r.Score).Append('\n');
            }

            if (count == 0) _names.Append("NO PLAYERS YET");

            if (RanksText != null) RanksText.text = _ranks.ToString();
            if (NamesText != null) NamesText.text = _names.ToString();
            if (ScoresText != null) ScoresText.text = _scores.ToString();
            if (FooterText != null)
            {
                FooterText.text = FusionVRManager.IsInRoom
                    ? FusionVRManager.PlayerCount + " IN " + FusionVRManager.CurrentRoomName
                    : "NOT IN A ROOM";
            }
        }
    }
}
