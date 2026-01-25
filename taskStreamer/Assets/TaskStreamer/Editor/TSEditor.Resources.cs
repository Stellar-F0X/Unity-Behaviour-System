using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
	internal partial class TSEditor
	{
		[SerializeField]
		private VisualTreeAsset _basicPropertiesSectionPanel;

		[SerializeField]
		private VisualTreeAsset _fieldPropertiesSection;

		[SerializeField]
		private VisualTreeAsset _serviceContainerPanel;

		[SerializeField]
		private VisualTreeAsset _serviceSectionPanel;

		[SerializeField]
		private VisualTreeAsset _bbbConditionField;

		[SerializeField]
		private VisualTreeAsset _bbbConditionListField;

		[SerializeField]
		private VisualTreeAsset _bbVariableField;

		[SerializeField]
		private VisualTreeAsset _settingsPanel;

		[SerializeField]
		private VisualTreeAsset _floatingInspector;

		[SerializeField]
		private VisualTreeAsset _behaviorNode;

		[SerializeField]
		private VisualTreeAsset _stateNode;

		[SerializeField]
		private VisualTreeAsset _serviceBlock;

		[SerializeField]
		private VisualTreeAsset _window;
		
		[SerializeField]
		private VisualTreeAsset _nodeScriptCreationPopup;

		[SerializeField]
		private StyleSheet _windowStyle;

		[SerializeField]
		private StyleSheet _settingsStyle;

		[SerializeField]
		private StyleSheet _edgeStyle;

		[SerializeField]
		private StyleSheet _blackboardStyle;

		[SerializeField]
		private Texture2D _resizeHandleImage;

		[SerializeField]
		private Texture2D _deleteButtonImage;

		[SerializeField]
		private Texture2D _bindingButtonImage;

		[SerializeField]
		private Texture2D _addButtonImage;



		public static VisualTreeAsset basicPropertiesSectionPanel
		{
			get { return Instance._basicPropertiesSectionPanel; }
		}

		public static VisualTreeAsset fieldPropertiesSection
		{
			get { return Instance._fieldPropertiesSection; }
		}

		public static VisualTreeAsset serviceContainerPanel
		{
			get { return Instance._serviceContainerPanel; }
		}

		public static VisualTreeAsset serviceSectionPanel
		{
			get { return Instance._serviceSectionPanel; }
		}

		public static VisualTreeAsset bbbConditionField
		{
			get { return Instance._bbbConditionField; }
		}

		public static VisualTreeAsset bbbConditionListField
		{
			get { return Instance._bbbConditionListField; }
		}

		public static VisualTreeAsset bbVariableField
		{
			get { return Instance._bbVariableField; }
		}

		public static VisualTreeAsset settingsPanel
		{
			get { return Instance._settingsPanel; }
		}

		public static VisualTreeAsset floatingInspector
		{
			get { return Instance._floatingInspector; }
		}

		public static VisualTreeAsset behaviorNode
		{
			get { return Instance._behaviorNode; }
		}

		public static VisualTreeAsset stateNode
		{
			get { return Instance._stateNode; }
		}

		public static VisualTreeAsset serviceBlock
		{
			get { return Instance._serviceBlock; }
		}

		public static VisualTreeAsset window
		{
			get { return Instance._window; }
		}

		public static VisualTreeAsset nodeScriptCreationPopup
		{
			get { return Instance._nodeScriptCreationPopup; }
		}

		public static StyleSheet windowStyle
		{
			get { return Instance._windowStyle; }
		}

		public static StyleSheet settingsStyle
		{
			get { return Instance._settingsStyle; }
		}

		public static StyleSheet edgeStyle
		{
			get { return Instance._edgeStyle; }
		}

		public static StyleSheet blackboardStyle
		{
			get { return Instance._blackboardStyle; }
		}

		public static Texture2D resizeHandle
		{
			get { return Instance._resizeHandleImage; }
		}


		public static Texture2D deleteButton
		{
			get { return Instance._deleteButtonImage; }
		}


		public static Texture2D bindingButton
		{
			get { return Instance._bindingButtonImage; }
		}


		public static Texture2D addButton
		{
			get { return Instance._addButtonImage; }
		}
	}
}