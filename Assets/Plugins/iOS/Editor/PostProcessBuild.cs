using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

public class PostProcessBuild : MonoBehaviour
{
    [PostProcessBuild(0)]
    public static void OnPostprocessBuild(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget != BuildTarget.iOS)
            return;
        EditorUtility.DisplayDialog("ALERT!!!", "you must ADD 'KeyInputViewController.xib' into 'Copy Bundle Resources'\nat xcode 'Unity-iPhone' build target.", "got it");
        EditorUtility.DisplayDialog("ALERT!!!", "and, you must REMOVE 'KeyInputViewController.xib' in 'Copy Bundle Resources'\nat xcode 'UnityFramework' build target.", "got it");
    }
}
