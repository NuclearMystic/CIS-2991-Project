using CIS2991Project.Jobs;
using UnityEngine;

namespace CIS2991Project.UI
{
    // Active/Completed job journal, opened with the Journal toggle key.
    public sealed class JournalPanel
    {
        private readonly Texture2D _backgroundTexture;
        private readonly float _panelWidth;
        private readonly float _panelHeight;
        private readonly float _rowHeight;

        private int _tab;

        public JournalPanel(Texture2D backgroundTexture, float panelWidth, float panelHeight, float rowHeight)
        {
            _backgroundTexture = backgroundTexture;
            _panelWidth = panelWidth;
            _panelHeight = panelHeight;
            _rowHeight = rowHeight;
        }

        // Returns true if the Close button was clicked, so the caller can clear its visibility flag.
        public bool Draw()
        {
            var panelX = (GuiScale.ReferenceWidth - _panelWidth) / 2f;
            var panelY = (GuiScale.ReferenceHeight - _panelHeight) / 2f;

            GuiDrawUtils.DrawSlot(new Rect(panelX, panelY, _panelWidth, _panelHeight), _backgroundTexture);
            GUI.Label(new Rect(panelX, panelY + 8f, _panelWidth, 28f), "Job Journal", GuiDrawUtils.CenteredLabelStyle);

            const float tabWidth = 140f;
            const float tabHeight = 28f;
            var tabY = panelY + 40f;
            var activeTabRect = new Rect(panelX + _panelWidth / 2f - tabWidth - 4f, tabY, tabWidth, tabHeight);
            var completedTabRect = new Rect(panelX + _panelWidth / 2f + 4f, tabY, tabWidth, tabHeight);

            var previousEnabled = GUI.enabled;
            GUI.enabled = _tab != 0;
            if (GUI.Button(activeTabRect, "Active"))
            {
                _tab = 0;
            }
            GUI.enabled = _tab != 1;
            if (GUI.Button(completedTabRect, "Completed"))
            {
                _tab = 1;
            }
            GUI.enabled = previousEnabled;

            var listX = panelX + 16f;
            var listY = tabY + tabHeight + 12f;
            var listWidth = _panelWidth - 32f;

            if (_tab == 0)
            {
                DrawActiveTab(listX, listY, listWidth);
            }
            else
            {
                DrawCompletedTab(listX, listY, listWidth);
            }

            var closeRect = new Rect(panelX + _panelWidth - 76f, panelY + 8f, 60f, 24f);
            return GUI.Button(closeRect, "Close");
        }

        private void DrawActiveTab(float x, float y, float width)
        {
            var activeJobs = JobManager.ActiveJobs;
            if (activeJobs.Count == 0)
            {
                GUI.Label(new Rect(x, y, width, _rowHeight), "No active jobs.");
                return;
            }

            var rowY = y;
            foreach (var job in activeJobs)
            {
                GUI.Label(new Rect(x, rowY, width, 20f), job.jobName);
                var objective = $"Kill {job.killTargetTag}s: {JobManager.GetProgress(job)}/{job.killTargetCount}";
                GUI.Label(new Rect(x, rowY + 20f, width, 20f), objective);
                rowY += _rowHeight;
            }
        }

        private void DrawCompletedTab(float x, float y, float width)
        {
            var finishedJobs = JobManager.FinishedJobs;
            if (finishedJobs.Count == 0)
            {
                GUI.Label(new Rect(x, y, width, _rowHeight), "No jobs completed yet.");
                return;
            }

            var rowY = y;
            foreach (var job in finishedJobs)
            {
                GUI.Label(new Rect(x, rowY, width, 20f), $"{job.jobName}  —  Complete");
                rowY += _rowHeight;
            }
        }
    }
}
