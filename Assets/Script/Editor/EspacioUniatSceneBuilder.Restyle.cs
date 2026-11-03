using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//Aplica el estilo de Espacio UNIAT sobre las escenas 0 (registro) y 1 (quizz) sin tocar su logica ni sus eventos
//Busca los objetos por nombre, si se renombran hay que actualizar las rutas de aqui
public static partial class EspacioUniatSceneBuilder
{
    private const string Scene0Path = "Assets/Scenes/0.unity";
    private const string Scene1Path = "Assets/Scenes/1.unity";

    private static readonly Color AnswerYes = new(0.3f, 0.95f, 0.55f);
    private static readonly Color AnswerNo = new(1f, 0.38f, 0.5f);
    private static readonly Color ErrorColor = new(1f, 0.5f, 0.6f);
    private static readonly Color HorizonColor = new(0.33f, 0.22f, 0.75f);

    [MenuItem("Espacio UNIAT/4. Aplicar estilo a escenas 0 y 1", priority = 23)]
    private static void RestyleExistingScenesMenu()
    {
        RestyleExistingScenes();
    }

    private static bool RestyleExistingScenes()
    {
        if (!SaveOpenScenesIfNeeded()) return false;
        if (!autoMode && !EditorUtility.DisplayDialog("Espacio UNIAT",
                $"Se aplicará el estilo a {Scene0Path} y {Scene1Path}.\nSe guardará una copia de cada una en {BackupFolder}.",
                "Aplicar", "Cancelar"))
        {
            return false;
        }

        BackupScene(Scene0Path);
        BackupScene(Scene1Path);
        RestyleRegistryScene();
        RestyleQuizzScene();
        return true;
    }

    #region Escena 0: Intro y registro

    private static void RestyleRegistryScene()
    {
        Scene scene = EditorSceneManager.OpenScene(Scene0Path, OpenSceneMode.Single);
        Sprite circle = GetCircleSprite();
        Sprite glow = GetGlowSprite();
        Sprite rounded = GetRoundedSprite();

        StyleSceneBase(scene, glow, out Transform canvas);

        //Intro
        Transform intro = canvas.Find("Intro");
        Image logo = intro.Find("Image")?.GetComponent<Image>();
        //Si no hay logo asignado la imagen blanca se vuelve invisible, queda el espacio para poner el logo despues
        if (logo != null && logo.sprite == null) logo.color = Color.clear;

        Image horizonPlanet = EnsureImage(intro, "HorizonPlanet", circle, HorizonColor, new Vector2(0, -1350), new Vector2(2200, 2200));
        Image horizonGlow = EnsureImage(intro, "HorizonGlow", glow, new Color(0.54f, 0.36f, 1f, 0.55f), new Vector2(0, -1150), new Vector2(2900, 2900));
        horizonPlanet.transform.SetAsFirstSibling();
        horizonGlow.transform.SetAsFirstSibling();

        TextMeshProUGUI introTitle = EnsureText(intro, "Title", "ESPACIO UNIAT", 150, Color.white, new Vector2(0, 230), new Vector2(1800, 190));
        introTitle.fontStyle = FontStyles.Bold;
        introTitle.characterSpacing = 14;
        EnsureComponent<FloatIdle>(introTitle.gameObject).amplitude = 10f;
        TextMeshProUGUI introSubtitle = EnsureText(intro, "Subtitle", "Descubre el planeta hecho para ti", 44, Accent, new Vector2(0, 100), new Vector2(1600, 70));

        Button startBT = intro.Find("Button").GetComponent<Button>();
        StyleButton(startBT, "DESPEGAR", Accent, SpaceBackground, new Vector2(480, 140), new Vector2(0, -170), 44);
        SetupPopGroup(intro.gameObject, new[] { introTitle.transform, introSubtitle.transform, startBT.transform }, 0.12f, true);

        //Registro
        Transform registry = canvas.Find("RegistryPanel");
        UserRegistry userRegistry = registry.GetComponent<UserRegistry>();

        TextMeshProUGUI registryTitle = EnsureText(registry, "Title", "REGISTRO DE TRIPULANTE", 72, Color.white, new Vector2(0, 390), new Vector2(1300, 110));
        registryTitle.fontStyle = FontStyles.Bold;
        registryTitle.characterSpacing = 8;
        TextMeshProUGUI registrySubtitle = EnsureText(registry, "Subtitle", "Antes de despegar, cuéntanos quién eres", 36, Accent, new Vector2(0, 320), new Vector2(1600, 60));

        Transform nameField = registry.Find("NameField");
        Transform emailField = registry.Find("EmailField");
        StyleInputField(nameField, "NOMBRE", "Tu nombre", new Vector2(0, 150), rounded);
        StyleInputField(emailField, "CORREO", "tu@correo.com", new Vector2(0, -100), rounded);

        TextMeshProUGUI message = EnsureText(registry, "Message", "", 34, ErrorColor, new Vector2(0, -290), new Vector2(1400, 60));
        userRegistry.messageText = message;

        Button nextBT = registry.Find("BT_NEXT").GetComponent<Button>();
        StyleButton(nextBT, "CONTINUAR", Accent, SpaceBackground, new Vector2(420, 120), new Vector2(-260, 130));
        Button returnBT = registry.Find("BT_RETURN").GetComponent<Button>();
        StyleButton(returnBT, "VOLVER", PanelColor, Color.white, new Vector2(220, 84), new Vector2(150, -80), 32);

        userRegistry.errorFeedback = AddShakeFeedback(registry.gameObject, 28f, "Error Shake");
        EditorUtility.SetDirty(userRegistry);

        SetupPopGroup(registry.gameObject, new[] { registryTitle.transform, registrySubtitle.transform, nameField, emailField, nextBT.transform, returnBT.transform }, 0.07f, true);

        //Panel de carga del StaticMenu: azul espacial en vez de negro, el alpha lo controla el fade
        Image loadingPanel = FindInScene(scene, "StaticCanvas/LoadingPanel")?.GetComponent<Image>();
        if (loadingPanel != null)
        {
            loadingPanel.color = new Color(SpaceBackground.r, SpaceBackground.g, SpaceBackground.b, loadingPanel.color.a);
            EditorUtility.SetDirty(loadingPanel);
        }

        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[Espacio UNIAT] Estilo aplicado: {Scene0Path}");
    }

    private static void StyleInputField(Transform _field, string _label, string _placeholder, Vector2 _position, Sprite _rounded)
    {
        RectTransform fieldRect = (RectTransform)_field;
        fieldRect.anchoredPosition = _position;
        fieldRect.sizeDelta = new Vector2(980, 200);

        TMP_InputField input = _field.GetComponentInChildren<TMP_InputField>(true);
        RectTransform inputRect = (RectTransform)input.transform;
        inputRect.anchoredPosition = new Vector2(0, -30);
        inputRect.sizeDelta = new Vector2(980, 130);

        Image background = input.GetComponent<Image>();
        background.sprite = _rounded;
        background.type = Image.Type.Sliced;
        background.color = new Color(1f, 1f, 1f, 0.08f);
        EditorUtility.SetDirty(background);

        input.text = "";
        input.customCaretColor = true;
        input.caretColor = Accent;
        input.caretWidth = 3;
        input.selectionColor = new Color(Accent.r, Accent.g, Accent.b, 0.35f);
        if (input.textViewport != null)
        {
            input.textViewport.offsetMin = new Vector2(30, 8);
            input.textViewport.offsetMax = new Vector2(-30, -8);
        }

        if (input.textComponent is TextMeshProUGUI text)
        {
            StyleText(text, "", 54, Color.white);
            text.alignment = TextAlignmentOptions.MidlineLeft;
        }
        if (input.placeholder is TextMeshProUGUI placeholder)
        {
            StyleText(placeholder, _placeholder, 48, new Color(1f, 1f, 1f, 0.35f));
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            placeholder.fontStyle = FontStyles.Italic;
        }
        EditorUtility.SetDirty(input);

        //La etiqueta original ("Name" / "Email") es el TMP hijo directo del campo
        TextMeshProUGUI label = _field.Cast<Transform>()
            .Select(t => t.GetComponent<TextMeshProUGUI>())
            .FirstOrDefault(t => t != null);
        if (label == null) label = EnsureText(_field, "Label", _label, 32, Accent, Vector2.zero, Vector2.zero);
        StyleText(label, _label, 32, Accent);
        label.fontStyle = FontStyles.Bold;
        label.characterSpacing = 6;
        label.alignment = TextAlignmentOptions.MidlineLeft;
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = labelRect.anchorMax = new Vector2(0.5f, 0.5f);
        labelRect.anchoredPosition = new Vector2(0, 65);
        labelRect.sizeDelta = new Vector2(980, 50);
    }

    #endregion

    #region Escena 1: Quizz

    private static void RestyleQuizzScene()
    {
        Scene scene = EditorSceneManager.OpenScene(Scene1Path, OpenSceneMode.Single);
        Sprite glow = GetGlowSprite();
        Sprite rounded = GetRoundedSprite();

        StyleSceneBase(scene, glow, out Transform canvas);

        //Seleccion: iniciar prueba u omitir
        Transform selector = canvas.Find("OptionSelector");
        TextMeshProUGUI selectorTitle = EnsureText(selector, "Title", "¿LISTO PARA DESPEGAR?", 96, Color.white, new Vector2(0, 330), new Vector2(1700, 140));
        selectorTitle.fontStyle = FontStyles.Bold;
        selectorTitle.characterSpacing = 6;
        EnsureComponent<FloatIdle>(selectorTitle.gameObject).amplitude = 8f;
        TextMeshProUGUI selectorSubtitle = EnsureText(selector, "Subtitle", "Responde 5 preguntas y te diremos qué planeta es para ti", 38, Accent, new Vector2(0, 235), new Vector2(1600, 60));

        Button startBT = selector.Find("Button").GetComponent<Button>();
        StyleButton(startBT, "INICIAR PRUEBA", Accent, SpaceBackground, new Vector2(560, 140), new Vector2(0, 20), 44);
        Button skipBT = selector.Find("Button (1)").GetComponent<Button>();
        StyleButton(skipBT, "OMITIR", PanelColor, Color.white, new Vector2(360, 110), new Vector2(0, -170));
        SetupPopGroup(selector.gameObject, new[] { selectorTitle.transform, selectorSubtitle.transform, startBT.transform, skipBT.transform }, 0.1f, true);

        //Preguntas
        Transform quizz = canvas.Find("QuizzPanel");
        QuizzUI quizzUI = FindInScene(scene, "QuizzManager").GetComponent<QuizzUI>();

        TextMeshProUGUI progress = EnsureText(quizz, "Progress", "PREGUNTA 1 / 5", 34, Accent, new Vector2(0, 420), new Vector2(1000, 60));
        progress.fontStyle = FontStyles.Bold;
        progress.characterSpacing = 8;

        Transform box = quizz.Find("TXT_BOX");
        RectTransform boxRect = (RectTransform)box;
        boxRect.anchoredPosition = new Vector2(0, 150);
        boxRect.sizeDelta = new Vector2(1400, 440);
        Image boxImage = box.GetComponent<Image>();
        boxImage.sprite = rounded;
        boxImage.type = Image.Type.Sliced;
        boxImage.color = PanelColor;
        EditorUtility.SetDirty(boxImage);
        Outline outline = EnsureComponent<Outline>(box.gameObject);
        outline.effectColor = new Color(Accent.r, Accent.g, Accent.b, 0.35f);
        outline.effectDistance = new Vector2(2, -2);

        TextMeshProUGUI question = box.Find("QuestionTMPRO").GetComponent<TextMeshProUGUI>();
        StyleText(question, "", 62, Color.white);
        question.rectTransform.sizeDelta = new Vector2(1300, 400);

        Button yesBT = quizz.Find("TrueAnswer").GetComponent<Button>();
        StyleButton(yesBT, "SÍ", AnswerYes, SpaceBackground, new Vector2(420, 150), new Vector2(-260, -250), 64);
        Button noBT = quizz.Find("FalseAnswer").GetComponent<Button>();
        StyleButton(noBT, "NO", AnswerNo, SpaceBackground, new Vector2(420, 150), new Vector2(260, -250), 64);

        quizzUI.progressText = progress;
        quizzUI.questionPop = SetupPopGroup(box.gameObject, new[] { box }, 0f, false);
        EditorUtility.SetDirty(quizzUI);
        SetupPopGroup(quizz.gameObject, new[] { progress.transform, yesBT.transform, noBT.transform }, 0.08f, true);

        EditorSceneManager.SaveScene(scene);
        Debug.Log($"[Espacio UNIAT] Estilo aplicado: {Scene1Path}");
    }

    #endregion

    #region Helpers restyle

    //Camara, canvas scaler y fondo espacial
    private static void StyleSceneBase(Scene _scene, Sprite _glow, out Transform _canvas)
    {
        foreach (Camera camera in _scene.GetRootGameObjects().SelectMany(go => go.GetComponentsInChildren<Camera>(true)))
        {
            StyleCamera(camera);
        }

        _canvas = FindInScene(_scene, "Canvas");
        SetupCanvasScaler(_canvas.GetComponent<CanvasScaler>());
        if (_canvas.Find("SpaceBackground") == null)
        {
            CreateSpaceBackground(_canvas, _glow);
        }
    }

    private static Transform FindInScene(Scene _scene, string _path)
    {
        string[] parts = _path.Split(new[] { '/' }, 2);
        GameObject root = _scene.GetRootGameObjects().FirstOrDefault(go => go.name == parts[0]);
        if (root == null) return null;
        return parts.Length == 1 ? root.transform : root.transform.Find(parts[1]);
    }

    private static T EnsureComponent<T>(GameObject _go) where T : Component
    {
        T component = _go.GetComponent<T>();
        return component != null ? component : _go.AddComponent<T>();
    }

    private static TextMeshProUGUI EnsureText(Transform _parent, string _name, string _text, float _fontSize, Color _color, Vector2 _position, Vector2 _size)
    {
        Transform existing = _parent.Find(_name);
        TextMeshProUGUI text = existing != null ? existing.GetComponent<TextMeshProUGUI>() : null;
        if (text == null) return CreateText(_parent, _name, _text, _fontSize, _color, _position, _size);

        StyleText(text, _text, _fontSize, _color);
        text.rectTransform.anchoredPosition = _position;
        text.rectTransform.sizeDelta = _size;
        return text;
    }

    private static Image EnsureImage(Transform _parent, string _name, Sprite _sprite, Color _color, Vector2 _position, Vector2 _size)
    {
        Transform existing = _parent.Find(_name);
        Image image = existing != null ? existing.GetComponent<Image>() : null;
        if (image == null) return CreateImage(_parent, _name, _sprite, _color, _position, _size);

        image.sprite = _sprite;
        image.color = _color;
        image.rectTransform.anchoredPosition = _position;
        image.rectTransform.sizeDelta = _size;
        return image;
    }

    //Cambia solo el aspecto del boton, sus eventos OnClick se quedan igual
    private static void StyleButton(Button _button, string _label, Color _background, Color _textColor, Vector2 _size, Vector2 _position, float _fontSize = 38)
    {
        RectTransform rect = (RectTransform)_button.transform;
        rect.sizeDelta = _size;
        rect.anchoredPosition = _position;

        Image image = _button.targetGraphic as Image;
        if (image == null) image = _button.GetComponent<Image>();
        image.sprite = GetRoundedSprite();
        image.type = Image.Type.Sliced;
        image.color = _background;
        EditorUtility.SetDirty(image);

        if (_background.a < 0.5f) SetTranslucentButtonColors(_button);
        else SetSolidButtonColors(_button);

        TextMeshProUGUI label = _button.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label == null)
        {
            label = CreateText(_button.transform, "Label", _label, _fontSize, _textColor, Vector2.zero, Vector2.zero);
        }
        StyleText(label, _label, _fontSize, _textColor);
        label.fontStyle = FontStyles.Bold;
        label.characterSpacing = 4;
        Stretch(label.rectTransform);

        AddButtonFeel(_button);
        EditorUtility.SetDirty(_button);
    }

    #endregion
}
