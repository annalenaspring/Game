using UnityEngine;
using UnityEngine.UI;

/// <summary>A circular live texture with a transparent feathered edge.</summary>
public class SoftCircleRawImage : RawImage
{
    protected override void OnPopulateMesh(VertexHelper mesh)
    {
        mesh.Clear();
        Rect rect = GetPixelAdjustedRect();
        const int segments = 96, rings = 8;
        mesh.AddVert(rect.center, color, new Vector2(0.5f, 0.5f));
        for (int ring = 0; ring < rings; ring++)
        {
            float t = (float)ring / (rings - 1);
            float radius = Mathf.Lerp(0.40f, 0.5f, t);
            Color tint = color; tint.a *= 1f - Mathf.SmoothStep(0f, 1f, t);
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2f / segments;
                Vector2 uv = new Vector2(0.5f + Mathf.Cos(angle) * radius, 0.5f + Mathf.Sin(angle) * radius);
                mesh.AddVert(new Vector3(rect.xMin + uv.x * rect.width, rect.yMin + uv.y * rect.height), tint, uv);
                int current = 1 + ring * segments + i;
                int next = 1 + ring * segments + (i + 1) % segments;
                if (ring == 0) mesh.AddTriangle(0, current, next);
                else
                {
                    mesh.AddTriangle(current - segments, current, next);
                    mesh.AddTriangle(current - segments, next, next - segments);
                }
            }
        }
    }
}
