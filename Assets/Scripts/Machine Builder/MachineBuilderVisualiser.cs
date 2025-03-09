using System;
using Unity.VisualScripting;
using UnityEngine;

namespace Terna.Gizmos
{
	public class MachineBuilderVisualiser : MonoBehaviour
	{
		private MachineBuilder builder;
		public void Initialize(MachineBuilder builder)
		{
			this.builder = builder;


		}

		public void UpdateVisuals(int hoveredIndex, bool isAnchor)
		{
			ResetAllIndicators();
			UpdateHoverIndicator(hoveredIndex, isAnchor);
			UpdateSelectedIndicator(builder.GetSelectedAssembledPartIndex(), builder.HasSelectedAnchorPoint());
		}

		private void UpdateSelectedIndicator(int selectedIndex, bool isAnchor)
		{
			if (selectedIndex < 0 || selectedIndex >= builder.GetAssembledMachineParts().Count)
			{
				return;
			}
			AssembledMachinePart part = builder.GetAssembledMachineParts()[selectedIndex];
			AssembledMachinePart parentPart = builder.IsIndexValid(part.parentIndex) ? builder.GetAssembledMachineParts()[part.parentIndex] : null;
			AssembledMachinePart childPart = builder.IsIndexValid(part.childIndex) ? builder.GetAssembledMachineParts()[part.childIndex] : null;
			part.anchorIndicator?.gameObject.SetActive(true);

			if (isAnchor)
			{
				part.anchorIndicator.SetState(Selectable.State.Selected);
				childPart?.selectIndicator?.SetState(Selectable.State.Selected);
			}
			else
			{
				part.selectIndicator.SetState(Selectable.State.Selected);
				parentPart?.anchorIndicator?.gameObject.SetActive(true);
				parentPart?.anchorIndicator?.SetState(Selectable.State.Selected);
			}

		}

		private void UpdateHoverIndicator(int hoveredIndex, bool isAnchor)
		{
			if (hoveredIndex < 0 || hoveredIndex >= builder.GetAssembledMachineParts().Count)
			{
				return;
			}
			AssembledMachinePart part = builder.GetAssembledMachineParts()[hoveredIndex];
			AssembledMachinePart parentPart = builder.IsIndexValid(part.parentIndex) ? builder.GetAssembledMachineParts()[part.parentIndex] : null;
			AssembledMachinePart childPart = builder.IsIndexValid(part.childIndex) ? builder.GetAssembledMachineParts()[part.childIndex] : null;
			if (isAnchor)
			{
				part.anchorIndicator.SetState(Selectable.State.Hover);
				childPart?.selectIndicator?.SetState(Selectable.State.Hover);
			}
			else
			{
				part.selectIndicator.SetState(Selectable.State.Hover);
				parentPart?.anchorIndicator?.gameObject.SetActive(true);
				parentPart?.anchorIndicator?.SetState(Selectable.State.Hover);
			}
		}

		private void ResetAllIndicators()
		{
			System.Collections.Generic.List<AssembledMachinePart> list = builder.GetAssembledMachineParts();

			for (int i = 0; i < list.Count; i++)
			{
				AssembledMachinePart part = list[i];

				part.selectIndicator?.SetState(Selectable.State.Default);
				part.anchorIndicator?.SetState(Selectable.State.Default);
				part.anchorIndicator?.gameObject.SetActive(false);
			}
		}

	}
}
