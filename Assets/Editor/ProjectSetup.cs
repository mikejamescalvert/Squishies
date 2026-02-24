using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEditor.SceneManagement;
using TMPro;
using System.IO;

namespace Squishies.Editor
{
    public static class ProjectSetup
    {
        [MenuItem("Squishies/Setup Project (Create Scenes)")]
        public static void SetupProject()
        {
            if (!EditorUtility.DisplayDialog("Squishies Project Setup",
                "This will create the Game, MainMenu, and Workshop scenes.\n\nContinue?",
                "Yes", "Cancel"))
                return;

            ConfigurePlayerSettings();
            CreateGameScene();
            CreateMainMenuScene();
            CreateWorkshopScene();
            SetupBuildSettings();

            EditorUtility.DisplayDialog("Setup Complete",
                "Squishies project setup complete!\n\n" +
                "1. Open Assets/Scenes/Game.unity\n" +
                "2. Press Play to test!\n\n" +
                "Or open MainMenu scene for the full flow.",
                "OK");
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.productName = "Squishies";
            PlayerSettings.companyName = "Squishies";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

#if UNITY_IOS
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            PlayerSettings.iOS.requiresPersistentWiFi = false;
#endif

            // Default resolution for editor testing
            PlayerSettings.defaultScreenWidth = 540;
            PlayerSettings.defaultScreenHeight = 960;
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;

            AssetDatabase.SaveAssets();
        }

        private static void CreateGameScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── Main Camera ──
            var cameraGO = new GameObject("Main Camera");
            var cam = cameraGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.94f, 0.92f, 0.98f, 1f); // Soft pastel lavender
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.tag = "MainCamera";
            cameraGO.AddComponent<AudioListener>();
            cameraGO.AddComponent<CameraEffects>();

            // ── Managers ──
            var managers = new GameObject("Managers");

            CreateManagerChild<GameManager>(managers, "_GameManager");
            CreateManagerChild<GridManager>(managers, "_GridManager");
            CreateManagerChild<MatchEngine>(managers, "_MatchEngine");
            CreateManagerChild<ComboSystem>(managers, "_ComboSystem");
            CreateManagerChild<ScoreManager>(managers, "_ScoreManager");
            CreateManagerChild<MoodSystem>(managers, "_MoodSystem");
            CreateManagerChild<AbilitySystem>(managers, "_AbilitySystem");
            CreateManagerChild<JuiceManager>(managers, "_JuiceManager");
            CreateManagerChild<ParticleManager>(managers, "_ParticleManager");
            CreateManagerChild<AudioManager>(managers, "_AudioManager");

            // ── Input System ──
            var inputSystem = new GameObject("InputSystem");

            var inputHandler = new GameObject("_InputHandler");
            inputHandler.transform.SetParent(inputSystem.transform);
            inputHandler.AddComponent<InputHandler>();
            inputHandler.AddComponent<GridInputMapper>();

            var pathDrawer = new GameObject("_PathDrawer");
            pathDrawer.transform.SetParent(inputSystem.transform);
            pathDrawer.AddComponent<PathDrawer>();

            // ── Grid Parent ──
            new GameObject("Grid");

            // ── UI Canvas ──
            var canvas = CreateUICanvas("Canvas");

            // HUD
            var hud = CreateUIPanel(canvas.transform, "HUD", false);
            hud.AddComponent<HUDController>();

            var scoreText = CreateTMPText(hud.transform, "ScoreText", "Score: 0",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, -10), new Vector2(400, 50), 28, TextAlignmentOptions.Center);

            var bestScoreText = CreateTMPText(hud.transform, "BestScoreText", "Best: 0",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, -55), new Vector2(400, 35), 20, TextAlignmentOptions.Center);
            bestScoreText.color = new Color(0.6f, 0.6f, 0.6f);

            var comboText = CreateTMPText(hud.transform, "ComboText", "Nice!",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(400, 80), 48, TextAlignmentOptions.Center);
            comboText.color = new Color(1f, 0.85f, 0.2f);
            comboText.gameObject.SetActive(false);

            var timerText = CreateTMPText(hud.transform, "TimerText", "90s",
                new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f),
                new Vector2(-20, -15), new Vector2(120, 50), 32, TextAlignmentOptions.Right);
            timerText.gameObject.SetActive(false);

            // Game Over Panel
            var gameOverPanel = CreateUIPanel(canvas.transform, "GameOverPanel", true);
            var goCanvasGroup = gameOverPanel.AddComponent<CanvasGroup>();
            goCanvasGroup.alpha = 0;
            goCanvasGroup.interactable = false;
            goCanvasGroup.blocksRaycasts = false;
            gameOverPanel.AddComponent<GameOverPanel>();

            // Semi-transparent background
            var goBg = gameOverPanel.AddComponent<Image>();
            goBg.color = new Color(0, 0, 0, 0.6f);

            CreateTMPText(gameOverPanel.transform, "GameOverTitle", "Game Over!",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, -80), new Vector2(400, 60), 42, TextAlignmentOptions.Center);

            CreateTMPText(gameOverPanel.transform, "FinalScoreText", "Score: 0",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 60), new Vector2(400, 50), 36, TextAlignmentOptions.Center);

            CreateTMPText(gameOverPanel.transform, "BestScoreLabel", "Best: 0",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 10), new Vector2(400, 40), 24, TextAlignmentOptions.Center);

            var newBest = CreateTMPText(gameOverPanel.transform, "NewBestLabel", "New Best!",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -30), new Vector2(300, 40), 28, TextAlignmentOptions.Center);
            newBest.color = new Color(1f, 0.85f, 0.2f);
            newBest.gameObject.SetActive(false);

            CreateUIButton(gameOverPanel.transform, "PlayAgainButton", "Play Again",
                new Vector2(0.5f, 0.5f), new Vector2(0, -100), new Vector2(220, 55),
                new Color(0.4f, 0.8f, 0.4f));

            CreateUIButton(gameOverPanel.transform, "MenuButton", "Menu",
                new Vector2(0.5f, 0.5f), new Vector2(0, -170), new Vector2(220, 55),
                new Color(0.5f, 0.6f, 0.9f));

            // Main Menu Panel (overlay in same scene)
            var menuPanel = CreateUIPanel(canvas.transform, "MainMenuPanel", true);
            var menuCanvasGroup = menuPanel.AddComponent<CanvasGroup>();
            menuPanel.AddComponent<MainMenuController>();

            var menuBg = menuPanel.AddComponent<Image>();
            menuBg.color = new Color(0.94f, 0.92f, 0.98f, 1f);

            var title = CreateTMPText(menuPanel.transform, "TitleText", "SQUISHIES",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, -120), new Vector2(500, 100), 64, TextAlignmentOptions.Center);
            title.color = new Color(0.9f, 0.45f, 0.6f);
            title.fontStyle = FontStyles.Bold;

            CreateTMPText(menuPanel.transform, "ZenBestText", "Best: 0",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, 30), new Vector2(300, 30), 18, TextAlignmentOptions.Center);

            CreateUIButton(menuPanel.transform, "ZenButton", "Zen Mode",
                new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(260, 60),
                new Color(0.55f, 0.85f, 0.65f));

            CreateTMPText(menuPanel.transform, "RushBestText", "Best: 0",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(0, -80), new Vector2(300, 30), 18, TextAlignmentOptions.Center);

            CreateUIButton(menuPanel.transform, "RushButton", "Rush Mode",
                new Vector2(0.5f, 0.5f), new Vector2(0, -130), new Vector2(260, 60),
                new Color(0.9f, 0.55f, 0.55f));

            // EventSystem
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            // Save
            string scenePath = "Assets/Scenes/Game.unity";
            EnsureDirectory(scenePath);
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log("Created Game scene at " + scenePath);
        }

        private static void CreateMainMenuScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            var cameraGO = new GameObject("Main Camera");
            var cam = cameraGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.94f, 0.92f, 0.98f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.tag = "MainCamera";
            cameraGO.AddComponent<AudioListener>();

            // Canvas
            var canvas = CreateUICanvas("Canvas");

            var title = CreateTMPText(canvas.transform, "TitleText", "SQUISHIES",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, -150), new Vector2(500, 100), 64, TextAlignmentOptions.Center);
            title.color = new Color(0.9f, 0.45f, 0.6f);
            title.fontStyle = FontStyles.Bold;

            CreateUIButton(canvas.transform, "ZenButton", "Zen Mode",
                new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(260, 60),
                new Color(0.55f, 0.85f, 0.65f));

            CreateUIButton(canvas.transform, "RushButton", "Rush Mode",
                new Vector2(0.5f, 0.5f), new Vector2(0, -80), new Vector2(260, 60),
                new Color(0.9f, 0.55f, 0.55f));

            // EventSystem
            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            string scenePath = "Assets/Scenes/MainMenu.unity";
            EnsureDirectory(scenePath);
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log("Created MainMenu scene at " + scenePath);
        }

        private static void CreateWorkshopScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var cameraGO = new GameObject("Main Camera");
            var cam = cameraGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.backgroundColor = new Color(0.94f, 0.92f, 0.98f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.tag = "MainCamera";
            cameraGO.AddComponent<AudioListener>();

            var canvas = CreateUICanvas("Canvas");

            var title = CreateTMPText(canvas.transform, "TitleText", "Workshop",
                new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0.5f, 1f),
                new Vector2(0, -150), new Vector2(500, 80), 48, TextAlignmentOptions.Center);
            title.color = new Color(0.9f, 0.45f, 0.6f);

            var comingSoon = CreateTMPText(canvas.transform, "ComingSoonText", "Coming Soon!",
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                Vector2.zero, new Vector2(400, 60), 36, TextAlignmentOptions.Center);
            comingSoon.color = new Color(0.6f, 0.6f, 0.7f);

            CreateUIButton(canvas.transform, "BackButton", "Back to Menu",
                new Vector2(0.5f, 0f), new Vector2(0, 80), new Vector2(220, 55),
                new Color(0.5f, 0.6f, 0.9f));

            var eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();

            string scenePath = "Assets/Scenes/Workshop.unity";
            EnsureDirectory(scenePath);
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log("Created Workshop scene at " + scenePath);
        }

        private static void SetupBuildSettings()
        {
            var scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Workshop.unity", true),
            };
            EditorBuildSettings.scenes = scenes;
            Debug.Log("Build settings updated with 3 scenes.");
        }

        // ── Helper Methods ──

        private static GameObject CreateManagerChild<T>(GameObject parent, string name) where T : Component
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent.transform);
            go.AddComponent<T>();
            return go;
        }

        private static GameObject CreateUICanvas(string name)
        {
            var canvasGO = new GameObject(name);
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasGO.AddComponent<GraphicRaycaster>();
            return canvasGO;
        }

        private static GameObject CreateUIPanel(Transform parent, string name, bool fullScreen)
        {
            var panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            var rt = panel.AddComponent<RectTransform>();

            if (fullScreen)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;
            }
            else
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.sizeDelta = Vector2.zero;
                rt.anchoredPosition = Vector2.zero;
            }

            return panel;
        }

        private static TextMeshProUGUI CreateTMPText(Transform parent, string name, string text,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot,
            Vector2 anchoredPos, Vector2 size, int fontSize,
            TextAlignmentOptions alignment)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.alignment = alignment;
            tmp.color = Color.white;
            tmp.enableAutoSizing = false;

            return tmp;
        }

        private static Button CreateUIButton(Transform parent, string name, string label,
            Vector2 anchor, Vector2 anchoredPos, Vector2 size, Color bgColor)
        {
            var btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent, false);

            var rt = btnGO.AddComponent<RectTransform>();
            rt.anchorMin = anchor;
            rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;

            var image = btnGO.AddComponent<Image>();
            image.color = bgColor;
            // Round the corners via sprite — just use default for now
            image.type = Image.Type.Sliced;

            var btn = btnGO.AddComponent<Button>();
            btn.targetGraphic = image;

            // Button label
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(btnGO.transform, false);

            var labelRT = labelGO.AddComponent<RectTransform>();
            labelRT.anchorMin = Vector2.zero;
            labelRT.anchorMax = Vector2.one;
            labelRT.sizeDelta = Vector2.zero;
            labelRT.anchoredPosition = Vector2.zero;

            var tmp = labelGO.AddComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 26;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.fontStyle = FontStyles.Bold;

            return btn;
        }

        private static void EnsureDirectory(string assetPath)
        {
            string dir = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
