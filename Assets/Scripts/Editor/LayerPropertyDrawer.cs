using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Terna.Attributes;

namespace Terna.Editor.Drawer
{
	[CustomPropertyDrawer(typeof(LayerAttribute))]
	public class LayerPropertyDrawer : PropertyDrawer
	{
		public override VisualElement CreatePropertyGUI(SerializedProperty property)
		{
			VisualElement root = new VisualElement();


			if (property.propertyType == SerializedPropertyType.Integer)
			{
				LayerField layerField = new LayerField(property.displayName, property.intValue);
				layerField.RegisterValueChangedCallback((evt) =>
				{
					property.intValue = (int)evt.newValue;
					property.serializedObject.ApplyModifiedProperties();
				});
				root.Add(layerField);
			}
			else
			{
				PropertyField field = new PropertyField(property);
				field.Bind(property.serializedObject);
				root.Add(field);
				
				HelpBox label = new HelpBox($"{nameof(LayerAttribute)} shouldn't be applied to {property.propertyType}, it's only valid on integers.", HelpBoxMessageType.Error);
				root.Add(label);
			}

			return root;
		}
	}
}
