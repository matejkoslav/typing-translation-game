using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Prototype core loop: type the EN translation of the shown SK word,
/// the player hops one platform up, the camera slowly rises.
/// Fall below the camera edge and you die.
/// </summary>
public class TypingGameController : MonoBehaviour
{
    private const string HighScoreKey = "HighScore";
    private const float ShakeDuration = 0.2f;
    private const float ShakeAngle = 1.5f;
    private static readonly Color LevelColor = new Color(0.5f, 0.8f, 1f);

    [SerializeField] private WordBank[] wordBanks;
    [SerializeField] private GameObject platformsRoot;
    [SerializeField] private GameObject cameraObject;
    [SerializeField] private float cameraRiseSpeed = 0.35f;
    [SerializeField] private float cameraFollowOffset = 2.5f;
    [SerializeField] private float cameraFollowSpeed = 6f;
    [SerializeField] private float jumpDuration = 0.35f;
    [SerializeField] private float jumpArcHeight = 0.9f;

    private readonly StringBuilder typed = new StringBuilder();
    private readonly HashSet<int> usedWordIndices = new HashSet<int>();
    private WordBank currentBank;
    private WordBank.WordEntry currentWord;
    private int score;
    private int bestScore;
    private int stepIndex;
    private bool alive = true;
    private bool jumping;
    private bool paused;
    private float cameraHalfHeight;
    private float shakeTime;
    private Quaternion baseCameraRotation;

    private TextMeshProUGUI wordLabel;
    private TextMeshProUGUI typedLabel;
    private TextMeshProUGUI scoreLabel;
    private TextMeshProUGUI levelLabel;
    private TextMeshProUGUI bestLabel;
    private TextMeshProUGUI infoLabel;

    private void Start()
    {
        Time.timeScale = 1f;

        if (platformsRoot == null)
        {
            platformsRoot = GameObject.Find("Platforms");
        }

        if (wordBanks == null || wordBanks.Length == 0)
        {
            Debug.LogError("TypingGameController: no word banks assigned.");
            enabled = false;
            return;
        }

        bestScore = PlayerPrefs.GetInt(HighScoreKey, 0);

        if (cameraObject == null && Camera.main != null)
        {
            cameraObject = Camera.main.gameObject;
        }

        baseCameraRotation = cameraObject.transform.rotation;

        var camera = cameraObject.GetComponent<Camera>();
        float cameraDistance = Mathf.Abs(cameraObject.transform.position.z - transform.position.z);
        cameraHalfHeight = Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * cameraDistance
            + Mathf.Tan(Mathf.Abs(cameraObject.transform.eulerAngles.x) * Mathf.Deg2Rad) * cameraDistance;

        BuildUi();
        SelectWord(0);
        RefreshLabels();

        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput += OnTextInput;
        }
        else
        {
            Debug.LogError("TypingGameController: no keyboard detected (new Input System).");
        }
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;

        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput -= OnTextInput;
        }
    }

    private void Update()
    {
        if (alive && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            SetPaused(!paused);
        }

        if (paused || !alive)
        {
            if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            {
                Restart();
            }

            return;
        }

        if (Keyboard.current != null && Keyboard.current.backspaceKey.wasPressedThisFrame && typed.Length > 0)
        {
            typed.Length--;
            RefreshLabels();
        }

        cameraObject.transform.position += Vector3.up * cameraRiseSpeed * Time.deltaTime;

        float followTarget = transform.position.y - cameraFollowOffset;
        if (cameraObject.transform.position.y < followTarget)
        {
            Vector3 cameraPosition = cameraObject.transform.position;
            cameraPosition.y = Mathf.Min(cameraPosition.y + cameraFollowSpeed * Time.deltaTime, followTarget);
            cameraObject.transform.position = cameraPosition;
        }

        if (transform.position.y < cameraObject.transform.position.y - cameraHalfHeight)
        {
            Die("Player fell below the camera.");
        }
    }

    private void LateUpdate()
    {
        if (cameraObject == null)
        {
            return;
        }

        if (shakeTime > 0f)
        {
            shakeTime -= Time.deltaTime;
            float falloff = Mathf.Clamp01(shakeTime / ShakeDuration);
            float angle = Mathf.Sin(Time.time * 70f) * ShakeAngle * falloff;
            cameraObject.transform.rotation = baseCameraRotation * Quaternion.Euler(0f, 0f, angle);
        }
        else if (cameraObject.transform.rotation != baseCameraRotation)
        {
            cameraObject.transform.rotation = baseCameraRotation;
        }
    }

    private void SetPaused(bool value)
    {
        paused = value;
        Time.timeScale = paused ? 0f : 1f;
        infoLabel.text = paused ? "PAUZA\nESC pokra\u010dova\u0165\nR restart" : string.Empty;
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void OnTextInput(char c)
    {
        if (!alive || paused || jumping || !char.IsLetter(c))
        {
            return;
        }

        typed.Append(char.ToLowerInvariant(c));
        string target = currentWord.en;

        if (typed.Length >= target.Length)
        {
            if (string.Equals(typed.ToString(), target, System.StringComparison.OrdinalIgnoreCase))
            {
                CorrectWord();
            }
            else
            {
                typed.Clear();
                RefreshLabels();
                StartCoroutine(FlashWrong());
                shakeTime = ShakeDuration;
            }
        }
        else
        {
            RefreshLabels();
        }
    }

    private IEnumerator FlashWrong()
    {
        typedLabel.color = new Color(1f, 0.35f, 0.35f);
        yield return new WaitForSeconds(0.15f);
        typedLabel.color = Color.white;
    }

    private IEnumerator FlashCorrect()
    {
        typedLabel.color = new Color(0.4f, 1f, 0.4f);
        scoreLabel.transform.localScale = Vector3.one * 1.15f;
        yield return new WaitForSeconds(0.15f);
        typedLabel.color = Color.white;
        scoreLabel.transform.localScale = Vector3.one;
    }

    private void CorrectWord()
    {
        typed.Clear();
        score++;
        SelectWord(stepIndex + 1);
        RefreshLabels();
        StartCoroutine(FlashCorrect());
        StartCoroutine(JumpRoutine());
    }

    private IEnumerator JumpRoutine()
    {
        int next = stepIndex + 1;
        Transform steps = platformsRoot.transform;

        if (next >= steps.childCount)
        {
            Win();
            yield break;
        }

        jumping = true;

        Transform target = steps.GetChild(next);
        Vector3 start = transform.position;
        Vector3 end = target.position;
        end.y = target.position.y + target.lossyScale.y * 0.5f + 1f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / jumpDuration;
            float clamped = Mathf.Clamp01(t);
            Vector3 pos = Vector3.Lerp(start, end, clamped);
            pos.y += Mathf.Sin(clamped * Mathf.PI) * jumpArcHeight;
            transform.position = pos;
            yield return null;
        }

        transform.position = end;
        stepIndex = next;
        jumping = false;
    }

    private void SelectWord(int step)
    {
        int bankCount = platformsRoot.transform.childCount;
        int bankIndex = Mathf.Clamp(step * wordBanks.Length / Mathf.Max(1, bankCount), 0, wordBanks.Length - 1);
        WordBank bank = wordBanks[bankIndex];
        bool levelChanged = currentBank != null && bank != currentBank;

        if (bank != currentBank)
        {
            currentBank = bank;
            usedWordIndices.Clear();
        }

        if (usedWordIndices.Count >= bank.Count)
        {
            usedWordIndices.Clear();
        }

        int index;
        do
        {
            index = Random.Range(0, bank.Count);
        } while (usedWordIndices.Contains(index));

        usedWordIndices.Add(index);
        currentWord = bank.Words[index];

        if (levelChanged)
        {
            StartCoroutine(PulseLevelLabel());
        }
    }

    private IEnumerator PulseLevelLabel()
    {
        float t = 0f;
        while (t < 0.3f)
        {
            t += Time.unscaledDeltaTime;
            float pulse = Mathf.Sin(Mathf.Clamp01(t / 0.3f) * Mathf.PI);
            levelLabel.transform.localScale = Vector3.one * (1f + 0.25f * pulse);
            levelLabel.color = Color.Lerp(LevelColor, Color.white, pulse);
            yield return null;
        }

        levelLabel.transform.localScale = Vector3.one;
        levelLabel.color = LevelColor;
    }

    private void RefreshLabels()
    {
        wordLabel.text = "Prelo\u017e: " + currentWord.sk;

        var sb = new StringBuilder();
        for (int i = 0; i < currentWord.en.Length; i++)
        {
            sb.Append(i < typed.Length ? typed[i] : '_');
            if (i < currentWord.en.Length - 1)
            {
                sb.Append(' ');
            }
        }

        typedLabel.text = sb.ToString();
        scoreLabel.text = "SK\u00d3RE: " + score;
        levelLabel.text = "LEVEL: " + currentBank.Level;
        bestLabel.text = "NAJLEP\u0160IE: " + bestScore;
    }

    private bool SaveScore()
    {
        if (score <= bestScore)
        {
            return false;
        }

        bestScore = score;
        PlayerPrefs.SetInt(HighScoreKey, bestScore);
        PlayerPrefs.Save();
        bestLabel.text = "NAJLEP\u0160IE: " + bestScore;
        return true;
    }

    private void Die(string reason)
    {
        alive = false;
        bool newBest = SaveScore();

        var playerRenderer = GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            playerRenderer.enabled = false;
        }

        infoLabel.text = "GAME OVER - SK\u00d3RE: " + score
            + (newBest ? "\n<color=#66FF66>NEW BEST!</color>" : string.Empty)
            + "\nStla\u010d R pre restart";
        infoLabel.color = new Color(1f, 0.35f, 0.35f);
        Debug.Log("Death: " + reason);
    }

    private void Win()
    {
        alive = false;
        bool newBest = SaveScore();
        infoLabel.text = "VYHRAL SI!"
            + (newBest ? "\nNEW BEST!" : string.Empty)
            + "\nStla\u010d R pre restart";
        infoLabel.color = new Color(0.4f, 1f, 0.4f);
    }

    private void BuildUi()
    {
        var canvasGo = new GameObject("UI", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        wordLabel = CreateLabel(canvasGo.transform, "WordLabel", 64f, new Vector2(0.5f, 1f), new Vector2(0f, -80f), TextAlignmentOptions.Center);
        typedLabel = CreateLabel(canvasGo.transform, "TypedLabel", 64f, new Vector2(0.5f, 1f), new Vector2(0f, -170f), TextAlignmentOptions.Center);
        scoreLabel = CreateLabel(canvasGo.transform, "ScoreLabel", 40f, new Vector2(0f, 1f), new Vector2(40f, -40f), TextAlignmentOptions.TopLeft);
        levelLabel = CreateLabel(canvasGo.transform, "LevelLabel", 40f, new Vector2(1f, 1f), new Vector2(-40f, -40f), TextAlignmentOptions.TopRight);
        levelLabel.color = LevelColor;
        bestLabel = CreateLabel(canvasGo.transform, "BestLabel", 32f, new Vector2(1f, 1f), new Vector2(-40f, -95f), TextAlignmentOptions.TopRight);
        bestLabel.color = new Color(0.75f, 0.75f, 0.75f);
        infoLabel = CreateLabel(canvasGo.transform, "InfoLabel", 72f, new Vector2(0.5f, 0.5f), Vector2.zero, TextAlignmentOptions.Center);
    }

    private static TextMeshProUGUI CreateLabel(Transform parent, string name, float size, Vector2 anchor, Vector2 offset, TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var rect = (RectTransform)go.transform;
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = offset;
        rect.sizeDelta = new Vector2(1400f, 120f);

        var text = go.GetComponent<TextMeshProUGUI>();
        text.fontSize = size;
        text.alignment = alignment;
        return text;
    }
}
