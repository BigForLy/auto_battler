using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using GlubinnyChertog.Core;
using GlubinnyChertog.Sanity;
using GlubinnyChertog.Enemies;

namespace GlubinnyChertog.EditorTools
{
    /// <summary>
    /// Editor-only utility to scaffold the "Run" scene with core systems
    /// already placed and components attached. Saves manual setup time.
    /// Run via menu: Tools/GlubinnyChertog/Create Run Scene
    /// </summary>
    public static class RunSceneBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Run.unity";

        [MenuItem("Tools/GlubinnyChertog/Create Run Scene")]
        public static void CreateRunScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // --- Core Systems ---
            var coreSystems = new GameObject("_CoreSystems");
            var runManagerGO = new GameObject("RunManager");
            runManagerGO.transform.SetParent(coreSystems.transform);
            runManagerGO.AddComponent<RunManager>();

            var sanityGO = new GameObject("SanityManager");
            sanityGO.transform.SetParent(coreSystems.transform);
            sanityGO.AddComponent<SanityManager>();

            var reviveGO = new GameObject("ReviveManager");
            reviveGO.transform.SetParent(coreSystems.transform);
            reviveGO.AddComponent<ReviveManager>();

            // --- Player placeholder ---
            var player = GameObject.CreatePrimitive(PrimitiveType.Quad);
            player.name = "Player";
            player.transform.position = Vector3.zero;
            Object.DestroyImmediate(player.GetComponent<MeshCollider>());
            player.AddComponent<Rigidbody2D>().gravityScale = 0f;
            player.AddComponent<BoxCollider2D>();
            player.AddComponent<Player.PlayerController>();

            var firePoint = new GameObject("FirePoint");
            firePoint.transform.SetParent(player.transform);
            firePoint.transform.localPosition = Vector3.zero;

            // --- Enemy Spawner ---
            var spawnerGO = new GameObject("EnemySpawner");
            var spawner = spawnerGO.AddComponent<EnemySpawner>();
            var so = new SerializedObject(spawner);
            so.FindProperty("player").objectReferenceValue = player.transform;
            so.ApplyModifiedProperties();

            // --- Main Camera ---
            var cameraGO = new GameObject("Main Camera");
            var cam = cameraGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cameraGO.transform.position = new Vector3(0, 0, -10);
            cameraGO.tag = "MainCamera";

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"[RunSceneBuilder] Run scene created at {ScenePath}. " +
                      "Remember to assign EnemyData assets and prefabs on EnemySpawner manually.");
        }
    }
}
