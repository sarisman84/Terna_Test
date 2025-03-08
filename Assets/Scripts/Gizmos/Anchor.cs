using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Terna.Gizmos
{
    public class Anchor : MonoBehaviour
    {
        MeshRenderer[] childRenderers;
        UniversalRenderPipelineAsset renderPipelineAsset;
        private void Awake()
        {
            childRenderers = GetComponentsInChildren<MeshRenderer>();
            renderPipelineAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
        }

        public void SetVisibility(bool visible)
        {
            foreach (var renderer in childRenderers)
            {
                renderer.enabled = visible;
            }
        }

        public void SetColor(Color color)
        {
            foreach (var renderer in childRenderers)
            {
                Color renderColor = renderer.material.color;
                renderer.sharedMaterial.color = new Color(color.r, color.g, color.b, renderColor.a);
                UpdateTransparencyColor(color, renderColor.a);
            }
        }

        private void UpdateTransparencyColor(Color color, float alpha)
        {
            const int anchorFeature = 0;

            UniversalRendererData data = renderPipelineAsset.rendererDataList[0] as UniversalRendererData;
            RenderObjects feature = data.rendererFeatures[anchorFeature] as RenderObjects;
            feature.settings.overrideMaterial.color = new Color(color.r, color.g, color.b, alpha);
        }
    }
}

