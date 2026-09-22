using System;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GLTFast;
using Yvonta;

public class Main : MonoBehaviour
{
    [Header("Server Endpoints")]
    [SerializeField] private string jsonRpcUrl = "https://yvonta.com/appapi/v2/xbot.php";
    [SerializeField] private string avatarGenUrl = "https://yvonta.com/appapi/v2/avatargen.php";
    [SerializeField] private string clothingUrl = "https://yvonta.com/appapi/v2/clothing.php";
    [SerializeField] private string hairUrl = "https://yvonta.com/appapi/v2/hair.php";
    [SerializeField] private string sttUrl = "https://yvonta.com/appapi/v2/stt.php";

#pragma warning disable 0414
    [SerializeField] private string ttsUrl = "https://yvonta.com/appapi/v2/tts.php";
#pragma warning restore 0414

    [SerializeField] private string llmUrl = "https://yvonta.com/appapi/v2/llm.php";

    [SerializeField] private string voiceCloningUrl = "https://yvonta.com/appapi/v2/voicecloning.php";

    [Header("UI References")]
    [SerializeField] private UILogin uiLogin;
    [SerializeField] private UIRegister uiRegister;

    [Header("Avatar Generation Parameters")]
    [SerializeField] private string faceImagePath = "Assets/Faces/dirkjan.jpg";
    [SerializeField] private float gender = 1.0f;
    [SerializeField] private float age = 0.8f;
    [SerializeField] private float weight = 0.2f;

    [Header("Customization Assets")]
    [SerializeField] private string clothingName = "green_tomato_rei_ayanami";
    [SerializeField] private string hairName = "o4saken_long01";

    [Header("Build Shader Fix")]
    [Tooltip("Drag Universal Render Pipeline/Lit or Standard shader here in the Inspector")]
    [SerializeField] private Shader fallbackShader;

    private EgoLinkAvatar _player;

    private EgoLinkSession session;
    private AudioMic audioMic;

    private EgoLinkTTSStreaming ttsStreamer;
    private UiVoiceCloning uiVoiceCloning;
    private UITextToSpeechDialog uiTextToSpeechDialog;

    private UserStatsResult _userstats;

    private void Update()
    {
        audioMic?.Update();
    }

    private void HandleWavData(AudioClip myAudioClip)
    {
        Debug.Log($"Received AudioClip '{myAudioClip.name}' (Length: {myAudioClip.length:F2}s, Frequency: {myAudioClip.frequency}Hz) via callback.");

        if (uiTextToSpeechDialog != null)
        {
            uiTextToSpeechDialog.SetStatusMessage("Transcribing audio...");
        }

        EgoLinkSTT client = new EgoLinkSTT(
            session,
            sttUrl,
            "auto",
            "large",
            280,
            this
        );

        client.SendAudioClipForTranscription(
            myAudioClip,
            onSuccess: (text) => {
                Debug.Log($"Transcribed: {text}");
                if (uiTextToSpeechDialog != null)
                {
                    uiTextToSpeechDialog.SetStatusMessage(string.Empty);
                }
                SendTextToLLM(text);
            },
            onError: (err) => {
                Debug.LogError($"STT Error: {err}");
                if (uiTextToSpeechDialog != null)
                {
                    uiTextToSpeechDialog.SetStatusMessage($"STT Error: {err}");
                }
            }
        );
    }

    private void SendTextToLLM(string text)
    {
        if (!string.IsNullOrEmpty(llmUrl))
        {
            try
            {
                EgoLinkLLMStreaming llmStreamer = GetComponent<EgoLinkLLMStreaming>();
                if (llmStreamer == null)
                {
                    llmStreamer = gameObject.AddComponent<EgoLinkLLMStreaming>();
                }
                llmStreamer.SetSystemPrompt("Play the role of a fruit fly!");

                llmStreamer.RequestStream(
                    text,
                    ttsStreamer.AddSentence,
                    llmUrl
                );
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[EgoLinkCompletions] Failed to send prompt: {ex.Message}");
            }
        }
    }

    private void Awake()
    {
        ttsStreamer = GetComponent<EgoLinkTTSStreaming>();
        if (ttsStreamer == null)
        {
            ttsStreamer = gameObject.AddComponent<EgoLinkTTSStreaming>();
        }

        uiTextToSpeechDialog = GetComponent<UITextToSpeechDialog>();
        if (uiTextToSpeechDialog == null)
        {
            uiTextToSpeechDialog = gameObject.AddComponent<UITextToSpeechDialog>();
        }

        EgoLinkSubtitles.Initialize();

        GameObject canvasObj = GameObject.Find("GeneratedCanvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("GeneratedCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>();
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        if (uiLogin == null)
        {
            uiLogin = canvasObj.GetComponent<UILogin>();
            if (uiLogin == null)
            {
                uiLogin = canvasObj.AddComponent<UILogin>();
                uiLogin.BuildUI(canvasObj.transform);
            }
        }

        if (uiRegister == null)
        {
            uiRegister = canvasObj.GetComponent<UIRegister>();
            if (uiRegister == null)
            {
                uiRegister = canvasObj.AddComponent<UIRegister>();
                uiRegister.BuildUI(canvasObj.transform);
            }
        }

        // Configure configurable options/buttons here
        var options = new List<UISettingsDialog.SettingsOption>
        {
            new UISettingsDialog.SettingsOption("Clone Voice", () => {
                Debug.Log("Clone Voice Clicked");
                
                if (uiVoiceCloning == null)
                {
                    uiVoiceCloning = canvasObj.GetComponent<UiVoiceCloning>();
                    if (uiVoiceCloning == null)
                    {
                        uiVoiceCloning = canvasObj.AddComponent<UiVoiceCloning>();
                        uiVoiceCloning.BuildUI(canvasObj.transform);
                    }
                }

                // Subscribe to recording event and ensure dialog visibility
                uiVoiceCloning.OnAudioRecorded -= HandleVoiceCloningRecorded;
                uiVoiceCloning.OnAudioRecorded += HandleVoiceCloningRecorded;
                uiVoiceCloning.SetVisible(true);
            }),
            new UISettingsDialog.SettingsOption("Clone Avatar", () => {
                Debug.Log("Clone Avatar Clicked");
            }),
            new UISettingsDialog.SettingsOption("Clone Persona", () => {
                Debug.Log("Clone Presona Clicked");
            })
        };


        uiTextToSpeechDialog.BuildUI(canvasObj.transform, options);
        uiTextToSpeechDialog.SetVisible(false);

        if (uiLogin != null) uiLogin.SetVisible(true);
        if (uiRegister != null) uiRegister.SetVisible(false);

        audioMic = new AudioMic(
            deviceName: null,
            sampleRate: 44100,
            maxRecordingLengthSeconds: 120,
            onAudioRecorded: HandleWavData
        );
    }

    private async void Start()
    {        
        bool islogin = false;

        if (uiLogin != null)
        {
            uiLogin.SetVisible(false);
            uiLogin.SetInteractable(false);
            uiLogin.SetStatusMessage("Checking existing session...");
        }

        this.session = new EgoLinkSession(jsonRpcUrl);

        _userstats = await session.UserStatsAsync();
        Debug.Log("Users online: " + _userstats.data.usersonline);
        Debug.Log("Users total: " + _userstats.data.userstotal);

        if (!string.IsNullOrEmpty(session.StoredCookie))
        {
            try
            {
                Debug.Log("Validating existing session...");
                islogin = await session.IsLoggedInAsync();
                
                if (islogin)
                {
                    Debug.Log("Session is valid! Running avatar workflow.");
                    ttsStreamer.Initialize(this.session, 0.1f, 0.5f);
                    
                    if (uiLogin != null)
                    {                        
                        uiLogin.SetVisible(false);
                    }

                    await RunAvatarWorkflow(session);
                    return;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"Stored session is invalid or expired: {ex.Message}. Requiring manual login.");
                session.ClearSession();
            }
        }

        if (uiLogin != null)
        {
            uiLogin.SetVisible(true);
            uiLogin.SetInteractable(true);
            uiLogin.SetStatusMessage("Please log in.");
        }
    }

    private async void Stop()
    {
        await session.LogoutAsync();
    }

    private void OnEnable()
    {
        if (uiTextToSpeechDialog != null)
        {
            uiTextToSpeechDialog.OnTextSubmitted += HandleTTSDialogTextSubmitted;
            uiTextToSpeechDialog.OnVoiceInputStarted += HandleTTSVoiceStarted;
            uiTextToSpeechDialog.OnVoiceInputStopped += HandleTTSVoiceStopped;
        }

        if (uiLogin != null)
        {
            uiLogin.OnLoginSubmitted.AddListener(HandleLoginSubmitted);
            uiLogin.OnRegisterClicked.AddListener(HandleShowRegisterClicked);
        }

        if (uiRegister != null)
        {
            uiRegister.OnRegisterSubmitted += HandleRegisterSubmitted;
            uiRegister.OnBackClicked += HandleBackToLoginClicked;
        }

        if (uiVoiceCloning != null)
        {
            uiVoiceCloning.OnAudioRecorded += HandleVoiceCloningRecorded;
        }
    }

    private void OnDisable()
    {
        if (uiTextToSpeechDialog != null)
        {
            uiTextToSpeechDialog.OnTextSubmitted -= HandleTTSDialogTextSubmitted;
            uiTextToSpeechDialog.OnVoiceInputStarted -= HandleTTSVoiceStarted;
            uiTextToSpeechDialog.OnVoiceInputStopped -= HandleTTSVoiceStopped;
        }

        if (uiLogin != null)
        {
            uiLogin.OnLoginSubmitted.RemoveListener(HandleLoginSubmitted);
            uiLogin.OnRegisterClicked.RemoveListener(HandleShowRegisterClicked);
        }

        if (uiRegister != null)
        {
            uiRegister.OnRegisterSubmitted -= HandleRegisterSubmitted;
            uiRegister.OnBackClicked -= HandleBackToLoginClicked;
        }

        if (uiVoiceCloning != null)
        {
            uiVoiceCloning.OnAudioRecorded -= HandleVoiceCloningRecorded;
        }
    }

    private async void HandleVoiceCloningRecorded(byte[] audioBytes, string targetSentence, string voiceName)
    {
        if (uiVoiceCloning != null)
        {
            uiVoiceCloning.UpdateButtonText("Processing...");
            uiVoiceCloning.SetUploadButtonInteractable(false);
        }

        EgoLinkVoiceCloning cloner = new EgoLinkVoiceCloning();
        cloner.Initialize(session, voiceCloningUrl);

        string response = await cloner.CloneVoiceAsync(audioBytes, "audio/wav", voiceName, "en");

        if (!string.IsNullOrEmpty(response))
        {
            Debug.Log($"[Main] Voice cloned successfully: {response}");
        }
        else
        {
            Debug.LogError("[Main] Voice cloning failed.");
        }

        // Close the dialog after API processing finishes
        if (uiVoiceCloning != null)
        {
            uiVoiceCloning.SetVisible(false);
            uiVoiceCloning.SetUploadButtonInteractable(true);
            uiVoiceCloning.UpdateButtonText("Start Recording");
            ttsStreamer.SetVoice(voiceName);
        }
    }

    private void HandleTTSDialogTextSubmitted(string text)
    {
        Debug.Log($"[Main] Text submitted from TTS Dialog: {text}");
        SendTextToLLM(text);

        if (uiTextToSpeechDialog != null)
        {
            uiTextToSpeechDialog.ClearInput();
        }
    }

    private void HandleTTSVoiceStarted()
    {
        if (audioMic != null)
        {
            audioMic.StartRecording();
        }
    }

    private void HandleTTSVoiceStopped()
    {
        if (audioMic != null)
        {
            audioMic.StopAndProcessRecording();
        }
    }

    private void HandleShowRegisterClicked()
    {
        if (uiLogin != null) uiLogin.SetVisible(false);
        if (uiRegister != null) uiRegister.SetVisible(true);
    }

    private void HandleBackToLoginClicked()
    {
        if (uiRegister != null) uiRegister.SetVisible(false);
        if (uiLogin != null) uiLogin.SetVisible(true);
    }

    private async void HandleRegisterSubmitted(string email, string password, string name, string genderInput, string birthdate)
    {
        Debug.Log($"Register submitted for: {email}, Name: {name}");

        EgoLinkSession newSession = new EgoLinkSession(jsonRpcUrl);

        string gender = "m";
        switch(genderInput)
        {
            case "Male": gender = "m"; break;
            case "Female": gender = "f"; break;
            case "Non-binair": gender = "x"; break;
        }

        try
        {
            await newSession.RegisterAsync(email, password, name, gender, birthdate);
            uiRegister.SetVisible(false);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Registration failed: {ex.Message}");
        }
    }

    private async void HandleLoginSubmitted(string email, string password)
    {
        if (uiLogin != null)
        {
            uiLogin.SetInteractable(false);
            uiLogin.SetStatusMessage("Logging in via JSON-RPC...");
        }

        if (this.session == null)
        {
            this.session = new EgoLinkSession(jsonRpcUrl);
        }

        try
        {
            Debug.Log("Logging in via JSON-RPC...");
            await this.session.LoginAsync(email, password);
            Debug.Log("Login successful! Session stored.");
            ttsStreamer.Initialize(this.session, 0.1f, 0.5f);
            if (uiLogin != null) uiLogin.SetVisible(false);

            await RunAvatarWorkflow(this.session);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"An error occurred during the EgoLink workflow: {ex.Message}");
            if (uiLogin != null)
            {
                uiLogin.SetVisible(true);
                uiLogin.SetStatusMessage($"Error: {ex.Message}");
                uiLogin.SetInteractable(true);
            }
        }
    }
    
    private async Task RunAvatarWorkflow(EgoLinkSession session)
    {
        if (uiTextToSpeechDialog != null)
        {
            uiTextToSpeechDialog.gameObject.SetActive(true);
            uiTextToSpeechDialog.SetVisible(true);
        }

        _player = await LoadAndInitializeAvatar(
            session,
            0f, 0f, 0f,
            gender,
            faceImagePath,
            clothingName,
            hairName,
            Vector3.zero
        );
    }

    private async Task<EgoLinkAvatar> LoadAndInitializeAvatar(EgoLinkSession session, float x, float y, float z, float avatarGender, string avatarFaceImagePath, string targetClothing, string targetHair, Vector3 accessoryPositionOffset)
    {
        EgoLinkAvatar avatar = new EgoLinkAvatar(avatarGenUrl, clothingUrl, hairUrl, session);

        if (avatar.TryLoadFromCache(targetClothing, targetHair, avatarGender, age, weight))
        {
            Debug.Log($"[Workflow] Loaded avatar setup from cache for position ({x},{y},{z}).");
        }
        else
        {
            string fullPath = Path.Combine(Application.streamingAssetsPath, avatarFaceImagePath);
            if (!File.Exists(fullPath))
            {
                fullPath = avatarFaceImagePath;
            }

            if (!File.Exists(fullPath))
            {
                Debug.LogError($"Face image file not found at: {fullPath}");
                return null;
            }

            byte[] imageBytes = File.ReadAllBytes(fullPath);

            Debug.Log($"Generating avatar via proxy for position ({x},{y},{z})...");
            await avatar.GenerateAvatarAsync(imageBytes, avatarGender, age, weight);
            
            Debug.Log($"Fitting clothing: {targetClothing}...");
            await avatar.FitClothingAsync(targetClothing);

            Debug.Log($"Fitting hair: {targetHair}...");
            await avatar.FitHairAsync(targetHair);
        }

        GameObject avatarObject = null;
        Transform mainArmatureRoot = null;

        if (avatar.AvatarGlbData != null)
        {
            Debug.Log($"Instantiating avatar at position ({x}, {y}, {z})...");

            Quaternion spawnRotation = Quaternion.Euler(0f, 180f, 0f);
            Vector3 spawnPosition = new Vector3(x, y, z);

            avatarObject = new GameObject($"EgoLinkAvatar_{avatar.AvatarId}");
            avatarObject.transform.position = spawnPosition;
            avatarObject.transform.rotation = spawnRotation;

            var gltfImport = new GltfImport();
            if (await gltfImport.Load(avatar.AvatarGlbData))
            {
                await gltfImport.InstantiateMainSceneAsync(avatarObject.transform);
                mainArmatureRoot = FindDeepChild(avatarObject.transform, "Hips") ?? avatarObject.transform;
            }

            async Task MergeAccessoryIntoAvatar(byte[] glbData)
            {
                if (glbData == null || mainArmatureRoot == null) return;

                var accImport = new GltfImport();
                if (await accImport.Load(glbData))
                {
                    var tempAccObj = new GameObject("TempAccessory");
                    await accImport.InstantiateMainSceneAsync(tempAccObj.transform);

                    SkinnedMeshRenderer[] accSmrs = tempAccObj.GetComponentsInChildren<SkinnedMeshRenderer>();
                    foreach (var accSmr in accSmrs)
                    {
                        accSmr.transform.SetParent(avatarObject.transform, true);
                        accSmr.transform.localPosition = accessoryPositionOffset;
                        accSmr.transform.localRotation = Quaternion.identity;
                        accSmr.transform.localScale = Vector3.one;

                        Transform[] mainBones = avatarObject.GetComponentsInChildren<Transform>();
                        Transform[] newBones = new Transform[accSmr.bones.Length];
                        
                        for (int i = 0; i < accSmr.bones.Length; i++)
                        {
                            if (accSmr.bones[i] != null)
                            {
                                foreach (var b in mainBones)
                                {
                                    if (b.name.Equals(accSmr.bones[i].name, System.StringComparison.OrdinalIgnoreCase))
                                    {
                                        newBones[i] = b;
                                        break;
                                    }
                                }
                                if (newBones[i] == null) newBones[i] = accSmr.bones[i];
                            }
                        }

                        accSmr.bones = newBones;
                        if (accSmr.rootBone != null)
                        {
                            foreach (var b in mainBones)
                            {
                                if (b.name.Equals(accSmr.rootBone.name, System.StringComparison.OrdinalIgnoreCase))
                                {
                                    accSmr.rootBone = b;
                                    break;
                                }
                            }
                        }
                    }

                    MeshRenderer[] accMrs = tempAccObj.GetComponentsInChildren<MeshRenderer>();
                    foreach (var mr in accMrs)
                    {
                        mr.transform.SetParent(avatarObject.transform, true);
                        mr.transform.localPosition = accessoryPositionOffset;
                        mr.transform.localRotation = Quaternion.identity;
                        mr.transform.localScale = Vector3.one;
                    }

                    if (tempAccObj != null)
                    {
                        Destroy(tempAccObj);
                    }
                }
            }

            foreach (var clothingItem in avatar.ClothingItems)
            {
                await MergeAccessoryIntoAvatar(clothingItem.GlbData);
            }

            if (avatar.HairGlbData != null)
            {
                await MergeAccessoryIntoAvatar(avatar.HairGlbData);
            }
        }

        if (avatarObject != null)
        {
            FixShaderOnLoadedModel(avatarObject);
            Debug.Log($"[Main] Avatar '{avatarObject.name}' successfully loaded at ({x}, {y}, {z}).");
        }

        if (uiLogin != null)
        {
            uiLogin.SetStatusMessage("Avatar loaded successfully!");
            uiLogin.SetInteractable(true);
        }

        return avatar;
    }

    private void FixShaderOnLoadedModel(GameObject loadedModel)
    {
        if (loadedModel == null) return;

        // 1. Try to grab the target shader from fallbackShader first
        Shader targetShader = fallbackShader;

        // 2. Fallback to Unity's default primitive material shader if null
        if (targetShader == null)
        {
            GameObject tempCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            targetShader = tempCube.GetComponent<Renderer>().sharedMaterial.shader;
            Destroy(tempCube);
        }

        if (targetShader == null)
        {
            Debug.LogError("[Main] Failed to acquire a valid render shader for build!");
            return;
        }

        Renderer[] renderers = loadedModel.GetComponentsInChildren<Renderer>(true);
        foreach (Renderer rend in renderers)
        {
            rend.enabled = true;

            foreach (Material mat in rend.materials)
            {
                if (mat == null) continue;

                // Save all textures before assigning shader
                Texture baseTex = mat.mainTexture ?? mat.GetTexture("_BaseMap") ?? mat.GetTexture("_MainTex");

                mat.shader = targetShader;

                // Re-bind texture to both URP and Standard slots
                if (baseTex != null)
                {
                    if (mat.HasProperty("_BaseMap")) mat.SetTexture("_BaseMap", baseTex);
                    if (mat.HasProperty("_MainTex")) mat.SetTexture("_MainTex", baseTex);
                }
            }
        }
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name.Equals(name, System.StringComparison.OrdinalIgnoreCase)) return child;
            Transform result = FindDeepChild(child, name);
            if (result != null) return result;
        }
        return null;
    }
}