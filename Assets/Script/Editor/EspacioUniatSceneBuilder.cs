using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

//Construye las escenas de Espacio UNIAT desde el menu "Espacio UNIAT"
//Este archivo: escenas generadas (recomendacion, menu de planetas, llegada a cada mundo) y helpers
//EspacioUniatSceneBuilder.Restyle.cs: estilo aplicado sobre las escenas existentes 0 y 1
[InitializeOnLoad]
public static partial class EspacioUniatSceneBuilder
{
    //Si existe este archivo, al terminar de compilar se construye automaticamente una sola vez
    //Contenido: vacio = todo, "buildsettings" = solo Build Settings, "restyle" = solo escenas 0 y 1
    private const string AutoBuildMarker = "Temp/EspacioUniat_AutoBuild";
    private static bool autoMode;

    private const string RecommendationScenePath = "Assets/Scenes/recomendation.unity";
    private const string PlanetMenuScenePath = "Assets/Scenes/3.unity";
    private const string WorldScenesFolder = "Assets/Scenes/worlds";
    private const string BackupFolder = "Assets/Scenes/_Backup";
    private const string GeneratedArtFolder = "Assets/Art/Generated";
    private const string PrefabFolder = "Assets/Prefabs/UI";
    private const string PlanetCardPrefabPath = PrefabFolder + "/PlanetCard.prefab";
    private const string EventSystemPrefabPath = "Assets/Scenes/EventSystem.prefab";

    private static readonly string[] BuildScenes =
    {
        "Assets/Scenes/0.unity",
        "Assets/Scenes/1.unity",
        "Assets/Scenes/2.unity",
        RecommendationScenePath,
        PlanetMenuScenePath,
        WorldScenesFolder + "/W_IPV.unity",
        WorldScenesFolder + "/W_LAV.unity",
        WorldScenesFolder + "/W_CAD.unity",
        WorldScenesFolder + "/W_CS.unity",
    };

    private static readonly Color SpaceBackground = new(0.027f, 0.04f, 0.12f);
    private static readonly Color NebulaPurple = new(0.54f, 0.36f, 1f, 0.22f);
    private static readonly Color NebulaCyan = new(0.31f, 0.82f, 1f, 0.14f);
    private static readonly Color Accent = new(0.31f, 0.82f, 1f);
    private static readonly Color Highlight = new(1f, 0.83f, 0.31f);
    private static readonly Color PanelColor = new(1f, 1f, 1f, 0.07f);
    private static readonly Color SoftWhite = new(1f, 1f, 1f, 0.75f);

    private static double nextMarkerCheck;
    private static readonly DateTime loadedAt = DateTime.Now;

    static EspacioUniatSceneBuilder()
    {
        EditorApplication.delayCall += RunPendingAutoBuild;
        EditorApplication.update += PollAutoBuildMarker;
    }

    //Revisa el marcador cada 2 segundos, asi funciona aunque no haya recompilacion
    private static void PollAutoBuildMarker()
    {
        if (EditorApplication.timeSinceStartup < nextMarkerCheck) return;
        nextMarkerCheck = EditorApplication.timeSinceStartup + 2;
        if (File.Exists(AutoBuildMarker) && !EditorApplication.isCompiling && !EditorApplication.isUpdating && !ScriptsChangedSinceLoad())
        {
            RunPendingAutoBuild();
        }
    }

    //Si algun script se modifico despues de cargar este codigo, falta recompilar y no hay que correr la version vieja
    private static bool ScriptsChangedSinceLoad()
    {
        return Directory.GetFiles("Assets/Script", "*.cs", SearchOption.AllDirectories)
            .Any(path => File.GetLastWriteTime(path) > loadedAt);
    }

    private static void RunPendingAutoBuild()
    {
        if (!File.Exists(AutoBuildMarker) || EditorApplication.isPlayingOrWillChangePlaymode) return;
        string mode = File.ReadAllText(AutoBuildMarker).Trim();
        File.Delete(AutoBuildMarker);

        autoMode = true;
        try
        {
            if (mode == "buildsettings") AddScenesToBuildSettings();
            else if (mode == "restyle")
            {
                if (RestyleExistingScenes()) EditorSceneManager.OpenScene(Scene0Path);
            }
            else BuildAll();
            Debug.Log("[Espacio UNIAT] Auto build terminado.");
        }
        catch (Exception e)
        {
            Debug.LogError($"[Espacio UNIAT] Auto build fallo: {e}");
        }
        finally
        {
            autoMode = false;
        }
    }

    [MenuItem("Espacio UNIAT/Construir todo", priority = 0)]
    private static void BuildAll()
    {
        if (!BuildRecommendationScene() || !BuildPlanetMenuScene() || !BuildWorldScenes() || !RestyleExistingScenes())
        {
            Debug.LogWarning("[Espacio UNIAT] Construccion cancelada.");
            return;
        }
        AddScenesToBuildSettings();
        EditorSceneManager.OpenScene(Scene0Path);
    }

    [MenuItem("Espacio UNIAT/1. Escena recomendation", priority = 20)]
    private static void BuildRecommendationSceneMenu()
    {
        BuildRecommendationScene();
    }

    [MenuItem("Espacio UNIAT/2. Escena 3 (Menu de planetas)", priority = 21)]
    private static void BuildPlanetMenuSceneMenu()
    {
        BuildPlanetMenuScene();
    }

    [MenuItem("Espacio UNIAT/3. Escenas de llegada a cada mundo (W_*)", priority = 22)]
    private static void BuildWorldScenesMenu()
    {
        BuildWorldScenes();
    }

    [MenuItem("Espacio UNIAT/5. Agregar escenas a Build Settings", priority = 40)]
    private static void AddScenesToBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        foreach (string path in BuildScenes)
        {
            if (File.Exists(path) && !scenes.Any(s => s.path == path))
            {
                scenes.Add(new EditorBuildSettingsScene(path, true));
            }
        }
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();
        Debug.Log("[Espacio UNIAT] Build Settings actualizados.");
    }

    #region Revelacion (recomendacion y llegada a mundos)

    private static bool BuildRecommendationScene()
    {
        return BuildRevealScene(RecommendationScenePath, null, new List<string> { "NUESTRA", "RECOMENDACIÓN ES" });
    }

    private static bool BuildWorldScenes()
    {
        foreach (worldInfo world in LoadWorlds())
        {
            string path = $"{WorldScenesFolder}/{world.sceneName}.unity";
            if (string.IsNullOrEmpty(world.sceneName) || !File.Exists(path))
            {
                Debug.LogWarning($"[Espacio UNIAT] No existe la escena {path} para {world.name}, se omite.");
                continue;
            }
            if (!BuildRevealScene(path, world, new List<string> { "BIENVENIDO A" })) return false;
        }
        return true;
    }

    //Escena con textos POP y el planeta revelado. Sin fixedWorld es la recomendacion, con fixedWorld es la llegada a ese mundo
    private static bool BuildRevealScene(string _path, worldInfo _fixedWorld, List<string> _introLines)
    {
        if (!PrepareScene(_path)) return false;
        bool isRecommendation = _fixedWorld == null;

        Sprite circle = GetCircleSprite();
        Sprite glow = GetGlowSprite();
        Sprite rounded = GetRoundedSprite();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCommonObjects();
        Canvas canvas = CreateCanvas("Canvas");
        CreateSpaceBackground(canvas.transform, glow);

        //Planeta (se crea antes que el texto para que quede detras)
        RectTransform planetGroup = CreateRect(canvas.transform, "PlanetGroup", new Vector2(0, 40), new Vector2(600, 600));
        RectTransform planetVisual = CreateRect(planetGroup, "PlanetVisual", Vector2.zero, new Vector2(560, 560));
        planetVisual.gameObject.AddComponent<FloatIdle>().amplitude = 14f;
        Image planetGlow = CreateImage(planetVisual, "PlanetGlow", glow, new Color(1, 1, 1, 0.5f), Vector2.zero, new Vector2(560, 560));
        Image planetImage = CreateImage(planetVisual, "Planet", circle, Color.white, Vector2.zero, new Vector2(300, 300));
        TextMeshProUGUI careerText = CreateText(planetGroup, "CareerText", "Carrera", 42, Accent, new Vector2(0, -200), new Vector2(1400, 70));

        TextMeshProUGUI popText = CreateText(canvas.transform, "PopText", _introLines[0], 120, Color.white, new Vector2(0, 370), new Vector2(1800, 170));
        popText.fontStyle = FontStyles.Bold;

        //Botones
        RectTransform buttons = CreateRect(canvas.transform, "Buttons", new Vector2(0, -350), new Vector2(1100, 120));
        CanvasGroup buttonsGroup = buttons.gameObject.AddComponent<CanvasGroup>();
        Button goToPlanetBT = null;
        Button otherPlanetsBT;
        if (isRecommendation)
        {
            goToPlanetBT = CreateButton(buttons, "BT_MyPlanet", "IR A MI PLANETA", new Vector2(-265, 0), new Vector2(500, 110), rounded, Accent, SpaceBackground);
            otherPlanetsBT = CreateButton(buttons, "BT_OtherPlanets", "VER OTROS PLANETAS", new Vector2(265, 0), new Vector2(500, 110), rounded, PanelColor, Color.white);
        }
        else
        {
            otherPlanetsBT = CreateButton(buttons, "BT_BackToSpace", "VOLVER AL ESPACIO", Vector2.zero, new Vector2(560, 110), rounded, PanelColor, Color.white);
        }

        CreateText(canvas.transform, "Footer", "ESPACIO UNIAT", 28, new Color(1, 1, 1, 0.4f), new Vector2(0, -500), new Vector2(800, 50));

        //Feel
        MMF_Player popFeedback = popText.gameObject.AddComponent<MMF_Player>();
        AddScaleFeedback(popFeedback, popText.transform, 0.35f, 1.3f, "Pop");

        MMF_Player planetRevealFeedback = planetGroup.gameObject.AddComponent<MMF_Player>();
        AddScaleFeedback(planetRevealFeedback, planetGroup, 0.6f, 1.15f, "Planet Pop");
        AddCanvasGroupFadeIn(planetRevealFeedback, buttonsGroup, 0.5f, 0.6f, "Buttons Fade In");

        //Logica
        GameObject screenGO = new GameObject("RecommendationScreen");
        RecommendationScreen screen = screenGO.AddComponent<RecommendationScreen>();
        screen.worlds = LoadWorlds();
        screen.fixedWorld = _fixedWorld;
        screen.introLines = _introLines;
        screen.popText = popText;
        screen.planetGroup = planetGroup;
        screen.planetImage = planetImage;
        screen.planetGlow = planetGlow;
        screen.careerText = careerText;
        screen.buttonsGroup = buttonsGroup;
        screen.goToPlanetBT = goToPlanetBT;
        screen.otherPlanetsBT = otherPlanetsBT;
        screen.popFeedback = popFeedback;
        screen.planetRevealFeedback = planetRevealFeedback;

        if (!isRecommendation)
        {
            //Preview en el editor
            planetImage.color = _fixedWorld.planetColor;
            careerText.text = _fixedWorld.carrera.GetDisplayName();
        }

        EditorSceneManager.SaveScene(scene, _path);
        Debug.Log($"[Espacio UNIAT] Escena creada: {_path}");
        return true;
    }

    #endregion

    #region Menu de planetas

    private static bool BuildPlanetMenuScene()
    {
        if (!PrepareScene(PlanetMenuScenePath)) return false;

        Sprite circle = GetCircleSprite();
        Sprite glow = GetGlowSprite();
        Sprite rounded = GetRoundedSprite();
        List<worldInfo> worlds = LoadWorlds();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCommonObjects();
        Canvas canvas = CreateCanvas("Canvas");
        CreateSpaceBackground(canvas.transform, glow);

        TextMeshProUGUI title = CreateText(canvas.transform, "Title", "ESPACIO UNIAT", 110, Color.white, new Vector2(0, 410), new Vector2(1600, 150));
        title.fontStyle = FontStyles.Bold;
        title.characterSpacing = 12;
        title.gameObject.AddComponent<FloatIdle>().amplitude = 8f;
        CreateText(canvas.transform, "Subtitle", "Elige el planeta que quieres explorar", 40, Accent, new Vector2(0, 310), new Vector2(1600, 60));
        CreateText(canvas.transform, "Footer", "Toca un planeta para viajar", 28, new Color(1, 1, 1, 0.45f), new Vector2(0, -490), new Vector2(800, 50));

        MMF_Player titleFeedback = title.gameObject.AddComponent<MMF_Player>();
        AddScaleFeedback(titleFeedback, title.transform, 0.45f, 1.2f, "Title Pop");

        GameObject cardPrefab = CreatePlanetCardPrefab(circle, glow, rounded);

        RectTransform cardsRoot = CreateRect(canvas.transform, "Planets", new Vector2(0, -80), new Vector2(1760, 560));
        List<PlanetCard> cards = new();
        for (int i = 0; i < worlds.Count; i++)
        {
            GameObject cardGO = (GameObject)PrefabUtility.InstantiatePrefab(cardPrefab, cardsRoot);
            cardGO.name = $"PlanetCard_{worlds[i].carrera}";
            ((RectTransform)cardGO.transform).anchoredPosition = new Vector2(-660 + i * 440, 0);
            PlanetCard card = cardGO.GetComponent<PlanetCard>();
            card.world = worlds[i];
            //Preview en el editor, en runtime lo vuelve a llenar PlanetCard.Setup
            card.planetNameText.text = worlds[i].worldName.ToUpper();
            card.careerText.text = worlds[i].carrera.GetDisplayName();
            card.planetImage.color = worlds[i].planetColor;
            Color glowColor = worlds[i].planetColor;
            glowColor.a = card.planetGlow.color.a;
            card.planetGlow.color = glowColor;
            PrefabUtility.RecordPrefabInstancePropertyModifications(cardGO);
            PrefabUtility.RecordPrefabInstancePropertyModifications(cardGO.transform);
            PrefabUtility.RecordPrefabInstancePropertyModifications(card);
            PrefabUtility.RecordPrefabInstancePropertyModifications(card.planetNameText);
            PrefabUtility.RecordPrefabInstancePropertyModifications(card.careerText);
            PrefabUtility.RecordPrefabInstancePropertyModifications(card.planetImage);
            PrefabUtility.RecordPrefabInstancePropertyModifications(card.planetGlow);
            cards.Add(card);
        }

        GameObject menuGO = new GameObject("PlanetSelectionMenu");
        PlanetSelectionMenu menu = menuGO.AddComponent<PlanetSelectionMenu>();
        menu.cards = cards;
        menu.titleText = title;
        menu.titleFeedback = titleFeedback;

        EditorSceneManager.SaveScene(scene, PlanetMenuScenePath);
        Debug.Log($"[Espacio UNIAT] Escena creada: {PlanetMenuScenePath}");
        return true;
    }

    private static GameObject CreatePlanetCardPrefab(Sprite circle, Sprite glow, Sprite rounded)
    {
        EnsureFolder(PrefabFolder);

        Image background = CreateImage(null, "PlanetCard", rounded, PanelColor, Vector2.zero, new Vector2(400, 560), true);
        background.type = Image.Type.Sliced;
        GameObject root = background.gameObject;

        Button button = root.AddComponent<Button>();
        button.targetGraphic = background;
        SetTranslucentButtonColors(button);

        RectTransform planetVisual = CreateRect(root.transform, "PlanetVisual", new Vector2(0, 110), new Vector2(380, 380));
        planetVisual.gameObject.AddComponent<FloatIdle>().amplitude = 10f;
        Image planetGlow = CreateImage(planetVisual, "PlanetGlow", glow, new Color(1, 1, 1, 0.45f), Vector2.zero, new Vector2(380, 380));
        Image planetImage = CreateImage(planetVisual, "Planet", circle, Color.white, Vector2.zero, new Vector2(210, 210));

        Image badge = CreateImage(root.transform, "RecommendedBadge", rounded, Highlight, new Vector2(0, 268), new Vector2(270, 56));
        badge.type = Image.Type.Sliced;
        TextMeshProUGUI badgeText = CreateText(badge.transform, "Label", "RECOMENDADO", 28, SpaceBackground, Vector2.zero, new Vector2(270, 56));
        badgeText.fontStyle = FontStyles.Bold;

        TextMeshProUGUI planetName = CreateText(root.transform, "PlanetName", "PLANETA", 42, Color.white, new Vector2(0, -50), new Vector2(380, 60));
        planetName.fontStyle = FontStyles.Bold;
        TextMeshProUGUI careerText = CreateText(root.transform, "Career", "Carrera", 24, SoftWhite, new Vector2(0, -130), new Vector2(350, 90));

        Image explore = CreateImage(root.transform, "Explore", rounded, new Color(Accent.r, Accent.g, Accent.b, 0.2f), new Vector2(0, -225), new Vector2(230, 58));
        explore.type = Image.Type.Sliced;
        TextMeshProUGUI exploreText = CreateText(explore.transform, "Label", "EXPLORAR", 26, Accent, Vector2.zero, new Vector2(230, 58));
        exploreText.fontStyle = FontStyles.Bold;

        PlanetCard card = root.AddComponent<PlanetCard>();
        card.planetImage = planetImage;
        card.planetGlow = planetGlow;
        card.planetNameText = planetName;
        card.careerText = careerText;
        card.recommendedBadge = badge.gameObject;
        card.button = button;

        MMF_Player appear = root.AddComponent<MMF_Player>();
        AddScaleFeedback(appear, root.transform, 0.45f, 1.15f, "Card Pop");
        card.appearFeedback = appear;
        AddButtonFeel(button);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PlanetCardPrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    #endregion

    #region Feel

    private static MMF_Scale AddScaleFeedback(MMF_Player _player, Transform _target, float _duration, float _overshoot, string _label, float _delay = 0f)
    {
        return AddScaleFeedback(_player, _target, _duration, PopCurve(_overshoot), _label, _delay);
    }

    private static MMF_Scale AddScaleFeedback(MMF_Player _player, Transform _target, float _duration, AnimationCurve _curve, string _label, float _delay = 0f)
    {
        MMF_Scale scale = (MMF_Scale)_player.AddFeedback(typeof(MMF_Scale));
        scale.Label = _label;
        //Feel migra estas curvas legacy en OnValidate y truena si son null (solo pasa al crear el feedback por codigo)
        scale.AnimateScaleX = new AnimationCurve();
        scale.AnimateScaleY = new AnimationCurve();
        scale.AnimateScaleZ = new AnimationCurve();
        scale.AnimateScaleTarget = _target;
        scale.Mode = MMF_Scale.Modes.Absolute;
        scale.AnimateScaleDuration = _duration;
        scale.RemapCurveZero = 0f;
        scale.RemapCurveOne = 1f;
        scale.AnimateScaleTweenX = new MMTweenType(new AnimationCurve(_curve.keys), "AnimateX");
        scale.AnimateScaleTweenY = new MMTweenType(new AnimationCurve(_curve.keys), "AnimateY");
        scale.AnimateScaleTweenZ = new MMTweenType(new AnimationCurve(_curve.keys), "AnimateZ");
        scale.Timing.InitialDelay = _delay;
        EditorUtility.SetDirty(_player);
        return scale;
    }

    private static void AddCanvasGroupFadeIn(MMF_Player _player, CanvasGroup _target, float _delay, float _duration, string _label)
    {
        MMF_CanvasGroup fade = (MMF_CanvasGroup)_player.AddFeedback(typeof(MMF_CanvasGroup));
        fade.Label = _label;
        fade.TargetCanvasGroup = _target;
        fade.Mode = MMF_FeedbackBase.Modes.OverTime;
        fade.Duration = _duration;
        fade.AlphaCurve = new MMTweenType(new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1)));
        fade.RemapZero = 0f;
        fade.RemapOne = 1f;
        fade.Timing.InitialDelay = _delay;
        EditorUtility.SetDirty(_player);
    }

    //Shake horizontal (en pixeles) para avisar de un error
    private static MMF_Player AddShakeFeedback(GameObject _host, float _strength, string _label)
    {
        MMF_Player player = CreateFeelHost(_host.transform, "Feel_Shake");
        MMF_Position shake = (MMF_Position)player.AddFeedback(typeof(MMF_Position));
        shake.Label = _label;
        shake.AnimatePositionCurve = new AnimationCurve();
        shake.AnimatePositionCurveX = new AnimationCurve();
        shake.AnimatePositionCurveY = new AnimationCurve();
        shake.AnimatePositionCurveZ = new AnimationCurve();
        shake.AnimatePositionTarget = _host;
        shake.Mode = MMF_Position.Modes.AlongCurve;
        shake.Space = MMF_Position.Spaces.RectTransform;
        shake.AnimatePositionDuration = 0.45f;
        shake.AnimateX = true;
        shake.AnimateY = false;
        shake.AnimateZ = false;
        shake.AnimatePositionTweenX = new MMTweenType(new AnimationCurve(
            new Keyframe(0f, 0f), new Keyframe(0.12f, 1f), new Keyframe(0.3f, -1f), new Keyframe(0.48f, 0.7f),
            new Keyframe(0.66f, -0.45f), new Keyframe(0.84f, 0.2f), new Keyframe(1f, 0f)), "AnimateX");
        shake.RemapCurveZero = 0f;
        shake.RemapCurveOne = _strength;
        shake.RelativePosition = true;
        EditorUtility.SetDirty(player);
        return player;
    }

    //Crece desde 0, se pasa un poco (overshoot) y se asienta en 1, eso da el efecto POP
    private static AnimationCurve PopCurve(float _overshoot)
    {
        return new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.55f, _overshoot),
            new Keyframe(0.8f, 0.95f),
            new Keyframe(1f, 1f));
    }

    //Se encoge al presionar y rebota
    private static AnimationCurve PressCurve()
    {
        return new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(0.25f, 0.86f),
            new Keyframe(0.6f, 1.08f),
            new Keyframe(1f, 1f));
    }

    //MMF_Player no permite dos en el mismo GameObject, los feedbacks extra viven en un hijo "Feel_*"
    private static MMF_Player CreateFeelHost(Transform _parent, string _name)
    {
        Transform existing = _parent.Find(_name);
        if (existing != null) Object.DestroyImmediate(existing.gameObject);

        GameObject host = new GameObject(_name, typeof(RectTransform));
        host.transform.SetParent(_parent, false);
        return host.AddComponent<MMF_Player>();
    }

    private static void AddButtonFeel(Button _button)
    {
        ButtonFeel feel = _button.GetComponent<ButtonFeel>();
        if (feel != null && feel.pressFeedback != null) return;
        if (feel == null) feel = _button.gameObject.AddComponent<ButtonFeel>();

        MMF_Player player = CreateFeelHost(_button.transform, "Feel_Press");
        AddScaleFeedback(player, _button.transform, 0.3f, PressCurve(), "Press Punch");
        feel.pressFeedback = player;
        EditorUtility.SetDirty(feel);
    }

    //Crea (o recrea) un FeelPopGroup con un POP por objetivo, uno tras otro
    private static FeelPopGroup SetupPopGroup(GameObject _host, IEnumerable<Transform> _targets, float _stagger, bool _playOnEnable)
    {
        FeelPopGroup group = _host.GetComponent<FeelPopGroup>();
        if (group != null) Object.DestroyImmediate(group);

        group = _host.AddComponent<FeelPopGroup>();
        group.targets = _targets.Where(t => t != null).ToList();
        group.playOnEnable = _playOnEnable;
        group.popFeedback = CreateFeelHost(_host.transform, "Feel_Pop");
        for (int i = 0; i < group.targets.Count; i++)
        {
            AddScaleFeedback(group.popFeedback, group.targets[i], 0.45f, 1.18f, $"Pop {group.targets[i].name}", i * _stagger);
        }
        EditorUtility.SetDirty(group);
        return group;
    }

    #endregion

    #region Objetos comunes

    //Guarda cambios pendientes y respalda la escena si ya existe
    private static bool PrepareScene(string _path)
    {
        if (!SaveOpenScenesIfNeeded()) return false;

        if (File.Exists(_path))
        {
            bool rebuild = autoMode || EditorUtility.DisplayDialog("Espacio UNIAT",
                $"Se va a reconstruir {_path}.\nSe guardará una copia de la escena actual en {BackupFolder}.",
                "Reconstruir", "Cancelar");
            if (!rebuild) return false;
            BackupScene(_path);
        }
        return true;
    }

    private static bool SaveOpenScenesIfNeeded()
    {
        //En auto build una escena sin titulo es una construccion temporal que fallo, se descarta sin preguntar
        if (autoMode && string.IsNullOrEmpty(SceneManager.GetActiveScene().path)) return true;
        return EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
    }

    private static void BackupScene(string _path)
    {
        EnsureFolder(BackupFolder);
        string backupPath = $"{BackupFolder}/{Path.GetFileNameWithoutExtension(_path)}_{DateTime.Now:yyyyMMdd_HHmmss}.unity";
        AssetDatabase.CopyAsset(_path, backupPath);
    }

    private static void CreateCommonObjects()
    {
        GameObject cameraGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraGO.tag = "MainCamera";
        cameraGO.transform.position = new Vector3(0, 0, -10);
        StyleCamera(cameraGO.GetComponent<Camera>());

        GameObject eventSystemPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EventSystemPrefabPath);
        if (eventSystemPrefab != null)
        {
            PrefabUtility.InstantiatePrefab(eventSystemPrefab);
        }
        else
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }

        //Mismo patron que las otras escenas: al iniciar hace FadeIn del StaticMenu
        StaticMenuBridge bridge = new GameObject("StaticMenuBridge").AddComponent<StaticMenuBridge>();
        CallBackerEvent callBacker = new GameObject("CallBackerEvent").AddComponent<CallBackerEvent>();
        callBacker.onStart ??= new UnityEvent();
        UnityEventTools.AddVoidPersistentListener(callBacker.onStart, bridge.FadeIn);
        EditorUtility.SetDirty(callBacker);
    }

    private static void StyleCamera(Camera _camera)
    {
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = SpaceBackground;
        EditorUtility.SetDirty(_camera);
    }

    private static Canvas CreateCanvas(string _name)
    {
        GameObject go = new GameObject(_name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        SetupCanvasScaler(go.GetComponent<CanvasScaler>());
        return canvas;
    }

    private static void SetupCanvasScaler(CanvasScaler _scaler)
    {
        _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        _scaler.referenceResolution = new Vector2(1920, 1080);
        _scaler.matchWidthOrHeight = 0.5f;
        EditorUtility.SetDirty(_scaler);
    }

    private static void CreateSpaceBackground(Transform _canvas, Sprite _glow)
    {
        Image background = CreateImage(_canvas, "SpaceBackground", null, SpaceBackground, Vector2.zero, Vector2.zero);
        Stretch(background.rectTransform);
        background.transform.SetAsFirstSibling();

        CreateImage(background.transform, "NebulaPurple", _glow, NebulaPurple, new Vector2(-620, 280), new Vector2(1300, 1300));
        CreateImage(background.transform, "NebulaCyan", _glow, NebulaCyan, new Vector2(700, -320), new Vector2(1200, 1200));

        RectTransform starsRect = CreateRect(background.transform, "StarField", Vector2.zero, Vector2.zero);
        Stretch(starsRect);
        StarField stars = starsRect.gameObject.AddComponent<StarField>();
        stars.starSprite = _glow;
    }

    #endregion

    #region Helpers UI

    private static RectTransform CreateRect(Transform _parent, string _name, Vector2 _position, Vector2 _size)
    {
        GameObject go = new GameObject(_name, typeof(RectTransform));
        if (_parent != null) go.transform.SetParent(_parent, false);
        RectTransform rt = (RectTransform)go.transform;
        rt.anchoredPosition = _position;
        rt.sizeDelta = _size;
        return rt;
    }

    private static void Stretch(RectTransform _rt)
    {
        _rt.anchorMin = Vector2.zero;
        _rt.anchorMax = Vector2.one;
        _rt.offsetMin = Vector2.zero;
        _rt.offsetMax = Vector2.zero;
    }

    private static Image CreateImage(Transform _parent, string _name, Sprite _sprite, Color _color, Vector2 _position, Vector2 _size, bool _raycastTarget = false)
    {
        RectTransform rt = CreateRect(_parent, _name, _position, _size);
        Image image = rt.gameObject.AddComponent<Image>();
        image.sprite = _sprite;
        image.color = _color;
        image.raycastTarget = _raycastTarget;
        return image;
    }

    private static TextMeshProUGUI CreateText(Transform _parent, string _name, string _text, float _fontSize, Color _color, Vector2 _position, Vector2 _size)
    {
        RectTransform rt = CreateRect(_parent, _name, _position, _size);
        TextMeshProUGUI text = rt.gameObject.AddComponent<TextMeshProUGUI>();
        StyleText(text, _text, _fontSize, _color);
        return text;
    }

    private static void StyleText(TextMeshProUGUI _text, string _content, float _fontSize, Color _color)
    {
        if (TMP_Settings.defaultFontAsset != null) _text.font = TMP_Settings.defaultFontAsset;
        _text.text = _content;
        //Algunos textos originales traian margenes y estilos que los desacomodaban
        _text.margin = Vector4.zero;
        _text.fontStyle = FontStyles.Normal;
        _text.fontSize = _fontSize;
        _text.enableAutoSizing = false;
        _text.color = _color;
        _text.alignment = TextAlignmentOptions.Center;
        _text.raycastTarget = false;
        EditorUtility.SetDirty(_text);
    }

    private static Button CreateButton(Transform _parent, string _name, string _label, Vector2 _position, Vector2 _size, Sprite _sprite, Color _background, Color _textColor)
    {
        Image image = CreateImage(_parent, _name, _sprite, _background, _position, _size, true);
        image.type = Image.Type.Sliced;
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        if (_background.a < 0.5f) SetTranslucentButtonColors(button);
        else SetSolidButtonColors(button);
        TextMeshProUGUI label = CreateText(image.transform, "Label", _label, 36, _textColor, Vector2.zero, _size);
        label.fontStyle = FontStyles.Bold;
        AddButtonFeel(button);
        return button;
    }

    //Para fondos casi transparentes: al pasar el mouse o tocar se vuelven mas opacos
    private static void SetTranslucentButtonColors(Button _button)
    {
        ColorBlock colors = _button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1f, 1f, 1f, 2f);
        colors.pressedColor = new Color(1f, 1f, 1f, 3f);
        colors.selectedColor = Color.white;
        _button.colors = colors;
    }

    //Para fondos de color: se oscurecen un poco al pasar el mouse o tocar
    private static void SetSolidButtonColors(Button _button)
    {
        ColorBlock colors = _button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
        colors.pressedColor = new Color(0.78f, 0.78f, 0.78f);
        colors.selectedColor = Color.white;
        _button.colors = colors;
    }

    #endregion

    #region Assets

    private static List<worldInfo> LoadWorlds()
    {
        List<worldInfo> worlds = AssetDatabase.FindAssets("t:worldInfo")
            .Select(guid => AssetDatabase.LoadAssetAtPath<worldInfo>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(w => w != null)
            .OrderBy(w => w.carrera)
            .ToList();

        if (worlds.Count == 0)
        {
            Debug.LogWarning("[Espacio UNIAT] No se encontraron assets worldInfo.");
        }
        return worlds;
    }

    private static Sprite GetRoundedSprite()
    {
        return AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
    }

    private static Sprite GetCircleSprite()
    {
        //Circulo solido con 1px de antialiasing en el borde
        return GetOrCreateSprite("Circle", 256, (distance, radius) => Mathf.Clamp01(radius - distance));
    }

    private static Sprite GetGlowSprite()
    {
        //Degradado radial suave para halos, nebulosas y estrellas
        return GetOrCreateSprite("Glow", 256, (distance, radius) =>
        {
            float t = Mathf.Clamp01(1f - distance / radius);
            return t * t;
        });
    }

    private static Sprite GetOrCreateSprite(string _name, int _size, Func<float, float, float> _alpha)
    {
        string path = $"{GeneratedArtFolder}/{_name}.png";
        if (!File.Exists(path))
        {
            EnsureFolder(GeneratedArtFolder);
            Texture2D texture = new Texture2D(_size, _size, TextureFormat.RGBA32, false);
            float radius = _size / 2f;
            Vector2 center = new Vector2(radius, radius);
            for (int y = 0; y < _size; y++)
            {
                for (int x = 0; x < _size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, _alpha(distance, radius)));
                }
            }
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static void EnsureFolder(string _path)
    {
        if (AssetDatabase.IsValidFolder(_path)) return;
        string parent = Path.GetDirectoryName(_path).Replace('\\', '/');
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, Path.GetFileName(_path));
    }

    #endregion
}
