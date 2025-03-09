using UnityEngine;
using UnityEngine.UIElements;

namespace Terna.UI
{
	public class MachinePartUI : Button
	{
		private int machinePart;
		private MachineBuilder machineBuilder;
		private Color originalColor;

		public MachinePartUI(int machinePart, MachineBuilder machineBuilder, MachineBuilderUI machineBuilderUI)
		{
			this.machinePart = machinePart;
			this.machineBuilder = machineBuilder;

			MachinePart part = machineBuilder.GetMachinePart(machinePart);

			this.clicked += OnClick;
			this.text = "";
			this.style.backgroundImage = Background.FromTexture2D(part.GetPartPreviewTexture());
			this.tooltip = part.GetPartPrefab().name;
			this.style.width = 75;
			this.style.height = 75;


			this.style.marginBottom = 2;
			this.style.marginLeft = 2;
			this.style.marginRight = 2;
			this.style.marginTop = 2;

			originalColor = this.style.backgroundColor.value;

			this.RegisterCallback<MouseEnterEvent>(evt =>
			{
				this.style.backgroundColor = Color.white;
				machineBuilder.SetInputActive(false);
			});

			this.RegisterCallback<MouseLeaveEvent>(evt =>
			{
				this.style.backgroundColor = originalColor;
				machineBuilder.SetInputActive(true);
			});

		}

		private void OnClick()
		{
			var index = machineBuilder.GetSelectedAssembledPartIndex();
			var hasSelectedAnchorPoint = machineBuilder.HasSelectedAnchorPoint();
			if (hasSelectedAnchorPoint)
			{
				machineBuilder.CreateNewAssemblyPart(machinePart, index);
			}
			else
			{
				machineBuilder.UpdateAssemblyPart(machinePart, index);
			}
		}
	}
}