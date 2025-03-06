using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Terna.UI
{
	public class MachineBuilderUI
	{
		private MachineBuilder builder;
		private VisualElement partPickerElement;
		private Button clearButton;
		private VisualElement root;
		private Label selectedPartLabel;
		public MachineBuilderUI(MachineBuilder builder, VisualElement root, Action clearDelegate)
		{
			this.builder = builder;
			this.root = root;
			
			partPickerElement = this.root.Q<VisualElement>("part_picker");
			clearButton = this.root.Q<Button>("clear_button");
			selectedPartLabel = this.root.Q<Label>("selected_machine_part");
			clearButton.clicked += clearDelegate;

			ResetSelectPartLabel();
		}
		

		public void UpdatePartSelection(MachinePart[] partsToDisplay){
			partPickerElement.Clear();
			foreach (var part in partsToDisplay)
			{
				partPickerElement.Add(new MachinePartUI(part, builder));
			}
		}

		public void ResetSelectPartLabel(){
			selectedPartLabel.text = "Selected Part: None";
		}
		public void UpdateSelectedPartLabel(AssembledMachinePart selectedPart){
			selectedPartLabel.text = $"Selected Part: {selectedPart.part.name}";
		}
	}
}
