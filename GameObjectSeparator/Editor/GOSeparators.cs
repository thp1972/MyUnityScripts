using System.Text;
using UnityEditor;
using UnityEngine;

public class GOSeparators : EditorWindow
{
    string separatorName = "";
    StringBuilder gameObjectSeparator;
    char prePostchar = '-';
    readonly int howManyChars = 9;

    // hotkey ALT+SHIFT+s
    [MenuItem("GameObject/Create Separator... &#s")]
    public static void ShowWindow()
    {
        GetWindow<GOSeparators>(true, "*** Game Object Separators ***");
    }

    void OnGUI()
    {
        GUI.SetNextControlName("txtSep");
        separatorName = EditorGUILayout.TextField("Separator name:", separatorName);
        EditorGUI.FocusTextInControl("txtSep");

        GUILayout.Space(15);

        if (GUILayout.Button("Ok"))
        {
            GameObject go = new GameObject(ParseTheSeparatorName(separatorName));
            go.tag = "EditorOnly";
            Close();
        }
    }

    string ParseTheSeparatorName(string separatorName)
    {
        gameObjectSeparator = new StringBuilder(separatorName);

        for (int ix = 0; ix < howManyChars; ix++)
        {
            gameObjectSeparator.Insert(ix, separatorName.Length != 0 && ix == howManyChars - 1 ? ' ' : prePostchar);
            gameObjectSeparator.Append(separatorName.Length != 0 && ix == 0 ? ' ' : prePostchar);
        }

        return gameObjectSeparator.ToString();
    }
}
