using System;
using Terna.Attributes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Terna.Gizmos
{
	public class Selectable : MonoBehaviour
	{
		[Header("Selection Colors")]
		[SerializeField] private Material selectedMaterial;
		[SerializeField] private Material hoverMaterial;
		[SerializeField] private Material defaultMaterial;

		[Header("Render Layers")]
		[SerializeField][Layer] private int defaultLayer;
		[SerializeField][Layer] private int hoverLayer;
		[SerializeField][Layer] private int selectLayer;


		public enum State
		{
			Default,
			Hover,
			Selected
		}


		private MeshRenderer[] childRenderers;
		private UniversalRenderPipelineAsset renderPipelineAsset;
		private Color originalColor;



		[Header("Debug State")]
		[SerializeField] private State currentState = State.Default;
		[SerializeField] private State oldState = State.Default;

		public State GetCurrentState() => currentState;
		public State GetOldState() => oldState;
		private void Awake()
		{
			childRenderers = GetComponentsInChildren<MeshRenderer>();
			renderPipelineAsset = GraphicsSettings.defaultRenderPipeline as UniversalRenderPipelineAsset;

			originalColor = childRenderers[0].material.color;
		}

		public void SetState(State state)
		{
			Material targetMaterial;
			switch (state)
			{
				case State.Hover:
					targetMaterial = hoverMaterial;
					break;
				case State.Selected:
					targetMaterial = selectedMaterial;
					break;
				default:
					targetMaterial = defaultMaterial;
					break;
			}

			foreach (MeshRenderer renderer in childRenderers)
			{
				renderer.material = targetMaterial;
			}
			UpdateRenderLayer(state);

			oldState = currentState;
			currentState = state;

			//Debug.Log($"{transform.parent.gameObject.name}/{gameObject.name}: Set state to {state}");
		}

		public void SetTransform(Vector3 center, Vector3 size)
		{
			transform.position = center;
			transform.localScale = size;
		}


		private void UpdateRenderLayer(State state)
		{
			int targetMask;
			switch (state)
			{
				case State.Hover:
					targetMask = hoverLayer;
					break;
				case State.Selected:
					targetMask = selectLayer;
					break;
				default:
					targetMask = defaultLayer;
					break;
			}

			gameObject.layer = targetMask;
			foreach (MeshRenderer renderer in childRenderers)
			{
				renderer.gameObject.layer = targetMask;
			}
		}
	}
}
