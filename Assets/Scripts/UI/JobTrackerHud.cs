using CIS2991Project.Jobs;
using UnityEngine;

namespace CIS2991Project.UI
{
    // Always-visible kill-count readout for active jobs, top-right below the money HUD.
    public sealed class JobTrackerHud
    {
        private readonly Texture2D _rowBackgroundTexture;
        private readonly float _width;
        private readonly float _rowHeight;
        private readonly float _rowGap;
        private readonly float _moneyHeight;

        public JobTrackerHud(Texture2D rowBackgroundTexture, float width, float rowHeight, float rowGap, float moneyHeight)
        {
            _rowBackgroundTexture = rowBackgroundTexture;
            _width = width;
            _rowHeight = rowHeight;
            _rowGap = rowGap;
            _moneyHeight = moneyHeight;
        }

        public void Draw()
        {
            var activeJobs = JobManager.ActiveJobs;
            if (activeJobs.Count == 0)
            {
                return;
            }

            var x = GuiScale.ReferenceWidth - _width - 16f;
            var y = 16f + _moneyHeight + 8f;

            foreach (var job in activeJobs)
            {
                var rect = new Rect(x, y, _width, _rowHeight);
                GuiDrawUtils.DrawSlot(rect, _rowBackgroundTexture);
                GUI.Label(rect, $"{job.killTargetTag}s: {JobManager.GetProgress(job)}/{job.killTargetCount}", GuiDrawUtils.CenteredLabelStyle);
                y += _rowHeight + _rowGap;
            }
        }
    }
}
