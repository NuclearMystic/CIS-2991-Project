using CIS2991Project.Player;
using UnityEngine;

namespace CIS2991Project.UI
{
    // Allocate skill points earned from leveling up.
    public sealed class SkillsPanel
    {
        private readonly Texture2D _backgroundTexture;
        private readonly float _panelWidth;
        private readonly float _rowHeight;

        public SkillsPanel(Texture2D backgroundTexture, float panelWidth, float rowHeight)
        {
            _backgroundTexture = backgroundTexture;
            _panelWidth = panelWidth;
            _rowHeight = rowHeight;
        }

        public void Draw(CharacterSheet characterSheet, float startY)
        {
            if (characterSheet == null)
            {
                return;
            }

            var skills = (SkillType[])System.Enum.GetValues(typeof(SkillType));
            var startX = GuiScale.ReferenceWidth - _panelWidth - 16f;
            var height = 28f + skills.Length * _rowHeight + 8f;

            GuiDrawUtils.DrawSlot(new Rect(startX, startY, _panelWidth, height), _backgroundTexture);

            GUI.Label(new Rect(startX + 8f, startY + 4f, _panelWidth - 16f, 20f),
                $"Level {characterSheet.Level}   XP {characterSheet.Experience}/{characterSheet.ExperienceToNextLevel}   Points: {characterSheet.UnspentSkillPoints}");

            var rowY = startY + 28f;
            foreach (var skill in skills)
            {
                var skillLevel = characterSheet.GetLevel(skill);
                GUI.Label(new Rect(startX + 8f, rowY, _panelWidth - 56f, _rowHeight), $"{skill}: {skillLevel}/{CharacterSheet.MaxSkillLevel}");

                var canAllocate = characterSheet.UnspentSkillPoints > 0 && skillLevel < CharacterSheet.MaxSkillLevel;
                GUI.enabled = canAllocate;
                if (GUI.Button(new Rect(startX + _panelWidth - 40f, rowY, 32f, _rowHeight - 4f), "+"))
                {
                    characterSheet.TryAllocateSkillPoint(skill);
                }
                GUI.enabled = true;

                rowY += _rowHeight;
            }
        }
    }
}
