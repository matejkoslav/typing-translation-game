using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Prototype core loop: type the translation of the shown word (SK↔EN),
/// the player hops one platform up, the camera slowly rises.
/// Fall below the camera edge and you die.
/// </summary>
public class TypingGameController : MonoBehaviour
{
    private const string HighScoreKey = "HighScore";
    private const string DirectionKey = "Direction";
    private const float ShakeDuration = 0.2f;
    private const float ShakeAngle = 1.5f;
    private const float PlatformSpacing = 2f;
    private const float HintRevealDuration = 1.5f;
    private static readonly Color LevelColor = new Color(0.5f, 0.8f, 1f);

    private enum Direction
    {
        SkToEn = 0,
        EnToSk = 1,
    }

    [SerializeField] private WordBank[] wordBanks;
    [SerializeField] private GameObject platformsRoot;
    [SerializeField] private GameObject cameraObject;
    [SerializeField] private float cameraRiseSpeed = 0.35f;
    [SerializeField] private float cameraFollowOffset = 2.5f;
    [SerializeField] private float cameraFollowSpeed = 6f;
    [SerializeField] private float jumpDuration = 0.35f;
    [SerializeField] private float jumpArcHeight = 0.9f;
    [SerializeField] private bool hintsEnabled = true;

    private readonly StringBuilder typed = new StringBuilder();
    private readonly HashSet<int> usedWordIndices = new HashSet<int>();
    private WordBank currentBank;
    private WordBank.WordEntry currentWord;
    private Direction direction;
    private bool choosingDirection;
    private int score;
    private int bestScore;
    private int stepIndex;
    private bool alive = true;
    private bool jumping;
    private bool paused;
    private int hintIndex;
    private float hintRevealTime;
    private float cameraHalfHeight;
    private float shakeTime;
    private Quaternion baseCameraRotation;

    private TextMeshProUGUI wordLabel;
    private TextMeshProUGUI typedLabel;
    private TextMeshProUGUI scoreLabel;
    private TextMeshProUGUI levelLabel;
    private TextMeshProUGUI bestLabel;
    private TextMeshProUGUI directionLabel;
    private TextMeshProUGUI infoLabel;

    private string Answer => direction == Direction.SkToEn ? currentWord.en : currentWord.sk;
    private string Prompt => direction == Direction.SkToEn ? currentWord.sk : currentWord.en;

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
        direction = (Direction)PlayerPrefs.GetInt(DirectionKey, (int)Direction.SkToEn);

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

        choosingDirection = true;
        infoLabel.text = "Vyber smer:\n1  SK \u2192 EN\n2  EN \u2192 SK";

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
        if (choosingDirection)
        {
            HandleDirectionChoice();
            return;
        }

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

        if (!jumping && hintRevealTime > 0f)
        {
            hintRevealTime -= Time.deltaTime;
            if (hintRevealTime <= 0f)
            {
                RefreshLabels();
            }
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

    private void HandleDirectionChoice()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            SetDirection(Direction.SkToEn);
        }
        else if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            SetDirection(Direction.EnToSk);
        }
    }

    private void SetDirection(Direction value)
    {
        direction = value;
        PlayerPrefs.SetInt(DirectionKey, (int)direction);
        PlayerPrefs.Save();

        typed.Clear();
        choosingDirection = false;
        infoLabel.text = string.Empty;
        ResetHint();
        RefreshLabels();
    }

    private void ResetHint()
    {
        hintRevealTime = 0f;

        if (!hintsEnabled || currentBank.Level < WordBank.CefrLevel.B2 || Answer.Length == 0)
        {
            return;
        }

        hintIndex = Random.Range(0, Answer.Length);
        hintRevealTime = HintRevealDuration;
    }

    private void OnTextInput(char c)
    {
        if (!alive || paused || choosingDirection || jumping || !char.IsLetter(c))
        {
            return;
        }

        typed.Append(char.ToLowerInvariant(c));
        string target = Normalize(Answer);

        if (typed.Length >= Answer.Length)
        {
            if (Normalize(typed.ToString()) == target)
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
        Transform target = steps.GetChild(next % steps.childCount);

        if (next >= steps.childCount)
        {
            float topY = float.MinValue;
            for (int i = 0; i < steps.childCount; i++)
            {
                topY = Mathf.Max(topY, steps.GetChild(i).position.y);
            }

            Vector3 targetPosition = target.position;
            target.position = new Vector3(targetPosition.x, topY + PlatformSpacing, targetPosition.z);
        }

        jumping = true;

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
        ResetHint();

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
        string answer = Answer;
        wordLabel.text = "Prelo\u017e: " + Prompt;

        var sb = new StringBuilder();
        for (int i = 0; i < answer.Length; i++)
        {
            char letter = i < typed.Length
                ? typed[i]
                : (i == hintIndex && hintRevealTime > 0f ? answer[i] : '_');
            sb.Append(letter);
            if (i < answer.Length - 1)
            {
                sb.Append(' ');
            }
        }

        typedLabel.text = sb.ToString();
        scoreLabel.text = "SK\u00d3RE: " + score;
        levelLabel.text = "LEVEL: " + currentBank.Level;
        bestLabel.text = "NAJLEP\u0160IE: " + bestScore;
        directionLabel.text = direction == Direction.SkToEn ? "SK \u2192 EN" : "EN \u2192 SK";
    }

    private static string Normalize(string value)
    {
        string decomposed = value.ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(decomposed.Length);

        foreach (char c in decomposed)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
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

        shakeTime = ShakeDuration;
        StartCoroutine(DeathRoutine());

        infoLabel.text = "GAME OVER - SK\u00d3RE: " + score
            + (newBest ? "\n<color=#66FF66>NEW BEST!</color>" : string.Empty)
            + "\nStla\u010d R pre restart";
        infoLabel.color = new Color(1f, 0.35f, 0.35f);
        Debug.Log("Death: " + reason);
    }

    private IEnumerator DeathRoutine()
    {
        var playerRenderer = GetComponent<Renderer>();
        Vector3 start = transform.position;
        Quaternion startRotation = transform.rotation;

        float t = 0f;
        while (t < 0.7f)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / 0.7f);
            transform.position = start + Vector3.down * (p * p * 3f);
            transform.rotation = startRotation * Quaternion.Euler(0f, 0f, p * 180f);
            yield return null;
        }

        if (playerRenderer != null)
        {
            playerRenderer.enabled = false;
        }
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
        directionLabel = CreateLabel(canvasGo.transform, "DirectionLabel", 32f, new Vector2(0f, 1f), new Vector2(40f, -95f), TextAlignmentOptions.TopLeft);
        directionLabel.color = new Color(0.75f, 0.75f, 0.75f);
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
