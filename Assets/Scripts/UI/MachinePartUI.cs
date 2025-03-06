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
			this.text = machinePart.name;
		}

		private void OnClick()
		{
			machineBuilder.AttachPart(machinePart);
		}
	}
}