using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace com.pellegrinoprincipe
{
    public class SceneSelector : AssetPostprocessor
    {
        // key =   scene name
        // value = scene path; i.e: Assets/MyScenes/MyScene.unity
        static Dictionary<string, string> scenes = new Dictionary<string, string>();

        static string menuFileName = "SceneMenuItems.cs";
        static StringBuilder sbFileCreator = new StringBuilder();
        static string keyPrefSceneCount = "com.pellegrinoprincipe." +
                                           PlayerSettings.productName +
                                          "SceneSelector.sceneCount";
        static string keyPrefImpOrMoved = "com.pellegrinoprincipe." +
                                           PlayerSettings.productName +
                                           "SceneSelector.impOrMoved";

        [InitializeOnLoadMethod]
        static void InitSceneSelector() { SearchScenes(); }

        static void SearchScenes()
        {
            // I can't use a static member variable, i.e.: static int sceneCount, because
            // [InitializeOnLoadMethod] but also [InitializeOnLoad] "re-initialize" the
            // class putting always the default value into member variables...
            int sceneCount = EditorPrefs.GetInt(keyPrefSceneCount);
            bool impOrMoved = EditorPrefs.GetBool(keyPrefImpOrMoved);

            string[] scenesGuids = AssetDatabase.FindAssets("t:scene");
            int currentSceneCount = scenesGuids.Length;

            if (sceneCount != currentSceneCount || impOrMoved)
            {
                foreach (string guid in scenesGuids)
                {
                    string scenetPath = AssetDatabase.GUIDToAssetPath(guid);

                    string sceneName = Path.GetFileName(scenetPath).Split('.')[0]; // remove .unity

                    if (!scenes.ContainsKey(sceneName))
                        scenes[sceneName] = scenetPath;
                }

                EditorPrefs.SetInt(keyPrefSceneCount, currentSceneCount);
                EditorPrefs.SetBool(keyPrefImpOrMoved, false);

                CreateFileThatGeneratesMenu();
            }
        }

        static void CreateFileThatGeneratesMenu()
        {
            string _HEADER_ = @"// This class is auto-generated; please don't modify!
using UnityEditor;
using UnityEditor.SceneManagement;

namespace com.pellegrinoprincipe
{
    public class SceneMenuItems
    {";
            string _FOOTER_ = @"    }
}";
            sbFileCreator.Length = 0;
            sbFileCreator.AppendLine(_HEADER_);

            foreach (KeyValuePair<string, string> pair in scenes)
            {
                sbFileCreator.AppendLine("\t\t[MenuItem(\"Assets/Select Scene/" + pair.Key + "\"" + ", false, 0)]").
                  AppendLine("\t\tstatic void Scene" + Mathf.Abs(pair.GetHashCode()) + "()").
                  AppendLine("\t\t{").
                  AppendLine("\t\t\tEditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();").
                  AppendLine("\t\t\tEditorSceneManager.OpenScene(\"" + pair.Value + "\");").
                  AppendLine("\t\t}");
            }

            sbFileCreator.AppendLine("// sceneCount=" + scenes.Count);
            sbFileCreator.AppendLine(_FOOTER_);

            string fullMenuFileName = Application.dataPath + "/Editor/com.pellegrinoprincipe/" + menuFileName;
            try
            {
                // if the file already exists, only overwrite... 
                using (FileStream fs = File.Create(fullMenuFileName))
                {
                    Byte[] info = new UTF8Encoding(true).GetBytes(sbFileCreator.ToString());
                    fs.Write(info, 0, info.Length);
                }

                AssetDatabase.ImportAsset("Assets/Editor/com.pellegrinoprincipe/" + menuFileName);
            }
            catch (IOException exc) { Debug.Log(exc); }
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            string elem = null;

            if (importedAssets.Length != 0)
                elem = Array.Find<string>(importedAssets, v => { return v.Contains(".unity"); });
            else if (movedAssets.Length != 0)
                elem = Array.Find<string>(movedAssets, v => { return v.Contains(".unity"); });
            else if (deletedAssets.Length != 0)
                elem = Array.Find<string>(deletedAssets, v => { return v.Contains(".unity"); });

            if (elem != null) // scenes deleted, imported or moved (i.e., renamed)... 
            {
                EditorPrefs.SetBool(keyPrefImpOrMoved, true);
                SearchScenes();
            }
        }
    }
}