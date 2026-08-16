using UnityEngine;

namespace CIS2991Project.UI
{
    // Legacy OnGUI draws in raw screen pixels, which don't scale between the Editor's Game view and a
    // build's native resolution - fixed-size elements end up tiny on a high-resolution build. Call
    // Begin() as the first line of OnGUI to uniformly scale everything drawn afterward to fit
    // ReferenceWidth/ReferenceHeight, and use those constants (not Screen.width/Screen.height) for any
    // positioning math after that point - the live screen size is exactly what Begin() is compensating
    // for, so using it again after scaling would double-apply the correction.
    public static class GuiScale
    {
        public const float ReferenceWidth = 1920f;
        public const float ReferenceHeight = 1080f;

        public static void Begin()
        {
            // GUI.matrix isn't reset automatically between different components' OnGUI calls, so start
            // from a known-clean slate rather than compounding onto whatever the last OnGUI left behind.
            GUI.matrix = Matrix4x4.identity;
            var scale = Mathf.Min(Screen.width / ReferenceWidth, Screen.height / ReferenceHeight);
            GUIUtility.ScaleAroundPivot(new Vector2(scale, scale), Vector2.zero);
        }
    }
}
