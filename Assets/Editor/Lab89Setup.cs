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
    const string TimedDoorPrefabPath = PrefabsRoot + "/TimedDoor.prefab";
    const string TimedSwitchPrefabPath = PrefabsRoot + "/TimedSwitch.prefab";
    const string EnemyBuffedPrefabPath = PrefabsRoot + "/EnemyBuffed.prefab";

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

    // Альтернативний твіст для лабораторної 8:
    // плита відкриває двері на час, за якими стоїть посилений ворог.
    public static void Lab8_Step2_TimedDoorTwist()
    {
        var scene = EditorSceneManager.OpenScene(MyLevelPath);

        var risingType = LoadScriptType("Assets/_2DAdventureGame/Scripts/RisingWater.cs");
        foreach (var go in UnityEngine.Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None)
                     .Where(obj => obj.GetComponent(risingType) != null))
        {
            UnityEngine.Object.DestroyImmediate(go);
        }

        DestroyIfExists("TimedDoor");
        DestroyIfExists("TimedSwitchPlate");
        DestroyIfExists("EnemyBuffedGate");

        var doorPrefab = EnsureTimedDoorPrefab();
        var switchPrefab = EnsureTimedSwitchPrefab();
        var enemyBuffedPrefab = EnsureEnemyBuffedPrefab();

        var door = (GameObject)PrefabUtility.InstantiatePrefab(doorPrefab, scene);
        door.name = "TimedDoor";
        door.transform.position = new Vector3(5.1f, -0.95f, 0f);
        door.transform.localScale = new Vector3(0.27f, 0.27f, 1f);

        var plate = (GameObject)PrefabUtility.InstantiatePrefab(switchPrefab, scene);
        plate.name = "TimedSwitchPlate";
        plate.transform.position = new Vector3(0.1f, -1.15f, 0f);
        plate.transform.localScale = new Vector3(1.0f, 1.0f, 1f);

        var timedSwitchType = LoadScriptType("Assets/_2DAdventureGame/Scripts/TimedSwitch.cs");
        var timedSwitch = plate.GetComponent(timedSwitchType);
        var tsSo = new SerializedObject(timedSwitch);
        tsSo.FindProperty("door").objectReferenceValue = door;
        tsSo.FindProperty("openTime").floatValue = 4.5f;
        tsSo.ApplyModifiedPropertiesWithoutUndo();

        var buffedEnemy = (GameObject)PrefabUtility.InstantiatePrefab(enemyBuffedPrefab, scene);
        buffedEnemy.name = "EnemyBuffedGate";
        buffedEnemy.transform.position = new Vector3(9.0f, -0.9f, 0f);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("LAB8 OK: TimedSwitch + door + buffed enemy placed");
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

    // Фікс для статичної камери: вмикаємо пряме слідування за гравцем.
    public static void Lab9_Step2_CameraFollowFix()
    {
        var scene = EditorSceneManager.OpenScene(MyLevelPath);
        var player = GameObject.Find("PlayerCharacter");
        var mainCamera = GameObject.Find("Main Camera");
        if (player == null || mainCamera == null)
        {
            throw new Exception("PlayerCharacter or Main Camera not found in MyLevel");
        }

        // Щоб не було конфлікту, вимикаємо cinemachine-логіку для камери в цій сцені.
        foreach (var mb in mainCamera.GetComponents<MonoBehaviour>())
        {
            if (mb != null && mb.GetType().FullName.Contains("CinemachineBrain"))
            {
                mb.enabled = false;
            }
        }

        var virtualCamera = GameObject.Find("2D Follow Camera");
        if (virtualCamera != null)
        {
            virtualCamera.SetActive(false);
        }

        var cameraFollowType = LoadScriptType("Assets/_2DAdventureGame/Scripts/CameraFollow2D.cs");
        var follow = mainCamera.GetComponent(cameraFollowType);
        if (follow == null)
        {
            follow = mainCamera.AddComponent(cameraFollowType);
        }

        var so = new SerializedObject(follow);
        so.FindProperty("target").objectReferenceValue = player.transform;
        so.FindProperty("smoothSpeed").floatValue = 7.0f;
        so.FindProperty("offset").vector3Value = new Vector3(0f, 0f, -10f);
        so.FindProperty("minBounds").vector2Value = new Vector2(-11.0f, -2.6f);
        so.FindProperty("maxBounds").vector2Value = new Vector2(10.4f, 1.9f);
        so.ApplyModifiedPropertiesWithoutUndo();

        var cameraComponent = mainCamera.GetComponent<Camera>();
        cameraComponent.orthographic = true;
        cameraComponent.orthographicSize = 4.4f;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("LAB9 OK: camera follow fix applied");
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

    static void DestroyIfExists(string objectName)
    {
        var go = GameObject.Find(objectName);
        if (go != null)
        {
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    static GameObject EnsureTimedDoorPrefab()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TimedDoorPrefabPath);
        if (prefab != null)
        {
            return prefab;
        }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_2DAdventureGame/Art/Ducko/Environmnent/Decoration_3.png");
        if (sprite == null)
        {
            throw new Exception("Decoration_3 sprite not found for TimedDoor");
        }

        var door = new GameObject("TimedDoor");
        var sr = door.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = 2;

        var collider = door.AddComponent<BoxCollider2D>();
        collider.isTrigger = false;
        var size = sprite.bounds.size;
        collider.size = new Vector2(size.x * 0.6f, size.y * 0.24f);
        collider.offset = new Vector2(0f, size.y * 0.08f);

        var created = PrefabUtility.SaveAsPrefabAsset(door, TimedDoorPrefabPath);
        UnityEngine.Object.DestroyImmediate(door);
        return created;
    }

    static GameObject EnsureTimedSwitchPrefab()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(TimedSwitchPrefabPath);
        if (prefab != null)
        {
            return prefab;
        }

        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_2DAdventureGame/Art/Ducko/Environmnent/Decoration_1.png");
        if (sprite == null)
        {
            throw new Exception("Decoration_1 sprite not found for TimedSwitch");
        }

        var plate = new GameObject("TimedSwitch");
        var sr = plate.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.color = new Color(1f, 0.92f, 0.65f, 1f);
        sr.sortingOrder = 1;

        var collider = plate.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        var size = sprite.bounds.size;
        collider.size = new Vector2(size.x * 0.75f, size.y * 0.45f);
        collider.offset = new Vector2(0f, size.y * 0.1f);

        var timedSwitchType = LoadScriptType("Assets/_2DAdventureGame/Scripts/TimedSwitch.cs");
        plate.AddComponent(timedSwitchType);

        var created = PrefabUtility.SaveAsPrefabAsset(plate, TimedSwitchPrefabPath);
        UnityEngine.Object.DestroyImmediate(plate);
        return created;
    }

    static GameObject EnsureEnemyBuffedPrefab()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyBuffedPrefabPath);
        if (prefab != null)
        {
            return prefab;
        }

        var baseEnemy = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsRoot}/EnemyFast.prefab")
                       ?? AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsRoot}/Enemy.prefab");
        if (baseEnemy == null)
        {
            throw new Exception("Enemy base prefab not found");
        }

        var enemy = (GameObject)PrefabUtility.InstantiatePrefab(baseEnemy);
        enemy.name = "EnemyBuffed";
        var controller = enemy.GetComponent("EnemyController");
        var so = new SerializedObject(controller);
        so.FindProperty("speed").floatValue = 2.6f;
        so.FindProperty("changeTime").floatValue = 1.2f;
        so.FindProperty("vertical").boolValue = false;
        so.FindProperty("randomAxis").boolValue = false;
        so.ApplyModifiedPropertiesWithoutUndo();

        var sr = enemy.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = new Color(1f, 0.45f, 0.25f, 1f);
        }

        var created = PrefabUtility.SaveAsPrefabAssetAndConnect(enemy, EnemyBuffedPrefabPath, InteractionMode.AutomatedAction);
        UnityEngine.Object.DestroyImmediate(enemy);
        return created;
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
