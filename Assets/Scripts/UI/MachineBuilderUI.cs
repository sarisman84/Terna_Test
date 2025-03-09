using System;
using System.IO;
using SFB;
using UnityEngine;
using UnityEngine.UIElements;

namespace Terna.UI
{
	public class MachineBuilderUI
	{
		private MachineBuilder builder;

		#region UI Elements
		private VisualElement partPickerElement;
		#region Buttons
		private Button clearButton;
		private Button deletePartButton;
		private Button saveButton;
		private Button loadButton;
		private Label fileNameLabel;
		private Label selectPartLabel;
		#endregion

		#region Prompts
		private Label selectPromptLabel;
		private Label panPromptLabel;
		private Label zoomPromptLabel;
		private Label deletePromptLabel;
		#endregion

		private VisualElement root;
		#endregion

		private const string extension = "assembly";
		public MachineBuilderUI(MachineBuilder builder, VisualElement root)
		{
			this.builder = builder;
			this.root = root;

			partPickerElement = this.root.Q<VisualElement>("part_picker");
			clearButton = this.root.Q<Button>("clear_button");
			deletePartButton = this.root.Q<Button>("delete_part");
			saveButton = this.root.Q<Button>("save_button");
			loadButton = this.root.Q<Button>("load_button");
			fileNameLabel = this.root.Q<Label>("file_name");
			selectPartLabel = this.root.Q<Label>("select_indicator");

			selectPromptLabel = this.root.Q<Label>("select_prompt");
			panPromptLabel = this.root.Q<Label>("pan_prompt");
			zoomPromptLabel = this.root.Q<Label>("zoom_prompt");
			deletePromptLabel = this.root.Q<Label>("delete_prompt");


			clearButton.clicked += () =>
			{
				builder.ClearAssembly();
				fileNameLabel.text = "Assembly: Untitled";
			};
			deletePartButton.clicked += () => builder.ClearAssemblyPart(builder.GetSelectedAssembledPartIndex());
			saveButton.clicked += OnSave;
			loadButton.clicked += OnLoad;
			builder.updateEvent += UpdateUI;

			saveButton.RegisterCallback<MouseEnterEvent>(DisableInput);
			saveButton.RegisterCallback<MouseLeaveEvent>(EnableInput);

			loadButton.RegisterCallback<MouseEnterEvent>(DisableInput);
			loadButton.RegisterCallback<MouseLeaveEvent>(EnableInput);

			clearButton.RegisterCallback<MouseEnterEvent>(DisableInput);
			clearButton.RegisterCallback<MouseLeaveEvent>(EnableInput);

			deletePartButton.RegisterCallback<MouseEnterEvent>(DisableInput);
			deletePartButton.RegisterCallback<MouseLeaveEvent>(EnableInput);

			saveButton.RegisterCallback<MouseEnterEvent>(DisableInput);
			saveButton.RegisterCallback<MouseLeaveEvent>(EnableInput);

			loadButton.RegisterCallback<MouseEnterEvent>(DisableInput);
			loadButton.RegisterCallback<MouseLeaveEvent>(EnableInput);


			fileNameLabel.text = "Assembly: Untitled";

			selectPromptLabel.text = $"Select: {builder.GetSelectKey()}";
			panPromptLabel.text = $"Pan: {builder.GetPanKey()}";
			zoomPromptLabel.text = $"Zoom: {builder.GetZoomKey()}";
			deletePromptLabel.text = $"Delete Selected: {builder.GetDeleteKey()}";
		}

		public void UpdatePartSelection(int[] partsToDisplay)
		{
			partPickerElement.Clear();
			for (int i = 0; i < partsToDisplay.Length; i++)
			{
				partPickerElement.Add(new MachinePartUI(partsToDisplay[i], builder));
			}
		}

		public void UpdateSelectedIndicator(string partName)
		{
			if (string.IsNullOrEmpty(partName))
			{
				selectPartLabel.text = "";
				return;
			}
			selectPartLabel.text = $"Selected Part: {partName}";
		}

		public void SetCustomMessageToSelectIndicator(string message)
		{
			selectPartLabel.text = message;
		}

		private void DisableInput(MouseEnterEvent evt)
		{
			builder.SetInputActive(false);
		}

		private void EnableInput(MouseLeaveEvent evt)
		{
			builder.SetInputActive(true);
		}

		private void UpdateUI()
		{
			deletePartButton.SetEnabled(builder.GetSelectedAssembledPartIndex() != -1);

		}


		private void OnSave()
		{
			SetCustomMessageToSelectIndicator("Saving Assembly...");
			string path = StandaloneFileBrowser.SaveFilePanel("Save Machine", "", "New Machine", extension);
			if (string.IsNullOrEmpty(path))
			{
				SetCustomMessageToSelectIndicator("Save Cancelled");
				return;
			}

			builder.GetMachineSerializer().SaveAssembly(path);
			string name = Path.GetFileName(path).Replace($".{extension}", "");
			fileNameLabel.text = $"Assembly: {name}";
			SetCustomMessageToSelectIndicator($"Saved Assembly: {name}");
		}

		private void OnLoad()
		{
			SetCustomMessageToSelectIndicator($"Loading...");
			string[] paths = StandaloneFileBrowser.OpenFilePanel("Load Machine", "", extension, false);
			if (paths.Length == 0)
			{
				SetCustomMessageToSelectIndicator("Load Cancelled");
				return;
			}

			builder.GetMachineSerializer().LoadAssembly(paths[0]);
			string name = Path.GetFileName(paths[0]).Replace($".{extension}", "");
			fileNameLabel.text = $"Assembly: {name}";
			SetCustomMessageToSelectIndicator($"Loaded Assembly: {name}");
		}
	}
}
