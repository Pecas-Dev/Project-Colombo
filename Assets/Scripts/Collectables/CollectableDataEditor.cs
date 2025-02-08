using UnityEditor;
using UnityEngine;
using static ProjectColombo.Inventory.Collectable.CollectableData;

namespace ProjectColombo.Inventory.Collectable
{
    [CustomEditor(typeof(CollectableData))]
    public class CollectableDataEditor : Editor
    {
        // Declare serialized properties for all fields
        SerializedProperty type;
        SerializedProperty itemColor;
        SerializedProperty dropChance;
        SerializedProperty isPickable;

        // Serialized fields for currency, charm, and mask
        SerializedProperty minAmount;
        SerializedProperty maxAmount;
        SerializedProperty charmType;
        SerializedProperty valueModifierInPercent;
        SerializedProperty maskType;

        private void OnEnable()
        {
            // Get the serialized properties for each field
            type = serializedObject.FindProperty("type");
            itemColor = serializedObject.FindProperty("itemColor");
            dropChance = serializedObject.FindProperty("dropChance");
            isPickable = serializedObject.FindProperty("isPickable");

            // Currency fields
            minAmount = serializedObject.FindProperty("minAmount");
            maxAmount = serializedObject.FindProperty("maxAmount");

            // Charm fields
            charmType = serializedObject.FindProperty("charmType");
            valueModifierInPercent = serializedObject.FindProperty("valueModifierInPercent");

            // Mask fields
            maskType = serializedObject.FindProperty("maskType");
        }

        public override void OnInspectorGUI()
        {
            // Update the serialized object so it reflects any changes
            serializedObject.Update();

            // Draw the common fields
            EditorGUILayout.PropertyField(type);
            EditorGUILayout.PropertyField(itemColor);
            EditorGUILayout.PropertyField(dropChance);
            EditorGUILayout.PropertyField(isPickable);

            // Conditionally show fields based on CollectibleType
            switch ((CollectibleType)type.enumValueIndex)
            {
                case CollectibleType.Currency:
                    // Show Currency-related fields
                    EditorGUILayout.PropertyField(minAmount);
                    EditorGUILayout.PropertyField(maxAmount);
                    break;

                case CollectibleType.Charm:
                    // Show Charm-related fields
                    EditorGUILayout.PropertyField(charmType);
                    EditorGUILayout.PropertyField(valueModifierInPercent);
                    break;

                case CollectibleType.Mask:
                    // Show Mask-related fields
                    EditorGUILayout.PropertyField(maskType);
                    break;
            }

            // Apply any changes made in the inspector
            serializedObject.ApplyModifiedProperties();
        }
    }
}
