using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityToolbarExtender;
using UnityToolbarExtender.Examples;

namespace ggj25
{
    public class UnityToolbarBeginFirstLevel : MonoBehaviour
    {
        [InitializeOnLoad]
        public class SceneSwitchLeftButton
        {
            static SceneSwitchLeftButton()
            {
                ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUI);
            }

            static void OnToolbarGUI()
            {
                GUILayout.FlexibleSpace();

                if(GUILayout.Button(new GUIContent("Play From Scene 0", "Start Scene 1")))
                {
                    SceneHelper.StartScene("MainScene");
                }
                
                if(GUILayout.Button(new GUIContent("Open Main Scene", "Start Scene 1")))
                {
                    EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
                }
                
                if(GUILayout.Button(new GUIContent("Open Game Scene", "Start Scene 1")))
                {
                    EditorSceneManager.OpenScene("Assets/Scenes/GameScene.unity");
                }
            }
        }
    }
}
