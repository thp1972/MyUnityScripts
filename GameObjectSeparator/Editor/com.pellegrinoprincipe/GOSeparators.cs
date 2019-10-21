// GOSeparators V 0.3#21102019
// By Pellegrino ~thp~ Principe
// A little script to create and update game objects ***separators*** in the Hierarchy window

using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace com.pellegrinoprincipe
{
    public class GOSeparators : EditorWindow
    {
        string separatorName = "";
        string gameobjectTag = "EditorOnly";
        StringBuilder gameObjectSeparator;

        int charSepState;
        int alignState = 1;
        bool buttonStateOK;
        bool buttonStateCancel;

        // you can choose... 1, 2, etc.
        int littleGapFromRight = 1; // leave a little gap from the right border; 0 = no gap

        string[] charSepText = { "-", "*", "_" };
        string[] alignText = { "Left", "Center", "Right" };

        // prefs keys
        string prefCharSepText = "com.pellegrinoprincipe.charSepText";
        string prefAlignText = "com.pellegrinoprincipe.alignText";

        // the text in the Hierarchy window starts 30 pixel from the left border
        // could this in the future change? Maybe...
        int hierOffset = 52; // MAGIC NUMBER :)

        static GOSeparators Instance;

        void OnEnable() { getPreferences(); }

        void OnGUI()
        {
            CreateTheUI();
            ManageKeyPress();
        }

        // hotkey to activate the window: CTRL (CMD on macOS) + SHIFT + s 
        [MenuItem("COM.PELLEGRINOPRINCIPE.TOOLS/Separators/Create Separator... #c", false, 0)]
        public static void ShowWindow()
        {
            int startX = 500, startY = 200;
            int wWidth = 390, wHeight = 110;
            EditorWindow window = GetWindow<GOSeparators>(true, "*** Game Object Separators ***");
            window.position = new Rect(startX, startY, wWidth, wHeight);
            window.maxSize = window.minSize = new Vector2(wWidth, wHeight);
        }

        // hotkey to update the separators: CTRL (CMD on macOS) + SHIFT + U 
        [MenuItem("COM.PELLEGRINOPRINCIPE.TOOLS/Separators/Update Separators #u", false, 1)]
        public static void UpdateSeparators()
        {
            if (Instance == null)
                Instance = ScriptableObject.CreateInstance<GOSeparators>();
            Instance.ExecUpdateSeparators();
        }

        void CreateTheUI()
        {
            GUILayout.BeginArea(new Rect(5, 5, 380, 100), EditorStyles.helpBox);

            // input field
            GUI.SetNextControlName("txtSep");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Separator name: ");
            separatorName = GUILayout.TextField(separatorName, 50, GUILayout.Width(236));
            EditorGUI.FocusTextInControl("txtSep");

            GUILayout.EndHorizontal();

            // radio button - separator chars
            GUILayout.BeginHorizontal();
            GUILayout.Label("Separator char: ", GUILayout.Width(130));
            charSepState = GUILayout.SelectionGrid(charSepState, charSepText,
                                                 charSepText.Length, EditorStyles.radioButton);
            GUILayout.EndHorizontal();

            // radio button - alignment
            GUILayout.BeginHorizontal();
            GUILayout.Label("Separator alignment: ", GUILayout.Width(130));
            alignState = GUILayout.SelectionGrid(alignState, alignText,
                                                 alignText.Length, EditorStyles.radioButton);
            GUILayout.EndHorizontal();

            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            // buttons
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            buttonStateCancel = GUILayout.Button("Cancel", GUILayout.Width(80));
            buttonStateOK = GUILayout.Button("Ok", GUILayout.Width(80));
            GUILayout.EndHorizontal();

            if (buttonStateOK)
                MakeSeparator();

            if (buttonStateCancel)
                Close();

            GUILayout.EndArea();
        }

        private void getPreferences()
        {
            if (EditorPrefs.HasKey(prefCharSepText))
            {
                int ixCharSepState = EditorPrefs.GetInt(prefCharSepText);
                charSepState = ixCharSepState;
            }

            if (EditorPrefs.HasKey(prefAlignText))
            {
                int ixAlignTextState = EditorPrefs.GetInt(prefAlignText);
                alignState = ixAlignTextState;
            }
        }

        private void setPreferences() // are saved only if hitted OK...
        {
            EditorPrefs.SetInt(prefCharSepText, charSepState);
            EditorPrefs.SetInt(prefAlignText, alignState);
        }

        void ManageKeyPress()
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.Escape:
                    Close();
                    break;
                case KeyCode.Return:
                    MakeSeparator();
                    break;
            }
        }

        float GetHierarchyWindowWidth()
        {
            EditorApplication.ExecuteMenuItem("Window/General/Hierarchy");
            EditorWindow window = EditorWindow.focusedWindow;
            float hierWidth = window.position.width - hierOffset;
            return hierWidth;
        }

        void AssignTextAndSepWidth(out float textWidth, out float sepWidth, string separatorName, string sepText)
        {
            GUIStyle _style = new GUIStyle();
            textWidth = _style.CalcSize(new GUIContent(separatorName)).x;
            sepWidth = _style.CalcSize(new GUIContent(sepText)).x;
        }

        void MakeSeparator()
        {
            string sepText = charSepText[charSepState];

            // take the width of the Hierarchy window
            float hierWidth = GetHierarchyWindowWidth();

            // take the text and separator width
            float textWidth = 0, sepWidth = 0;
            AssignTextAndSepWidth(out textWidth, out sepWidth, separatorName, sepText);

            GameObject go = new GameObject
            (ParseTheSeparator(hierWidth, textWidth, sepWidth, sepText, separatorName, alignText[alignState]));

            go.tag = gameobjectTag;
            setPreferences();
            Close();
        }

        void ExecUpdateSeparators()
        {
            // take the width of the Hierarchy window
            float hierWidth = GetHierarchyWindowWidth();

            GameObject[] gos = GameObject.FindGameObjectsWithTag(gameobjectTag);
            foreach (GameObject go in gos)
            {
                float textWidth = 0, sepWidth = 0;
                string separatorName = Regex.Match(go.name, @"\b[^_ ]+\b").Value; // assert position at a word boundary...
                string sepText = Regex.Match(go.name, @"[*_-]").Value; //  a single character in the range between * (index 42) and _ (index 95)
                AssignTextAndSepWidth(out textWidth, out sepWidth, separatorName, sepText);

                // alignment?
                string alignText = "Center";
                char fChar = go.name[0];
                char lChar = go.name[go.name.Length - 1];

                if (Regex.Match(fChar.ToString(), @"[^*_-]").Length == 1) // Match a single character not present in the list below [^*_-]
                    alignText = "Left";
                else if (Regex.Match(lChar.ToString(), @"[^*_-]").Length == 1) // Match a single character not present in the list below [^*_-]
                    alignText = "Right";

                // update the separator
                go.name = ParseTheSeparator(hierWidth, textWidth, sepWidth, sepText, separatorName, alignText);
            }

        }

        string ParseTheSeparator(float hierWidth, float textWidth, float sepWidth, string sepText, string sepName, string alignText)
        {
            gameObjectSeparator = new StringBuilder();

            int howManyChars = Mathf.FloorToInt((hierWidth - textWidth) / sepWidth) - littleGapFromRight;

            for (int ix = 0; ix < howManyChars; ix++)
            {
                if (sepName.Length == 0) // print only separator char
                    gameObjectSeparator.Append(sepText);
                else
                {
                    switch (alignText)
                    {
                        case "Left":
                            if (ix == 0)
                                gameObjectSeparator.Append(sepName).Append(' ').Append(sepText);
                            else if (ix < howManyChars)
                                gameObjectSeparator.Append(sepText);
                            break;
                        case "Center":
                            if (ix < howManyChars / 2 || ix > howManyChars / 2)
                                gameObjectSeparator.Append(sepText);
                            else
                                gameObjectSeparator.Append(' ').Append(sepName).Append(' ');
                            break;
                        case "Right":
                            if (ix < howManyChars - 1)
                                gameObjectSeparator.Append(sepText);
                            else
                                gameObjectSeparator.Append(sepText).Append(' ').Append(sepName);
                            break;
                    }
                }
            }

            return gameObjectSeparator.ToString();
        }
    }
}