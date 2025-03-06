using System;
using UnityEngine;
using UnityEngine.Pool;

namespace Terna
{
	[CreateAssetMenu(menuName = "Terna_Test/MachineBuilder/MachinePart")]
	public class MachinePart : ScriptableObject
	{
		[SerializeField] private GameObject partPrefab;

		public GameObject GetPartPrefab() => partPrefab;

	}
}
