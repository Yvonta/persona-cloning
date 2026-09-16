using System;
using System.IO;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Yvonta;

    public class UiVoiceCloning : MonoBehaviour
    {
        public event Action<byte[], string, string> OnAudioRecorded;

        [Header("Recording Settings")]
        [SerializeField] private int maxRecordingDuration = 30;
        [SerializeField] private int sampleRate = 44100;
        [SerializeField] private float volumeSensitivity = 10f;

        [Header("Phonetic Sentence")]
        [TextArea(3, 5)]
        [SerializeField] private string targetSentence = 
            "The quick brown fox jumps over the lazy dog, while five boxing wizards jump quickly with vibrant joy.";

        // UI References
        private CanvasGroup canvasGroup;
        private GameObject panelObj;
        private TextMeshProUGUI sentenceText;
        private TMP_InputField voiceNameInput;
        private Button recordButton;
        private TextMeshProUGUI recordButtonText;
        private Button previewButton;
        private TextMeshProUGUI previewButtonText;
        private Button uploadButton;
        private TextMeshProUGUI uploadButtonText;
        private Slider volumeMeterSlider;
        private AudioSource previewAudioSource;

        // Recording State
        private AudioClip recordedClip;
        private AudioClip trimmedClip;
        private byte[] cachedWavBytes;
        private string selectedMicrophone;
        private bool isRecording = false;

        private void Awake()
        {
            EnsureEventSystem();
            SetupAudioSource();
        }

        private void Start()
        {
            if (sentenceText != null)
            {
                sentenceText.text = targetSentence;
            }

            if (Microphone.devices.Length > 0)
            {
                selectedMicrophone = Microphone.devices[0];
                SetupButtonListeners();
                UpdateButtonText("Start Recording");
            }
            else
            {
                UpdateButtonText("No Mic Found");
                DisableAllButtons();
            }
        }

        private void Update()
        {
            if (isRecording)
            {
                UpdateVolumeMeter();
            }
        }

        public void BuildUI(Transform parentTransform)
{
    if (panelObj != null) return;

    // Panel Root (Increased height to accommodate all elements)
    panelObj = UIHelper.CreatePanel(
        parentTransform, 
        "VoiceCloningPanel", 
        new Vector2(500, 520), 
        new RectOffset(30, 30, 25, 25), 
        12f
    );

    canvasGroup = panelObj.AddComponent<CanvasGroup>();

    // Header Section
    UIHelper.CreateHeader(panelObj.transform, "Voice Studio", "Record your reading sample to clone your voice", 40f);

    // Phonetic Reading Card
    sentenceText = UIHelper.CreateCard(panelObj.transform, targetSentence, 90f);

    // Voice Name Input Field
    voiceNameInput = UIHelper.CreateInputField(panelObj.transform, "Voice Profile Name", TMP_InputField.ContentType.Standard, 38f);
    voiceNameInput.text = "My Personal Voice";

    // Volume Meter Bar
    volumeMeterSlider = UIHelper.CreateSlider(panelObj.transform, 8f);

    // Buttons
    ColorBlock uploadColors = ColorBlock.defaultColorBlock;
    uploadColors.normalColor = UIHelper.SecondaryAccent;
    uploadColors.highlightedColor = UIHelper.SecondaryHover;

    recordButton = UIHelper.CreateButton(panelObj.transform, "Start Recording", UIHelper.GetPrimaryButtonColors(), UIHelper.TextLight, 42f, out recordButtonText);
    previewButton = UIHelper.CreateButton(panelObj.transform, "Play Preview", UIHelper.GetSecondaryButtonColors(), UIHelper.TextLight, 38f, out previewButtonText);
    uploadButton = UIHelper.CreateButton(panelObj.transform, "Upload & Process Voice", uploadColors, UIHelper.TextLight, 42f, out uploadButtonText);

    if (Microphone.devices.Length > 0)
    {
        selectedMicrophone = Microphone.devices[0];
        SetupButtonListeners();
        previewButton.interactable = false;
        uploadButton.interactable = false;
    }
}

        private void SetupButtonListeners()
        {
            if (recordButton != null)
            {
                recordButton.onClick.RemoveAllListeners();
                recordButton.onClick.AddListener(ToggleRecording);
            }
            if (previewButton != null)
            {
                previewButton.onClick.RemoveAllListeners();
                previewButton.onClick.AddListener(PlayRecordedPreview);
            }
            if (uploadButton != null)
            {
                uploadButton.onClick.RemoveAllListeners();
                uploadButton.onClick.AddListener(UploadRecordedAudio);
            }
        }

        private void DisableAllButtons()
        {
            if (recordButton != null) recordButton.interactable = false;
            if (previewButton != null) previewButton.interactable = false;
            if (uploadButton != null) uploadButton.interactable = false;
        }

        public void SetVisible(bool isVisible)
        {
            if (canvasGroup == null && panelObj != null)
            {
                canvasGroup = panelObj.GetComponent<CanvasGroup>();
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = isVisible ? 1f : 0f;
                canvasGroup.interactable = isVisible;
                canvasGroup.blocksRaycasts = isVisible;
            }
            else if (panelObj != null)
            {
                panelObj.SetActive(isVisible);
            }
        }

        public void UpdateButtonText(string text)
        {
            if (recordButtonText != null)
            {
                recordButtonText.text = text;
            }
        }

        public void SetRecordButtonInteractable(bool interactable)
        {
            if (recordButton != null) recordButton.interactable = interactable;
        }

        public void SetUploadButtonInteractable(bool interactable)
        {
            if (uploadButton != null) uploadButton.interactable = interactable;
        }

        public void ToggleRecording()
        {
            if (!isRecording)
            {
                StartRecording();
            }
            else
            {
                StopAndProcessRecording();
            }
        }

        private void StartRecording()
        {
            if (string.IsNullOrEmpty(selectedMicrophone)) return;
            if (Microphone.IsRecording(selectedMicrophone)) return;

            if (previewAudioSource != null && previewAudioSource.isPlaying)
            {
                previewAudioSource.Stop();
            }

            if (previewButton != null) previewButton.interactable = false;
            if (uploadButton != null) uploadButton.interactable = false;

            cachedWavBytes = null;
            isRecording = true;
            UpdateButtonText("Stop Recording");

            recordedClip = Microphone.Start(selectedMicrophone, false, maxRecordingDuration, sampleRate);
            StartCoroutine(AutoStopRoutine(maxRecordingDuration));
        }

        private void StopAndProcessRecording()
        {
            if (!isRecording) return;

            int recordPosition = Microphone.GetPosition(selectedMicrophone);
            Microphone.End(selectedMicrophone);
            isRecording = false;

            ResetVolumeMeter();
            UpdateButtonText("Processing Audio...");
            recordButton.interactable = false;

            if (recordPosition <= 0)
            {
                UpdateButtonText("Start Recording");
                recordButton.interactable = true;
                return;
            }

            trimmedClip = TrimClipToActualSamples(recordedClip, recordPosition);

            if (previewAudioSource != null)
            {
                previewAudioSource.clip = trimmedClip;
            }

            cachedWavBytes = EncodeToWav(trimmedClip);

            UpdateButtonText("Start Recording");
            recordButton.interactable = true;
            if (previewButton != null) previewButton.interactable = true;
            if (uploadButton != null) uploadButton.interactable = true;
        }

        private void PlayRecordedPreview()
        {
            if (previewAudioSource == null)
            {
                SetupAudioSource();
            }

            if (trimmedClip == null) return;

            previewAudioSource.clip = trimmedClip;
            previewAudioSource.Stop();
            previewAudioSource.Play();
        }

        private void UploadRecordedAudio()
        {
            if (cachedWavBytes == null || cachedWavBytes.Length == 0) return;

            string voiceName = voiceNameInput != null && !string.IsNullOrEmpty(voiceNameInput.text)
                ? voiceNameInput.text 
                : "My Personal Voice";

            OnAudioRecorded?.Invoke(cachedWavBytes, targetSentence, voiceName);
        }

        private void UpdateVolumeMeter()
        {
            if (volumeMeterSlider == null || recordedClip == null || string.IsNullOrEmpty(selectedMicrophone)) return;

            int micPosition = Microphone.GetPosition(selectedMicrophone);
            int sampleWindow = 128;

            if (micPosition < sampleWindow) return;

            float[] waveData = new float[sampleWindow];
            recordedClip.GetData(waveData, micPosition - sampleWindow);

            float sum = 0f;
            for (int i = 0; i < sampleWindow; i++)
            {
                sum += waveData[i] * waveData[i];
            }

            float rmsVolume = Mathf.Sqrt(sum / sampleWindow);
            volumeMeterSlider.value = Mathf.Clamp01(rmsVolume * volumeSensitivity);
        }

        private void ResetVolumeMeter()
        {
            if (volumeMeterSlider != null)
            {
                volumeMeterSlider.value = 0f;
            }
        }

        private AudioClip TrimClipToActualSamples(AudioClip sourceClip, int actualSamplesRecorded)
        {
            if (actualSamplesRecorded <= 0) return sourceClip;

            float[] sampleBuffer = new float[actualSamplesRecorded * sourceClip.channels];
            sourceClip.GetData(sampleBuffer, 0);

            AudioClip trimmed = AudioClip.Create(
                sourceClip.name + "_trimmed", 
                actualSamplesRecorded, 
                sourceClip.channels, 
                sourceClip.frequency, 
                false
            );

            trimmed.SetData(sampleBuffer, 0);
            return trimmed;
        }

        private IEnumerator AutoStopRoutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            if (isRecording)
            {
                StopAndProcessRecording();
            }
        }

        private void SetupAudioSource()
        {
            previewAudioSource = GetComponent<AudioSource>();
            if (previewAudioSource == null)
            {
                previewAudioSource = gameObject.AddComponent<AudioSource>();
            }
            previewAudioSource.playOnAwake = false;
            previewAudioSource.spatialBlend = 0f;
            previewAudioSource.volume = 1f;
        }

        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject eventSystemObj = new GameObject("EventSystem");
                eventSystemObj.AddComponent<EventSystem>();
                eventSystemObj.AddComponent<StandaloneInputModule>();
            }
        }

        private byte[] EncodeToWav(AudioClip clip)
        {
            int totalSamples = clip.samples * clip.channels;
            int pcmByteCount = totalSamples * 2;

            using (MemoryStream stream = new MemoryStream(44 + pcmByteCount))
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                float[] samples = new float[totalSamples];
                clip.GetData(samples, 0);

                writer.Write(new char[4] { 'R', 'I', 'F', 'F' });
                writer.Write(36 + pcmByteCount);
                writer.Write(new char[4] { 'W', 'A', 'V', 'E' });
                writer.Write(new char[4] { 'f', 'm', 't', ' ' });
                writer.Write(16);
                writer.Write((ushort)1);
                writer.Write((ushort)clip.channels);
                writer.Write(clip.frequency);
                writer.Write(clip.frequency * clip.channels * 2);
                writer.Write((ushort)(clip.channels * 2));
                writer.Write((ushort)16);
                writer.Write(new char[4] { 'd', 'a', 't', 'a' });
                writer.Write(pcmByteCount);

                for (int i = 0; i < samples.Length; i++)
                {
                    float clamped = Mathf.Clamp(samples[i], -1f, 1f);
                    short sample = (short)(clamped * 32767f);
                    writer.Write(sample);
                }

                return stream.ToArray();
            }
        }
    }