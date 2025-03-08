using System;
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
		private Button clearButton;
		private Button deletePartButton;
		private Button saveButton;
		private Button loadButton;
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

			clearButton.clicked += () => builder.ClearAssembly();
			deletePartButton.clicked += () => builder.ClearAssemblyPart(builder.GetSelectedAssembledPartIndex());
			saveButton.clicked += OnSave;
			loadButton.clicked += OnLoad;
			builder.updateEvent += UpdateUI;
			
			clearButton.RegisterCallback<MouseEnterEvent>(DisableInput);
			clearButton.RegisterCallback<MouseLeaveEvent>(EnableInput);

			deletePartButton.RegisterCallback<MouseEnterEvent>(DisableInput);
			deletePartButton.RegisterCallback<MouseLeaveEvent>(EnableInput);

			saveButton.RegisterCallback<MouseEnterEvent>(DisableInput);
			saveButton.RegisterCallback<MouseLeaveEvent>(EnableInput);

			loadButton.RegisterCallback<MouseEnterEvent>(DisableInput);
			loadButton.RegisterCallback<MouseLeaveEvent>(EnableInput);


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
		public void UpdatePartSelection(int[] partsToDisplay)
		{
			partPickerElement.Clear();
			for (int i = 0; i < partsToDisplay.Length; i++)
			{
				partPickerElement.Add(new MachinePartUI(partsToDisplay[i], builder));
			}
		}

		private void OnSave()
		{
			StandaloneFileBrowser.SaveFilePanelAsync("Save Machine", "", "New Machine", extension, (string path) =>
			{
				if (string.IsNullOrEmpty(path))
				{
					return;
				}

				builder.GetMachineSerializer().SaveAssembly(path);
			});
		}

		private void OnLoad()
		{
			StandaloneFileBrowser.OpenFilePanelAsync("Load Machine", "", extension, false, (string[] paths) =>
			{
				if (paths.Length == 0)
				{
					return;
				}

				builder.GetMachineSerializer().LoadAssembly(paths[0]);
			});
		}
	}
}
