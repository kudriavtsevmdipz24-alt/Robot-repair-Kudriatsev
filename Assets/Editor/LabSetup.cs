using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEditor.SceneManagement;

// Допоміжний скрипт для налаштування ассетів лабораторних робіт.
// Запускається з командного рядка: -executeMethod LabSetup.Метод
public static class LabSetup
{
    const string ArtRoot = "Assets/_2DAdventureGame/Art/Ducko";
    const string AnimRoot = "Assets/_2DAdventureGame/Animation/Ducko";
    const string ScriptsRoot = "Assets/_2DAdventureGame/Scripts";
    const string PrefabsRoot = "Assets/_2DAdventureGame/Prefabs";
    const string EnemySheetPath = ArtRoot + "/Characters/EnemySheet.png";

    static Sprite SheetSprite(string name)
    {
        var sprite = AssetDatabase.LoadAllAssetsAtPath(EnemySheetPath)
            .OfType<Sprite>().FirstOrDefault(s => s.name == name);
        if (sprite == null) throw new Exception("Sheet sprite not found: " + name);
        return sprite;
    }

    static MonoScript LoadScript(string fileName)
    {
        var ms = AssetDatabase.LoadAssetAtPath<MonoScript>($"{ScriptsRoot}/{fileName}");
        if (ms == null) throw new Exception("Script not found: " + fileName);
        return ms;
    }

    // ─────────────────────────── ЛАБОРАТОРНА 5 ───────────────────────────

    // Крок 1+11: Pivot ворога, Pixels Per Unit листа ворога і снаряда
    public static void Lab5_Step1_Assets()
    {
        var enemy = AssetImporter.GetAtPath(ArtRoot + "/Characters/Enemy.png") as TextureImporter;
        var enemySettings = new TextureImporterSettings();
        enemy.ReadTextureSettings(enemySettings);
        enemySettings.spritePixelsPerUnit = 100;
        enemySettings.spriteAlignment = (int)SpriteAlignment.Custom; // Custom = 9
        enemySettings.spritePivot = new Vector2(0.5f, 0f);
        enemy.SetTextureSettings(enemySettings);
        enemy.SaveAndReimport();

        var sheet = AssetImporter.GetAtPath(EnemySheetPath) as TextureImporter;
        sheet.spritePixelsPerUnit = 100;
        sheet.SaveAndReimport();

        var proj = AssetImporter.GetAtPath(ArtRoot + "/VFX/Projectile.png") as TextureImporter;
        proj.spritePixelsPerUnit = 300;
        proj.SaveAndReimport();

        Debug.Log("LAB5 STEP1 OK: importers set (Enemy pivot 0.5/0, sheet PPU 100, projectile PPU 300)");
    }

    static AnimationClip MakeEnemyClip(string clipName, string[] frameNames, int flipX)
    {
        var clip = new AnimationClip { name = clipName };
        clip.frameRate = 4f; // Samples = 4: кадр видно 0.25 с — «ходить машина, а не мерехтить»

        var frames = new ObjectReferenceKeyframe[frameNames.Length + 1];
        for (int i = 0; i <= frameNames.Length; i++)
        {
            frames[i] = new ObjectReferenceKeyframe
            {
                time = i / 4f,
                value = SheetSprite(frameNames[i % frameNames.Length])
            };
        }
        AnimationUtility.SetObjectReferenceCurve(
            clip, EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite"), frames);

        // Flip X: 1 у кліпі праворуч, 0 — ліворуч (як у демо-контролері)
        clip.SetCurve("", typeof(SpriteRenderer), "m_FlipX",
            AnimationCurve.Constant(0f, frameNames.Length / 4f, flipX));

        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        AssetDatabase.CreateAsset(clip, $"{AnimRoot}/Enemy/AnimationClips/{clipName}.anim");
        return clip;
    }

    // Кроки 6-8: кліпи EnemyLeft / EnemyRight / EnemyFixed і контролер Enemy з Blend Tree
    public static void Lab5_Step2_ClipsAndController()
    {
        var left = MakeEnemyClip("EnemyLeft",
            new[] { "GolemBoilerWalkSide1", "GolemBoilerWalkSide2", "GolemBoilerWalkSide3", "GolemBoilerWalkSide4" }, 0);
        var right = MakeEnemyClip("EnemyRight",
            new[] { "GolemBoilerWalkSide1", "GolemBoilerWalkSide2", "GolemBoilerWalkSide3", "GolemBoilerWalkSide4" }, 1);
        var fixedClip = MakeEnemyClip("EnemyFixed",
            new[] { "GolemBoilerFixedSide1", "GolemBoilerFixedSide2", "GolemBoilerFixedSide3", "GolemBoilerFixedSide4" }, 0);

        var down = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimRoot}/Enemy/AnimationClips/EnemyDown.anim");
        var up = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{AnimRoot}/Enemy/AnimationClips/EnemyUp.anim");
        if (down == null || up == null) throw new Exception("EnemyDown/EnemyUp clips not found");

        // Контролер Enemy: параметри Move X, Move Y і тригер Fixed; Blend Tree 2D Simple Directional
        var controller = AnimatorController.CreateAnimatorControllerAtPath($"{AnimRoot}/Enemy/Enemy.controller");
        controller.AddParameter("Move X", AnimatorControllerParameterType.Float);
        controller.AddParameter("Move Y", AnimatorControllerParameterType.Float);
        controller.AddParameter("Fixed", AnimatorControllerParameterType.Trigger);

        var blendTree = new BlendTree
        {
            name = "Blend Tree",
            hideFlags = HideFlags.HideInHierarchy,
            blendType = (BlendTreeType)1, // Simple Directional
            blendParameter = "Move X",
            blendParameterY = "Move Y",
            children = new[]
            {
                new ChildMotion { motion = down, position = new Vector2(0f, -1f) },
                new ChildMotion { motion = left, position = new Vector2(-1f, 0f) },
                new ChildMotion { motion = right, position = new Vector2(1f, 0f) },
                new ChildMotion { motion = up, position = new Vector2(0f, 1f) },
            }
        };
        AssetDatabase.AddObjectToAsset(blendTree, controller);

        var sm = controller.layers[0].stateMachine;
        var btState = sm.AddState("Blend Tree");
        btState.motion = blendTree;
        sm.defaultState = btState;

        var fixedState = sm.AddState("Fixed");
        fixedState.motion = fixedClip;

        var transition = btState.AddTransition(fixedState);
        transition.AddCondition(AnimatorConditionMode.If, 0, "Fixed");
        transition.hasExitTime = false;
        transition.duration = 0f;

        AssetDatabase.SaveAssets();
        Debug.Log("LAB5 STEP2 OK: clips EnemyLeft/EnemyRight/EnemyFixed + controller Enemy with Blend Tree");
    }

    // Кроки 1, 11, 12: префаб ворога, префаб снаряда, Animator і поля на PlayerCharacter
    public static void Lab5_Step3_Prefabs()
    {
        var enemyControllerType = LoadScript("EnemyController.cs").GetClass();
        var projectileType = LoadScript("Projectile.cs").GetClass();

        var enemySprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtRoot + "/Characters/Enemy.png");
        var projectileSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ArtRoot + "/VFX/Projectile.png");

        // ── Префаб ворога ──
        var enemyGO = new GameObject("Enemy");
        var enemySR = enemyGO.AddComponent<SpriteRenderer>();
        enemySR.sprite = enemySprite;
        enemySR.spriteSortPoint = SpriteSortPoint.Pivot;

        var enemyCol = enemyGO.AddComponent<BoxCollider2D>();
        enemyCol.isTrigger = true;
        var bounds = enemySprite.bounds;
        enemyCol.size = new Vector2(bounds.size.x * 0.55f, bounds.size.y * 0.65f);
        enemyCol.offset = new Vector2(0f, bounds.size.y * 0.35f);

        var enemyRB = enemyGO.AddComponent<Rigidbody2D>();
        enemyRB.gravityScale = 0f;
        enemyRB.constraints = RigidbodyConstraints2D.FreezeRotation;

        enemyGO.AddComponent(enemyControllerType);

        var enemyAnim = enemyGO.AddComponent<Animator>();
        enemyAnim.runtimeAnimatorController =
            AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{AnimRoot}/Enemy/Enemy.controller");

        PrefabUtility.SaveAsPrefabAsset(enemyGO, $"{PrefabsRoot}/Enemy.prefab");
        UnityEngine.Object.DestroyImmediate(enemyGO);

        // ── Префаб снаряда ──
        var projGO = new GameObject("Projectile");
        var projSR = projGO.AddComponent<SpriteRenderer>();
        projSR.sprite = projectileSprite;
        projSR.sortingOrder = 1;

        var projCol = projGO.AddComponent<BoxCollider2D>();
        projCol.isTrigger = true;
        var pb = projectileSprite.bounds;
        projCol.size = new Vector2(Mathf.Max(0.2f, pb.size.x * 0.7f), Mathf.Max(0.2f, pb.size.y * 0.7f));

        var projRB = projGO.AddComponent<Rigidbody2D>();
        projRB.gravityScale = 0f;

        projGO.AddComponent(projectileType);

        var projPrefab = PrefabUtility.SaveAsPrefabAsset(projGO, $"{PrefabsRoot}/Projectile.prefab");
        UnityEngine.Object.DestroyImmediate(projGO);

        // ── PlayerCharacter: Animator + посилання на снаряд + клавіша C ──
        var playerPath = $"{PrefabsRoot}/PlayerCharacter.prefab";
        var player = PrefabUtility.LoadPrefabContents(playerPath);
        try
        {
            var playerAnim = player.GetComponent<Animator>();
            if (playerAnim == null || !playerAnim)
            {
                playerAnim = player.AddComponent<Animator>();
            }
            playerAnim.runtimeAnimatorController =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>($"{AnimRoot}/Player.controller");

            var pc = player.GetComponent("PlayerController");
            if (pc == null) throw new Exception("PlayerController component not found on PlayerCharacter prefab");
            var so = new SerializedObject(pc);

            var prefabProp = so.FindProperty("projectilePrefab");
            if (prefabProp == null) throw new Exception("projectilePrefab property not found");
            prefabProp.objectReferenceValue = projPrefab;

            SetupButtonAction(so, "LaunchAction", "Launch", "<Keyboard>/c");

            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.SaveAsPrefabAsset(player, playerPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(player);
        }

        Debug.Log("LAB5 STEP3 OK: Enemy + Projectile prefabs, PlayerCharacter animator + Launch(C) + projectile ref");
    }

    static void SetupButtonAction(SerializedObject so, string fieldName, string actionName, string keyPath)
    {
        var p = so.FindProperty(fieldName);
        if (p == null) throw new Exception(fieldName + " property not found");

        p.FindPropertyRelative("m_Name").stringValue = actionName;
        p.FindPropertyRelative("m_Type").intValue = 1; // Button
        p.FindPropertyRelative("m_ExpectedControlType").stringValue = "Button";
        p.FindPropertyRelative("m_Id").stringValue = Guid.NewGuid().ToString("D");
        p.FindPropertyRelative("m_Processors").stringValue = "";
        p.FindPropertyRelative("m_Interactions").stringValue = "";
        p.FindPropertyRelative("m_Flags").intValue = 0;
        p.FindPropertyRelative("m_Priority").intValue = 0;

        var bindings = p.FindPropertyRelative("m_SingletonActionBindings");
        bindings.arraySize = 1;
        var b = bindings.GetArrayElementAtIndex(0);
        b.FindPropertyRelative("m_Name").stringValue = keyPath;
        b.FindPropertyRelative("m_Id").stringValue = Guid.NewGuid().ToString("D");
        b.FindPropertyRelative("m_Path").stringValue = keyPath;
        b.FindPropertyRelative("m_Interactions").stringValue = "";
        b.FindPropertyRelative("m_Processors").stringValue = "";
        b.FindPropertyRelative("m_Groups").stringValue = "";
        b.FindPropertyRelative("m_Action").stringValue = actionName;
        b.FindPropertyRelative("m_Flags").intValue = 8;
    }

    public static void Lab5_Diag()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_2DAdventureGame/Scenes/MainScene.unity");
        var tm = GameObject.Find("Tilemap");
        Debug.Log("DIAG Tilemap GO: " + (tm != null ? tm.name : "null"));
        var tilemap = tm != null ? tm.GetComponent<Tilemap>() : null;
        Debug.Log("DIAG Tilemap component: " + (tilemap != null));
        if (tilemap != null)
        {
            var b = tilemap.cellBounds;
            Debug.Log($"DIAG cellBounds: {b.position} .. {b.position + b.size}");
            int count = 0;
            int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
            foreach (var p in b.allPositionsWithin)
            {
                if (tilemap.HasTile(p))
                {
                    count++;
                    if (p.x < minX) minX = p.x;
                    if (p.x > maxX) maxX = p.x;
                    if (p.y < minY) minY = p.y;
                    if (p.y > maxY) maxY = p.y;
                }
            }
            Debug.Log($"DIAG tiles: {count}, x range {minX}..{maxX}, y range {minY}..{maxY}");
        }
        Debug.Log("DIAG OK");
    }

    // Крок 15: шари Character і Projectiles + вимкнення перетину; кроки 1-5: вороги на сцені
    public static void Lab5_Step4_LayersAndScene()
    {
        // Шари 6 = Character, 7 = Projectiles
        var tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset").First());
        var layers = tagManager.FindProperty("layers");
        layers.GetArrayElementAtIndex(6).stringValue = "Character";
        layers.GetArrayElementAtIndex(7).stringValue = "Projectiles";
        tagManager.ApplyModifiedProperties();

        // Матриця колізій: знімаємо перетин Character × Projectiles
        Physics2D.IgnoreLayerCollision(6, 7, true);
        uint mask6 = (uint)Physics2D.GetLayerCollisionMask(6);
        uint mask7 = (uint)Physics2D.GetLayerCollisionMask(7);
        Debug.Log($"Engine masks after IgnoreLayerCollision: layer6={mask6:X8}, layer7={mask7:X8}");

        var physics = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/Physics2DSettings.asset").First());
        var matrixProp = physics.FindProperty("m_LayerCollisionMatrix");
        var hex = matrixProp.stringValue;
        if (hex == null || hex.Length % 8 != 0) throw new Exception("Unexpected LayerCollisionMatrix format: " + hex);
        int count = hex.Length / 8;
        var rows = new uint[count];
        for (int i = 0; i < count; i++)
            rows[i] = Convert.ToUInt32(hex.Substring(i * 8, 8), 16);
        if (count > 6) rows[6] = mask6;
        if (count > 7) rows[7] = mask7;
        matrixProp.stringValue = string.Concat(rows.Select(r => r.ToString("x8")));
        physics.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(physics.targetObject);

        // ── Вороги на сцені MainScene ──
        var scene = EditorSceneManager.OpenScene("Assets/_2DAdventureGame/Scenes/MainScene.unity");
        var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsRoot}/Enemy.prefab");
        if (enemyPrefab == null) throw new Exception("Enemy prefab not found");

        var tilemapGO = GameObject.Find("Tilemap");
        Tilemap tilemap = tilemapGO != null ? tilemapGO.GetComponent<Tilemap>() : null;

        int enemyCount = 0;

        // Гравець стоїть на підлозі — від його позиції і розставляємо ворогів
        var playerGO = GameObject.Find("PlayerCharacter");
        if (playerGO == null) throw new Exception("PlayerCharacter not found in scene");
        var py = playerGO.transform.position.y;

        var placements = new[]
        {
            new { x = -6f, y = py, speed = 1f, vertical = false, changeTime = 3f, name = "Enemy A (horizontal)" },
            new { x = 3f,  y = py, speed = 1f, vertical = true,  changeTime = 3f, name = "Enemy B (vertical)" },
        };
        foreach (var pl in placements)
        {
            var e = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab, scene);
            e.transform.position = new Vector3(pl.x, pl.y, 0f);
            var ctrl = e.GetComponent("EnemyController");
            var so = new SerializedObject(ctrl);
            so.FindProperty("speed").floatValue = pl.speed;
            so.FindProperty("vertical").boolValue = pl.vertical;
            so.FindProperty("changeTime").floatValue = pl.changeTime;
            so.ApplyModifiedPropertiesWithoutUndo();
            enemyCount++;
            Debug.Log($"{pl.name} at ({pl.x}, {pl.y}), speed={pl.speed}, changeTime={pl.changeTime}");
        }

        if (enemyCount == 0) throw new Exception("No enemy could be placed");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"LAB5 STEP4 OK: layers Character/Projectiles, collision matrix, {enemyCount} enemies placed");
    }

    // ДЗ юніта 4: третій ворог, що випадково міняє вісь руху
    public static void Lab5_Bonus_RandomAxisEnemy()
    {
        var scene = EditorSceneManager.OpenScene("Assets/_2DAdventureGame/Scenes/MainScene.unity");
        var enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{PrefabsRoot}/Enemy.prefab");
        var playerGO = GameObject.Find("PlayerCharacter");
        if (playerGO == null) throw new Exception("PlayerCharacter not found in scene");
        var py = playerGO.transform.position.y;

        var e = (GameObject)PrefabUtility.InstantiatePrefab(enemyPrefab, scene);
        e.transform.position = new Vector3(0f, py + 1.0f, 0f);
        var so = new SerializedObject(e.GetComponent("EnemyController"));
        so.FindProperty("speed").floatValue = 1f;
        so.FindProperty("vertical").boolValue = true;
        so.FindProperty("changeTime").floatValue = 3f;
        so.FindProperty("randomAxis").boolValue = true;
        so.ApplyModifiedPropertiesWithoutUndo();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("LAB5 BONUS OK: random-axis enemy placed");
    }
}
