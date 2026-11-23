using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Lab89Setup
{
    const string MyLevelPath = "Assets/_2DAdventureGame/Scenes/MyLevel.unity";
    const string StartSceneTemplatePath = "Assets/_2DAdventureGame/Tutorial_Demo/Demo_Scenes/Final/StartScreen.unity";
    const string StartScenePath = "Assets/_2DAdventureGame/Scenes/StartScreen.unity";

    const string PrefabsRoot = "Assets/_2DAdventureGame/Prefabs";
    const string AudioRoot = "Assets/_2DAdventureGame/Audio";

    const string RisingWaterPrefabPath = PrefabsRoot + "/RisingWater.prefab";
    const string RisingWaterFastPrefabPath = PrefabsRoot + "/RisingWaterFast.prefab";

    static Type LoadScriptType(string scriptPath)
    {
        var script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
        if (script == null)
        {
            throw new Exception("Script not found: " + scriptPath);
        }

        var type = script.GetClass();
        if (type == null)
        {
            throw new Exception("Script class not found: " + scriptPath);
        }
        return type;
    }

    static AudioClip LoadClip(string clipName)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{AudioRoot}/{clipName}");
        if (clip == null)
        {
            throw new Exception("Audio clip not found: " + clipName);
        }
        return clip;
    }

    // Лабораторна 8: твіст "вода, що піднімається" + prefab variant.
    public static void Lab8_Step1_RisingWaterTwist()
    {
        var risingType = LoadScriptType("Assets/_2DAdventureGame/Scripts/RisingWater.cs");
        var scene = EditorSceneManager.OpenScene(MyLevelPath);

        // При повторному запуску методу прибираємо старі інстанси.
        var waters = UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
            .Where(go => go.GetComponent(risingType) != null)
            .ToArray();
        foreach (var go in waters)
        {
            UnityEngine.Object.DestroyImmediate(go);
        }

        var basePrefab = EnsureRisingWaterPrefab(risingType, RisingWaterPrefabPath, 0.22f, -0.6f, 1, new Color(0.31f, 0.65f, 0.92f, 0.66f));
        var fastVariantPrefab = EnsureRisingWaterPrefab(risingType, RisingWaterFastPrefabPath, 0.32f, 0.1f, 1, new Color(0.18f, 0.53f, 0.90f, 0.68f), basePrefab);

        PlaceWater(scene, basePrefab, risingType, "RisingWater_Main", new Vector3(-2.7f, -5.3f, 0f), new Vector3(9.5f, 2.1f, 1f), 0.22f, -0.55f, 1);
        PlaceWater(scene, fastVariantPrefab, risingType, "RisingWater_Fast", new Vector3(7.3f, -5.0f, 0f), new Vector3(6.2f, 1.9f, 1f), 0.32f, 0.1f, 1);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("LAB8 OK: RisingWater twist prefab + variant + MyLevel placement");
    }

    // Лабораторна 9: аудіо-фідбек, стартова сцена, build settings і параметри вікна.
    public static void Lab9_Step1_PolishSetup()
    {
        ConfigurePlayerAudio();
        ConfigureEnemyAudio();
        EnsureStartScene();
        ConfigureBuildScenes();
        ConfigurePlayerSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("LAB9 OK: polish setup done (audio, start screen, build scenes, player settings)");
    }

    static void ConfigurePlayerAudio()
    {
        var playerPath = $"{PrefabsRoot}/PlayerCharacter.prefab";
        var prefab = PrefabUtility.LoadPrefabContents(playerPath);
        try
        {
            var playerController = prefab.GetComponent("PlayerController");
            if (playerController == null)
            {
                throw new Exception("PlayerController component missing on PlayerCharacter.prefab");
            }

            var audioSource = prefab.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = prefab.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0.85f;

            var so = new SerializedObject(playerController);
            so.FindProperty("playerHitSound").objectReferenceValue = LoadClip("PlayerHit.wav");
            so.FindProperty("collectibleSound").objectReferenceValue = LoadClip("Collectible.wav");
            so.FindProperty("projectileThrowSound").objectReferenceValue = LoadClip("Projectile.wav");
            so.FindProperty("feedbackFlashTime").floatValue = 0.14f;
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefab, playerPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefab);
        }
    }

    static void ConfigureEnemyAudio()
    {
        ConfigureEnemyPrefab($"{PrefabsRoot}/Enemy.prefab");
        ConfigureEnemyPrefab($"{PrefabsRoot}/EnemyFast.prefab");
    }

    static void ConfigureEnemyPrefab(string path)
    {
        var prefab = PrefabUtility.LoadPrefabContents(path);
        try
        {
            var enemyController = prefab.GetComponent("EnemyController");
            if (enemyController == null)
            {
                throw new Exception("EnemyController missing on " + path);
            }

            var audioSource = prefab.GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = prefab.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
            audioSource.loop = false;
            audioSource.spatialBlend = 0f;
            audioSource.volume = 0.8f;

            var so = new SerializedObject(enemyController);
            so.FindProperty("fixedSound").objectReferenceValue = LoadClip("EnemyFix.wav");
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(prefab, path);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefab);
        }
    }

    static void EnsureStartScene()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(StartScenePath) != null)
        {
            return;
        }

        if (!AssetDatabase.CopyAsset(StartSceneTemplatePath, StartScenePath))
        {
            throw new Exception("Failed to copy start scene template");
        }
    }

    static void ConfigureBuildScenes()
    {
        var buildScenes = new[]
        {
            new EditorBuildSettingsScene(StartScenePath, true),
            new EditorBuildSettingsScene(MyLevelPath, true),
        };
        EditorBuildSettings.scenes = buildScenes;
    }

    static void ConfigurePlayerSettings()
    {
        PlayerSettings.productName = "Robot Repair - Flood Run";
        PlayerSettings.companyName = "Kudriatsev Team";
        PlayerSettings.defaultScreenWidth = 1280;
        PlayerSettings.defaultScreenHeight = 720;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
    }

    static GameObject EnsureRisingWaterPrefab(Type risingType, string path, float riseSpeed, float maxHeight, int damage, Color tint, GameObject variantParent = null)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null)
        {
            if (variantParent == null)
            {
                var temp = BuildRisingWaterTemplate(risingType, riseSpeed, maxHeight, damage, tint);
                prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
                UnityEngine.Object.DestroyImmediate(temp);
            }
            else
            {
                var temp = (GameObject)PrefabUtility.InstantiatePrefab(variantParent);
                ConfigureRisingWaterObject(temp, risingType, riseSpeed, maxHeight, damage, tint);
                prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(temp, path, InteractionMode.AutomatedAction);
                UnityEngine.Object.DestroyImmediate(temp);
            }
        }
        else
        {
            var contents = PrefabUtility.LoadPrefabContents(path);
            try
            {
                ConfigureRisingWaterObject(contents, risingType, riseSpeed, maxHeight, damage, tint);
                PrefabUtility.SaveAsPrefabAsset(contents, path);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(contents);
            }
        }
        return AssetDatabase.LoadAssetAtPath<GameObject>(path);
    }

    static GameObject BuildRisingWaterTemplate(Type risingType, float riseSpeed, float maxHeight, int damage, Color tint)
    {
        var damageZonePrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsRoot}/DamageZone.prefab");
        if (damageZonePrefab == null)
        {
            throw new Exception("DamageZone prefab not found");
        }
        var sourceRenderer = damageZonePrefab.GetComponent<SpriteRenderer>();

        var go = new GameObject("RisingWater");
        ConfigureRisingWaterObject(go, risingType, riseSpeed, maxHeight, damage, tint);

        var sr = go.GetComponent<SpriteRenderer>();
        sr.sprite = sourceRenderer.sprite;
        sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(2f, 2f);
        sr.sortingOrder = -1;
        return go;
    }

    static void ConfigureRisingWaterObject(GameObject go, Type risingType, float riseSpeed, float maxHeight, int damage, Color tint)
    {
        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) sr = go.AddComponent<SpriteRenderer>();
        sr.color = tint;

        var collider = go.GetComponent<BoxCollider2D>();
        if (collider == null) collider = go.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(2f, 2f);

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.simulated = true;

        var legacyDamage = go.GetComponent("DamageZone");
        if (legacyDamage != null)
        {
            UnityEngine.Object.DestroyImmediate((Component)legacyDamage);
        }

        var rising = go.GetComponent(risingType);
        if (rising == null) rising = go.AddComponent(risingType);

        var so = new SerializedObject(rising);
        so.FindProperty("riseSpeed").floatValue = riseSpeed;
        so.FindProperty("maxHeight").floatValue = maxHeight;
        so.FindProperty("damage").intValue = damage;
        so.ApplyModifiedPropertiesWithoutUndo();
    }

    static void PlaceWater(UnityEngine.SceneManagement.Scene scene, GameObject prefab, Type risingType, string objectName, Vector3 position, Vector3 scale, float riseSpeed, float maxHeight, int damage)
    {
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        instance.name = objectName;
        instance.transform.position = position;
        instance.transform.localScale = scale;

        var rising = instance.GetComponent(risingType);
        var so = new SerializedObject(rising);
        so.FindProperty("riseSpeed").floatValue = riseSpeed;
        so.FindProperty("maxHeight").floatValue = maxHeight;
        so.FindProperty("damage").intValue = damage;
        so.ApplyModifiedPropertiesWithoutUndo();
    }
}
