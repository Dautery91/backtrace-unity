using System;
using System.IO;
using Backtrace.Unity;
using Backtrace.Unity.Model;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Backtrace.Unity.Editor
{
    public class BacktraceIntegrationHelperWindow : EditorWindow
    {
        // Constant Labels and URLs
        private const string LogoPath = "Packages/io.backtrace.unity/Editor/Logos/Logo-Backtrace-TrueBlack-Horiz.png";

        // Notifications / error messages
        private string windowLogMessage;
        private MessageType logMessageType;
        
        private Vector2 scrollPosition;
        private Texture logo;
        
        private UnityEditor.Editor backtraceConfigurationEditor;

        // User input variables
        [SerializeField] string pathToCreateScriptable = "Assets/Backtrace/";
        [SerializeField] string configFileName = "BacktraceConfiguration";
        [SerializeField] private BacktraceConfiguration backtraceConfiguration;

        private GameObject clientGameObject;
        
        /// <summary>
        /// Creates the Editor window.  Accessible via Unity's top toolbar under Window>>Backtrace.
        /// </summary>
        /// <returns></returns>
        [MenuItem("Window/Backtrace/IntegrationHelper")]
        public static BacktraceIntegrationHelperWindow ShowWindow()
        {
            BacktraceIntegrationHelperWindow window =
                (BacktraceIntegrationHelperWindow) EditorWindow.GetWindow(typeof(BacktraceIntegrationHelperWindow), 
                    false, BacktraceIntegrationWindowLabels.LABEL_INTEGRATION_WINDOW_TITLE);

            window.minSize = new Vector2(400, 500);
            window.Show();
            return window;
        }

        #region Unity Callbacks

        private void OnEnable()
        {
            // Basic variable initialization
            logo = AssetDatabase.LoadAssetAtPath<Texture>(LogoPath);
            logMessageType = MessageType.Info;
            windowLogMessage = BacktraceIntegrationWindowLabels.LABEL_INTEGRATION_WINDOW_WELCOME_MESSAGE;
            scrollPosition = new Vector2(0, 0);
        }

        
        private void OnGUI()
        {
            // Header
            BTEditorUtility.DrawHeader(logo, Color.white, position);
            BTEditorUtility.DrawHorizontalUILine(Color.black,2,1);
            
            // Documentation Links
            EditorGUILayout.LabelField(BacktraceIntegrationWindowLabels.LABEL_INTEGRATION_RESOURCELINKSSECTION_HEADER, 
                BTEditorUtility.HeaderTextStyle);
            DrawDocumentationButtons();
            BTEditorUtility.DrawHorizontalUILine(Color.black,2,1);
            GUILayout.Space(10);
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            
            // Config asset creation + manipulation
            DrawBacktraceConfigSections();
            BTEditorUtility.DrawHorizontalUILine(Color.black,2,1);
            GUILayout.Space(10);
            
            // Client creation
            DrawBacktraceClientSection();
            
            EditorGUILayout.EndScrollView();

            // Notification box
            DrawNotificationMessage();
        }

        #endregion

        #region Utility Methods

        void DrawBacktraceClientSection()
        {
            EditorGUILayout.LabelField(BacktraceIntegrationWindowLabels.LABEL_INTEGRATION_CLIENTSECTION_HEADER, BTEditorUtility.HeaderTextStyle);
            BTEditorUtility.DrawHorizontalUILine(Color.grey,2,1);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(BacktraceIntegrationWindowLabels.LABEL_INTEGRATION_CREATECLIENT_BUTTON, BTEditorUtility.BaseFieldSizingLayoutOptions))
            {
                if (backtraceConfiguration == null)
                {
                    logMessageType = MessageType.Warning;
                    windowLogMessage = "Failed to create new Backtrace Client because you do not have a configuration selected. " +
                                       "Please create or select a Backtrace Configuration asset and try again.";
                }
                else
                {
                    if (clientGameObject == null)
                    {
                        clientGameObject = new GameObject("BacktraceClient", typeof(BacktraceClient));
                        clientGameObject.GetComponent<BacktraceClient>().Configuration = backtraceConfiguration;
                        logMessageType = MessageType.Info;
                        windowLogMessage = "Backtrace Client created in Scene: " + SceneManager.GetActiveScene().name;
                    }
                    else
                    {
                        clientGameObject.GetComponent<BacktraceClient>().Configuration = backtraceConfiguration;
                        logMessageType = MessageType.Warning;
                        windowLogMessage = "Backtrace Client already exists in Scene: " + SceneManager.GetActiveScene().name + 
                                           "\nThe existing Client was updated to reflect the currently selected Configuration";
                    }
                    
                    EditorUtility.FocusProjectWindow();
                    Selection.activeObject = clientGameObject;
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        void DrawBacktraceConfigSections()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(BacktraceIntegrationWindowLabels.LABEL_INTEGRATION_CONFIGSECTION_HEADER, BTEditorUtility.HeaderTextStyle);
            GUILayout.FlexibleSpace();

            backtraceConfiguration = (BacktraceConfiguration)EditorGUILayout
                .ObjectField(backtraceConfiguration, typeof(BacktraceConfiguration), new GUILayoutOption[]
                {
                    GUILayout.MinWidth(position.width / 3)
                });

            EditorGUILayout.EndHorizontal();
            BTEditorUtility.DrawHorizontalUILine(Color.grey,2,1);
            if (backtraceConfiguration != null)
            {
                BTEditorUtility.DrawSubHeading("Settings for: " + backtraceConfiguration.name);

                if (backtraceConfigurationEditor == null)
                    backtraceConfigurationEditor = UnityEditor.Editor.CreateEditor(backtraceConfiguration);
                backtraceConfigurationEditor.OnInspectorGUI();
                backtraceConfigurationEditor.Repaint();
            }
            else
            {
                DrawConfigCreatorSection();
            }
        }

        void DrawConfigCreatorSection()
        {
            BTEditorUtility.DrawSubHeading(BacktraceIntegrationWindowLabels.LABEL_INTEGRATION_CONFIGCREATIONSECTION_SUBHEADER);
            
            // Buttons
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(BacktraceIntegrationWindowLabels.LABEL_INTEGRATION_CONFIGCREATION_BUTTON, 
                BTEditorUtility.DefineLayoutSizingConstraints(10,30,60,position.width / 2)))
            {
                string fullPath = pathToCreateScriptable + configFileName + ".asset";
                if (File.Exists(fullPath))
                {
                    windowLogMessage = fullPath + " already exists! No configuration asset was created.";
                    logMessageType = MessageType.Warning;
                    return;
                }

                if (!Directory.Exists(pathToCreateScriptable))
                {
                    Directory.CreateDirectory(pathToCreateScriptable);
                    windowLogMessage = pathToCreateScriptable + " directory created.\n" + fullPath + " created successfully!";
                }
                else
                {
                    windowLogMessage = fullPath + " created successfully!";
                }
                logMessageType = MessageType.Info;
                
                BacktraceConfiguration configAsset = ScriptableObject.CreateInstance<BacktraceConfiguration>();
                AssetDatabase.CreateAsset(configAsset, fullPath);
                AssetDatabase.SaveAssets();
                
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = configAsset;
                backtraceConfiguration = configAsset;
                backtraceConfigurationEditor = UnityEditor.Editor.CreateEditor(backtraceConfiguration);
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            
            
            // Text fields
            GUILayout.BeginHorizontal();
            
            GUILayout.BeginVertical();
            EditorStyles.textField.wordWrap = true;
            GUILayout.Label(BacktraceIntegrationWindowLabels.LABEL_INTEGRATION_CONFIGNAME);
            configFileName = GUILayout.TextField(configFileName, 
                BTEditorUtility.DefineLayoutSizingConstraints(10,20,60,600));
            GUILayout.EndVertical();
            
            GUILayout.BeginVertical();
            GUILayout.Label(BacktraceIntegrationWindowLabels.LABEL_INTEGRATION_ASSETPATH);
            
            pathToCreateScriptable = GUILayout.TextField(pathToCreateScriptable,
                BTEditorUtility.DefineLayoutSizingConstraints(10, 20, 60, 600));
            EditorStyles.textField.wordWrap = false;
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        void DrawNotificationMessage()
        {
            GUILayout.FlexibleSpace();
            EditorGUILayout.HelpBox(windowLogMessage, logMessageType);
        }

        void DrawDocumentationButtons()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button(BacktraceIntegrationWindowLabels.LABEL_INTEGRATION_DOCUMENTATION_BUTTON, 
                BTEditorUtility.DefineLayoutSizingConstraints(10,20,60,600))) 
            { OnDocumentationButtonPressed(BacktraceIntegrationWindowLabels.LABEL_INTEGRATION_DOCUMENTATION_URL); }

            if (GUILayout.Button(BacktraceIntegrationWindowLabels.LABEL_INTEGRATION_VIDEO_BUTTON, 
                BTEditorUtility.DefineLayoutSizingConstraints(10,20,60,600))) 
            { OnDocumentationButtonPressed(BacktraceIntegrationWindowLabels.LABEL_INTEGRATION_VIDEO_URL);}

            if (GUILayout.Button(BacktraceIntegrationWindowLabels.LABEL_INTEGRATION_SUPPORT_BUTTON, 
                BTEditorUtility.DefineLayoutSizingConstraints(10,20,60,600)))
            { OnDocumentationButtonPressed(BacktraceIntegrationWindowLabels.LABEL_INTEGRATION_SUPPORT_URL);}
 
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        void OnDocumentationButtonPressed(string url)
        {
            Application.OpenURL(url);
            windowLogMessage = BacktraceIntegrationWindowLabels.LABEL_INTEGRATION_WINDOW_WELCOME_MESSAGE;
            logMessageType = MessageType.Info;
        }

        #endregion
        

    }
}