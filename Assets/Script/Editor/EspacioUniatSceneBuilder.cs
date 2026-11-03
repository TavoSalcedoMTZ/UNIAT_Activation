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

//Construye las escenas de Espacio UNIAT (recomendacion y menu de planetas) desde el menu "Espacio UNIAT"
[InitializeOnLoad]
public static class EspacioUniatSceneBuilder
{
    //Si existe este archivo, al terminar de compilar se construye todo automaticamente una sola vez
    //Si su contenido es "buildsettings" solo actualiza Build Settings
    private const string AutoBuildMarker = "Temp/EspacioUniat_AutoBuild";
    private static bool autoMode;

    static EspacioUniatSceneBuilder()
    {
        EditorApplication.delayCall += RunPendingAutoBuild;
    }

    private static void RunPendingAutoBuild()
    {
        if (!File.Exists(AutoBuildMarker) || EditorApplication.isPlayingOrWillChangePlaymode) return;
        bool onlyBuildSettings = File.ReadAllText(AutoBuildMarker).Trim() == "buildsettings";
        File.Delete(AutoBuildMarker);

        autoMode = true;
        try
        {
            if (onlyBuildSettings) AddScenesToBuildSettings();
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

    private const string RecommendationScenePath = "Assets/Scenes/recomendation.unity";
    private const string PlanetMenuScenePath = "Assets/Scenes/3.unity";
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
        "Assets/Scenes/worlds/W_IPV.unity",
        "Assets/Scenes/worlds/W_LAV.unity",
        "Assets/Scenes/worlds/W_CAD.unity",
        "Assets/Scenes/worlds/W_CS.unity",
    };

    private static readonly Color SpaceBackground = new(0.027f, 0.04f, 0.12f);
    private static readonly Color NebulaPurple = new(0.54f, 0.36f, 1f, 0.22f);
    private static readonly Color NebulaCyan = new(0.31f, 0.82f, 1f, 0.14f);
    private static readonly Color Accent = new(0.31f, 0.82f, 1f);
    private static readonly Color Highlight = new(1f, 0.83f, 0.31f);
    private static readonly Color PanelColor = new(1f, 1f, 1f, 0.07f);
    private static readonly Color SoftWhite = new(1f, 1f, 1f, 0.75f);

    [MenuItem("Espacio UNIAT/Construir todo", priority = 0)]
    private static void BuildAll()
    {
        if (!BuildRecommendationScene())
        {
            Debug.LogWarning("[Espacio UNIAT] Construccion cancelada.");
            return;
        }
        if (!BuildPlanetMenuScene()) return;
        AddScenesToBuildSettings();
        EditorSceneManager.OpenScene(RecommendationScenePath);
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

    [MenuItem("Espacio UNIAT/3. Agregar escenas a Build Settings", priority = 22)]
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

    #region Recomendacion

    private static bool BuildRecommendationScene()
    {
        if (!PrepareScene(RecommendationScenePath)) return false;

        Sprite circle = GetCircleSprite();
        Sprite glow = GetGlowSprite();
        Sprite rounded = GetRoundedSprite();

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateCommonObjects();
        Canvas canvas = CreateCanvas("Canvas");
        CreateSpaceBackground(canvas.transform, glow);

        //Planeta (se crea antes que el texto para que quede detras)
        RectTransform planetGroup = CreateRect(canvas.transform, "PlanetGroup", new Vector2(0, 40), new Vector2(600, 600));
        Image planetGlow = CreateImage(planetGroup, "PlanetGlow", glow, new Color(1, 1, 1, 0.5f), Vector2.zero, new Vector2(560, 560));
        Image planetImage = CreateImage(planetGroup, "Planet", circle, Color.white, Vector2.zero, new Vector2(300, 300));
        TextMeshProUGUI careerText = CreateText(planetGroup, "CareerText", "Carrera", 42, Accent, new Vector2(0, -200), new Vector2(1400, 70));

        TextMeshProUGUI popText = CreateText(canvas.transform, "PopText", "NUESTRA", 120, Color.white, new Vector2(0, 370), new Vector2(1800, 170));
        popText.fontStyle = FontStyles.Bold;

        //Botones
        RectTransform buttons = CreateRect(canvas.transform, "Buttons", new Vector2(0, -350), new Vector2(1100, 120));
        CanvasGroup buttonsGroup = buttons.gameObject.AddComponent<CanvasGroup>();
        Button goToPlanetBT = CreateButton(buttons, "BT_MyPlanet", "IR A MI PLANETA", new Vector2(-265, 0), new Vector2(500, 110), rounded, Accent, SpaceBackground);
        Button otherPlanetsBT = CreateButton(buttons, "BT_OtherPlanets", "VER OTROS PLANETAS", new Vector2(265, 0), new Vector2(500, 110), rounded, PanelColor, Color.white);

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

        EditorSceneManager.SaveScene(scene, RecommendationScenePath);
        Debug.Log($"[Espacio UNIAT] Escena creada: {RecommendationScenePath}");
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
        ColorBlock colors = button.colors;
        //El fondo es casi transparente, al pasar el mouse o tocar se vuelve mas opaco
        colors.highlightedColor = new Color(1f, 1f, 1f, 2f);
        colors.pressedColor = new Color(1f, 1f, 1f, 3f);
        colors.selectedColor = Color.white;
        button.colors = colors;

        Image planetGlow = CreateImage(root.transform, "PlanetGlow", glow, new Color(1, 1, 1, 0.45f), new Vector2(0, 110), new Vector2(380, 380));
        Image planetImage = CreateImage(root.transform, "Planet", circle, Color.white, new Vector2(0, 110), new Vector2(210, 210));

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

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PlanetCardPrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    #endregion

    #region Feel

    private static MMF_Scale AddScaleFeedback(MMF_Player _player, Transform _target, float _duration, float _overshoot, string _label)
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
        scale.AnimateScaleTweenX = new MMTweenType(PopCurve(_overshoot), "AnimateX");
        scale.AnimateScaleTweenY = new MMTweenType(PopCurve(_overshoot), "AnimateY");
        scale.AnimateScaleTweenZ = new MMTweenType(PopCurve(_overshoot), "AnimateZ");
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

    //Crece desde 0, se pasa un poco (overshoot) y se asienta en 1, eso da el efecto POP
    private static AnimationCurve PopCurve(float _overshoot)
    {
        return new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.55f, _overshoot),
            new Keyframe(0.8f, 0.95f),
            new Keyframe(1f, 1f));
    }

    #endregion

    #region Objetos comunes

    //Guarda cambios pendientes y respalda la escena si ya existe
    private static bool PrepareScene(string _path)
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;

        if (File.Exists(_path))
        {
            bool rebuild = autoMode || EditorUtility.DisplayDialog("Espacio UNIAT",
                $"Se va a reconstruir {_path}.\nSe guardará una copia de la escena actual en {BackupFolder}.",
                "Reconstruir", "Cancelar");
            if (!rebuild) return false;

            EnsureFolder(BackupFolder);
            string backupPath = $"{BackupFolder}/{Path.GetFileNameWithoutExtension(_path)}_{DateTime.Now:yyyyMMdd_HHmmss}.unity";
            AssetDatabase.CopyAsset(_path, backupPath);
        }
        return true;
    }

    private static void CreateCommonObjects()
    {
        GameObject cameraGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraGO.tag = "MainCamera";
        cameraGO.transform.position = new Vector3(0, 0, -10);
        Camera camera = cameraGO.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = SpaceBackground;

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

    private static Canvas CreateCanvas(string _name)
    {
        GameObject go = new GameObject(_name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = go.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = go.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        return canvas;
    }

    private static void CreateSpaceBackground(Transform _canvas, Sprite _glow)
    {
        Image background = CreateImage(_canvas, "Background", null, SpaceBackground, Vector2.zero, Vector2.zero);
        Stretch(background.rectTransform);

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
        if (TMP_Settings.defaultFontAsset != null) text.font = TMP_Settings.defaultFontAsset;
        text.text = _text;
        text.fontSize = _fontSize;
        text.color = _color;
        text.alignment = TextAlignmentOptions.Center;
        text.raycastTarget = false;
        return text;
    }

    private static Button CreateButton(Transform _parent, string _name, string _label, Vector2 _position, Vector2 _size, Sprite _sprite, Color _background, Color _textColor)
    {
        Image image = CreateImage(_parent, _name, _sprite, _background, _position, _size, true);
        image.type = Image.Type.Sliced;
        Button button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        TextMeshProUGUI label = CreateText(image.transform, "Label", _label, 36, _textColor, Vector2.zero, _size);
        label.fontStyle = FontStyles.Bold;
        return button;
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
