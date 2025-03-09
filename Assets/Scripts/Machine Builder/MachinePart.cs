using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

namespace Terna
{
	[CreateAssetMenu(menuName = "Terna_Test/MachineBuilder/MachinePart")]
	public class MachinePart : ScriptableObject
	{
		public enum PartType
		{
			None = 0,
			Boom = 1 << 0,
			Arm = 1 << 1,
			Bucket = 1 << 2,
			Wheels = 1 << 3,
			Cabin = 1 << 4,
			Count = 5
		}
		[SerializeField] private GameObject partPrefab;
		[SerializeField] private PartType partType;
		[SerializeField] private Vector3 rotationOffset;

		public GameObject GetPartPrefab() => partPrefab;
		public PartType GetPartType() => partType;
		public bool HasAnchor() => GetAnchor(partPrefab.transform) != null;
		public Quaternion GetRotationOffset(List<AssembledMachinePart> assembledMachineParts, int index)
		{
			Vector3 targetRot = rotationOffset;
			if (partType == PartType.Boom)
			{
				// Find all the booms in the assembled machine parts
				List<AssembledMachinePart> booms = assembledMachineParts.FindAll(part => part.type == PartType.Boom);
				// If there is more than one boom and the current index is not the first found boom, flip the rotation offset
				if (IsInputedBoomFirst(booms, index))
				{
					targetRot.x *= -1;
				}
			}


			return Quaternion.Euler(targetRot);
		}

		public static Transform GetAnchor(Transform root)
		{
			for (int i = 0; i < root.childCount; i++)
			{
				if (root.GetChild(i).CompareTag("AnchorPoint"))
				{
					return root.GetChild(i);
				}
			}
			return null;

		}

		private bool IsInputedBoomFirst(List<AssembledMachinePart> booms, int index)
		{
			for (int i = 0; i < booms.Count; i++)
			{
				if (booms[i].index < index)
				{
					return true;
				}
			}
			return false;
		}

		public Texture2D GetPartPreviewTexture()
		{
#if UNITY_EDITOR
			return Resources.Load<Texture2D>($"Previews/{name}_preview") ?? UnityEditor.AssetPreview.GetAssetPreview(partPrefab);
#else
			return Resources.Load<Texture2D>($"Previews/{name}_preview");
#endif
		}


#if UNITY_EDITOR
		private void OnValidate()
		{
			if (partPrefab == null)
			{
				return;
			}

			TryGeneratePreview(this);
		}

		public static (int, string) TryGeneratePreview(MachinePart machinePart)
		{
			string[] result = UnityEditor.AssetDatabase.FindAssets($"t:Texture2D {machinePart.name}_preview", new string[] { "Assets/Resources/Previews" });
			if (result == null || result.Length == 0)
			{
				string path = $"Assets/Resources/Previews/{machinePart.name}_preview.png";
				Texture2D preview = null;

				while (preview == null)
				{
					preview = UnityEditor.AssetPreview.GetAssetPreview(machinePart.GetPartPrefab());
				}

				try
				{
					var bytes = preview.EncodeToPNG();
					System.IO.File.WriteAllBytes(path, bytes);
					UnityEditor.AssetDatabase.ImportAsset(path);
				}
				catch (Exception e)
				{
					return (-1, e.Message);
				}


				//OK
				return (0, "Preview generated successfully");
			}

			return (1, "Preview already exists");
		}

#endif

	}
}
