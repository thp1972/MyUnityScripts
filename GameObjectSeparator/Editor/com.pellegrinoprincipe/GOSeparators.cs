// GOSeparators V 0.1#14092017
// By Pellegrino ~thp~ Principe
// A little script to create game objects separators in the Hierarchy window

using System.Text;
using UnityEditor;
using UnityEngine;

namespace com.pellegrinoprincipe
{
    public class GOSeparators : EditorWindow
    {
        string separatorName = "";
        StringBuilder gameObjectSeparator;

        int charSepState;
        int alignState = 1;
        bool buttonStateOK;
        bool buttonStateCancel;

        // you can choose... 1, 2, etc.
        int littleGapFromRight; // leave a little gap from the right border, else no gap

        string[] charSepText = { "-", "*", "_" };
        string[] alignText = { "Left", "Center", "Right" };

        // prefs keys
        string prefCharSepText = "com.pellegrinoprincipe.charSepText";
        string prefAlignText = "com.pellegrinoprincipe.alignText";

        // the text in the Hierarchy window starts 30 pixel from the left border
        // could this in the future change? Maybe...
        int hierOffset = 30; // MAGIC NUMBER :)

        // hotkey to activate the window: ALT + SHIFT + s 
        [MenuItem("GameObject/Create Separator... &#s")]
        public static void ShowWindow()
        {
            int startX = 500, startY = 200;
            int wWidth = 390, wHeight = 110;
            EditorWindow window = GetWindow<GOSeparators>(true, "*** Game Object Separators ***");
            window.position = new Rect(startX, startY, wWidth, wHeight);
            window.maxSize = window.minSize = new Vector2(wWidth, wHeight);
        }

        private void OnEnable() { getPreferences(); }

        void OnGUI()
        {
            CreateTheUI();
            ManageKeyPress();
        }

        void CreateTheUI()
        {
            // input field
            GUI.SetNextControlName("txtSep");
            GUILayout.BeginArea(new Rect(5, 5, 380, 100), EditorStyles.helpBox);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Separator name: ");
            separatorName = GUILayout.TextField(separatorName, 50, GUILayout.Width(236));
            GUILayout.EndHorizontal();

            EditorGUI.FocusTextInControl("txtSep");

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

        void MakeSeparator()
        {
            GameObject go = new GameObject(ParseTheSeparator());
            go.tag = "EditorOnly";
            setPreferences();
            Close();
        }

        string ParseTheSeparator()
        {
            gameObjectSeparator = new StringBuilder();

            // take the width of the Hierarchy window
            EditorApplication.ExecuteMenuItem("Window/Hierarchy");
            EditorWindow window = EditorWindow.focusedWindow;
            float hierWidth = window.position.width - hierOffset;

            // take the text and separator width
            GUIStyle _style = new GUIStyle();
            float textWidth = _style.CalcSize(new GUIContent(separatorName)).x;
            float sepWidth = _style.CalcSize(new GUIContent(charSepText[charSepState])).x;

            int howManyChars = Mathf.FloorToInt((hierWidth - textWidth) / sepWidth) - littleGapFromRight;

            for (int ix = 0; ix < howManyChars; ix++)
            {
                if (separatorName.Length == 0) // print only separator char
                    gameObjectSeparator.Append(charSepText[charSepState]);
                else
                {
                    switch (alignText[alignState])
                    {
                        case "Left":
                            if (ix == 0)
                                gameObjectSeparator.Append(separatorName).Append(' ').Append(charSepText[charSepState]);
                            else if (ix < howManyChars)
                                gameObjectSeparator.Append(charSepText[charSepState]);
                            break;
                        case "Center":
                            if (ix < howManyChars / 2 || ix > howManyChars / 2)
                                gameObjectSeparator.Append(charSepText[charSepState]);
                            else
                                gameObjectSeparator.Append(' ').Append(separatorName).Append(' ');
                            break;
                        case "Right":
                            if (ix < howManyChars - 1)
                                gameObjectSeparator.Append(charSepText[charSepState]);
                            else
                                gameObjectSeparator.Append(charSepText[charSepState]).Append(' ').Append(separatorName);
                            break;
                    }
                }
            }

            return gameObjectSeparator.ToString();
        }
    }
}