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
		private Button helpButton;
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
		private ModalWindow helpModal;

		#endregion

		public VisualElement GetRoot() => root;

		private const string extension = "assembly";
		public MachineBuilderUI(MachineBuilder builder, VisualElement root)
		{
			this.builder = builder;
			this.root = root;

			partPickerElement = this.root.Q<VisualElement>("part_picker");
			// Buttons
			clearButton = this.root.Q<Button>("clear_button");
			deletePartButton = this.root.Q<Button>("delete_part");
			saveButton = this.root.Q<Button>("save_button");
			loadButton = this.root.Q<Button>("load_button");
			helpButton = this.root.Q<Button>("info_button");
			// Labels
			fileNameLabel = this.root.Q<Label>("file_name");
			selectPartLabel = this.root.Q<Label>("select_indicator");
			// Prompts
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
			// Register button events
			saveButton.clicked += OnSave;
			loadButton.clicked += OnLoad;
			helpButton.clicked += OnHelpModal;

			// Register builder events
			builder.updateEvent += UpdateUI;

			// Register input toggling events
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

			// Set initial state
			fileNameLabel.text = "Assembly: Untitled";

			// Set initial prompts
			selectPromptLabel.text = $"Select: {builder.GetSelectKey()}";
			panPromptLabel.text = $"Pan: {builder.GetPanKey()}";
			zoomPromptLabel.text = $"Zoom: {builder.GetZoomKey()}";
			deletePromptLabel.text = $"Delete Selected: {builder.GetDeleteKey()}";
		}

		private void OnHelpModal()
		{
			if (helpModal != null)
			{
				helpModal.ClosePopup();
			}
			helpModal = root.CreatePopup(InitializeHelpModalContents(), () => helpModal = null);
			helpModal.style.opacity = 0.85f;
		}

		private VisualElement InitializeHelpModalContents()
		{
			VisualElement container = new VisualElement();

			container.style.width = Length.Percent(50);
			container.style.height = Length.Percent(50);

			container.style.alignItems = Align.Center;
			container.style.alignContent = Align.Center;
			container.style.alignSelf = Align.Center;

			Label description = new Label();
			description.text = "The machine builder tool allows you to create and modify a machine by selecting parts on the left side of the screen as well as selecting parts and their anchors within the world.\n\n" +
				"You can delete selected parts in the world by pressing the trash icon or the delete keybind, as well as clear the entire machine.\n\n" +
				"You can replace selected parts in the world by picking a part while having a selected part in the world.\n\n" +
				"Additionally, you have the option to save and load machines using the save and load buttons on the menu bar respectively.";
			description.style.textOverflow = TextOverflow.Clip;
			description.style.unityTextAlign = TextAnchor.MiddleCenter;
			description.style.whiteSpace = WhiteSpace.PreWrap;
			description.style.color = Color.white;
			description.style.backgroundColor = Color.clear;
			description.style.alignSelf = Align.Center;
			container.Add(description);

			Label exitInstruction = new Label();
			exitInstruction.text = "Click anywhere to close";
			exitInstruction.style.color = Color.grey;
			exitInstruction.style.unityTextAlign = TextAnchor.MiddleCenter;
			exitInstruction.style.backgroundColor = Color.clear;
			exitInstruction.style.alignSelf = Align.Center;
			exitInstruction.style.unityTextAlign = TextAnchor.MiddleCenter;

			container.Add(exitInstruction);

			return container;
		}

		public void UpdatePartSelection(int[] partsToDisplay)
		{
			partPickerElement.Clear();
			for (int i = 0; i < partsToDisplay.Length; i++)
			{
				partPickerElement.Add(new MachinePartUI(partsToDisplay[i], builder, this));
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
