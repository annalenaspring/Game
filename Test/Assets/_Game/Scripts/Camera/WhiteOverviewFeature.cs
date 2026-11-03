using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>Restore display white only at background pixels of the overview camera.</summary>
public class WhiteOverviewFeature : FullScreenPassRendererFeature
{
    private Material ownedMaterial;

    public override void Create()
    {
        if (ownedMaterial == null)
        {
            Shader shader = Resources.Load<Shader>("WhiteOverview");
            if (shader != null) ownedMaterial = CoreUtils.CreateEngineMaterial(shader);
        }
        passMaterial = ownedMaterial;
        injectionPoint = InjectionPoint.AfterRenderingPostProcessing;
        requirements = ScriptableRenderPassInput.Depth;
        fetchColorBuffer = false;
        base.Create();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        Camera camera = renderingData.cameraData.camera;
        if (camera.cameraType != CameraType.Game) return;
        var orbit = camera.GetComponent<OrbitCamera>();
        if (orbit == null || !orbit.isActiveAndEnabled ||
            orbit.backgroundStyle != OrbitCamera.OverviewBackground.White) return;
        base.AddRenderPasses(renderer, ref renderingData);
    }

    protected override void Dispose(bool disposing)
    {
        CoreUtils.Destroy(ownedMaterial);
        ownedMaterial = null;
        base.Dispose(disposing);
    }
}
