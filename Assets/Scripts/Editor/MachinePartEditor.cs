using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Terna;
using UnityEditor.VersionControl;

[CustomEditor(typeof(MachinePart))]
public class MachinePartEditor : Editor
{
	public override VisualElement CreateInspectorGUI()
	{
		MachinePart part = (MachinePart)target;

		VisualElement root = new VisualElement();
		root.style.flexDirection = FlexDirection.Row;
		root.style.width = Length.Percent(100);
		root.style.height = Length.Percent(100);

		VisualElement previewElement = new VisualElement();
		previewElement.style.backgroundImage = Background.FromTexture2D(part.GetPartPreviewTexture());
		previewElement.style.width = 100;
		previewElement.style.height = 100;
		root.Add(previewElement);

		VisualElement properties = new VisualElement();
		properties.style.width = Length.Percent(100);
		properties.style.height = Length.Percent(100);

		SerializedProperty iterator = serializedObject.GetIterator();
		iterator.NextVisible(true);
		while (iterator.NextVisible(false))
		{
			PropertyField propertyField = new PropertyField(iterator);
			properties.Add(propertyField);
		}

		HelpBox statusLabel = new HelpBox();
		statusLabel.style.width = Length.Percent(100);
		statusLabel.style.height = 40;

		if (part.GetPartPreviewTexture())
		{
			statusLabel.messageType = HelpBoxMessageType.Info;
			statusLabel.text = "Status: Preview found";
		}
		else
		{
			statusLabel.messageType = HelpBoxMessageType.Error;
			statusLabel.text = "Status: No preview generated yet";
		}


		Button updatePreviewButton = new Button();
		updatePreviewButton.text = "Update Preview";
		updatePreviewButton.clicked += () =>
		{
			MachinePart machinePart = (MachinePart)target;
			var (status, Message) = MachinePart.TryGeneratePreview(machinePart);

			switch (status)
			{
				case 0:
					statusLabel.messageType = HelpBoxMessageType.Info;
					break;
				case 1:
					statusLabel.messageType = HelpBoxMessageType.Warning;
					break;
				case 2:
					statusLabel.messageType = HelpBoxMessageType.Error;
					break;
			}

			statusLabel.text = $"Status: {Message}";
		};

		properties.Add(updatePreviewButton);
		properties.Add(statusLabel);

		root.Add(properties);

		return root;
	}
}