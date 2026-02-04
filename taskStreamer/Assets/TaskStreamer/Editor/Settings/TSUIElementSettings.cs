using TaskStreamer.Runtime;
using UnityEngine;
using UnityEngine.UIElements;

namespace TaskStreamer.Tool
{
#if USE_ASSETS_PATH
	[CreateAssetMenu(fileName = "New UIElementSettings", menuName = "Task Streamer/UIElementSettings")]
#endif
	public class TSUIElementSettings : ScriptableObject
	{
		private static TSUIElementSettings _instance;
		
		public static TSUIElementSettings instance
		{
			get { return _instance = _instance == null ? TSEditorUtility.FindAssetByName<TSUIElementSettings>("t:TSUIElementSettings") : _instance; }
		}
		
		
		[Underline("Tree Assets")]
		public VisualTreeAsset TaskHeaderSection;
		public VisualTreeAsset BBVariableFieldsPanel;
		public VisualTreeAsset ServiceSectionsPanel;
		public VisualTreeAsset ServiceSection;
		public VisualTreeAsset ConditionSection;
		public VisualTreeAsset ConditionSectionsPanel;
		public VisualTreeAsset BBVariableField;
		public VisualTreeAsset Settings;
		public VisualTreeAsset Inspector;
		public VisualTreeAsset BTNode;
		public VisualTreeAsset FSMNode;
		public VisualTreeAsset ServiceBlock;
		public VisualTreeAsset Window;
		public VisualTreeAsset ScriptCreationPopup;

		[Underline("Styles")]
		public StyleSheet WindowStyle;
		public StyleSheet SettingsStyle;
		public StyleSheet EdgeStyle;
		public StyleSheet BlackboardStyle;

		[Underline("Images")]
		public Texture2D ResizeHandleImage;
		public Texture2D DeleteButtonImage;
		public Texture2D BindingButtonImage;
		public Texture2D AddButtonImage;
	}
}