using UnityEngine;

public static class PortraitGameLayout
{
    public static Rect PovRect
    {
        get
        {
            Rect safe = Screen.safeArea;
            float diameter = Mathf.Min(safe.width * 0.9f, safe.height * 0.44f);
            return new Rect(safe.center.x - diameter / 2f,
                safe.yMin + safe.height * 0.035f, diameter, diameter);
        }
    }

    public static Vector2 OverviewCenter => new Vector2(
        Screen.safeArea.center.x / Mathf.Max(1, Screen.width),
        (PovRect.yMax + Screen.safeArea.yMax) * 0.5f / Mathf.Max(1, Screen.height));

    public static bool ContainsPov(Vector2 point)
    {
        Rect rect = PovRect;
        return (point - rect.center).sqrMagnitude <= rect.width * rect.width * 0.25f;
    }
}
