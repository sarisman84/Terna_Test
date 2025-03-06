using UnityEngine;
using UnityEngine.UIElements;

namespace Terna.UI
{
	public class MachinePartUI : Button
	{
		private MachinePart machinePart;
		private MachineBuilder machineBuilder;

		public MachinePartUI(MachinePart machinePart, MachineBuilder machineBuilder)
		{
			this.machinePart = machinePart;
			this.machineBuilder = machineBuilder;

			this.clicked += OnClick;
			this.text = "";
			this.style.backgroundImage = Background.FromTexture2D(machinePart.GetPartPreviewTexture());
			this.tooltip = machinePart.GetPartPrefab().name;
			this.style.width = 100;
			this.style.height = 100;
		}

		private void OnClick()
		{
			machineBuilder.AttachPart(machinePart, machineBuilder.GetSelectedAssembledPartIndex());
		}
	}
}