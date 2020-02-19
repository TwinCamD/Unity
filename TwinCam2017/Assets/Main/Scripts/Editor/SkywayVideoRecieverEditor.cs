/*Summary
 *  InspectorのGUIを更新
 *
 *Log
 *  2018.8.10 森田 unity2017
 *  右クリックで選ぶのめんどいからボタンを用意
 */

using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(SkywayVideoReciever))]
public class SkywayVideoRecieverEditor : Editor {

    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        SkywayVideoReciever myScript = (SkywayVideoReciever)target;

        //if (GUILayout.Button("Get Peer Id")) {
        //    myScript.GetPeerId();
        //}
        if (GUILayout.Button("Make Call")) {
            myScript.MakeCall();
        }
        if (GUILayout.Button("End Call")) {
            myScript.EndCall();
        }
    }
}