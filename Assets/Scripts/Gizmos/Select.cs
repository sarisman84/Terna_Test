using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Terna.Gizmos
{
	public class Select : MonoBehaviour
	{
		private MeshRenderer[] childRenderers;
		private UniversalRenderPipelineAsset renderPipelineAsset;
		private void Awake()
		{
			childRenderers = GetComponentsInChildren<MeshRenderer>();
			renderPipelineAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;
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

		public void SetTransform(Vector3 center, Vector3 size)
		{
			transform.position = center;
			transform.localScale = size;
		}


		private void UpdateTransparencyColor(Color color, float alpha)
		{
			const int selectFeature = 1;

			UniversalRendererData data = renderPipelineAsset.rendererDataList[0] as UniversalRendererData;
			RenderObjects feature = data.rendererFeatures[selectFeature] as RenderObjects;
			feature.settings.overrideMaterial.color = new Color(color.r, color.g, color.b, alpha);
		}
	}
}
