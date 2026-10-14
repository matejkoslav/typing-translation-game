using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Prototype core loop: type the EN translation of the shown SK word,
/// the player hops one platform up, the camera slowly rises.
/// Fall below the camera edge and you die.
/// </summary>
public class TypingGameController : MonoBehaviour
{
    private struct WordPair
    {
        public readonly string sk;
        public readonly string en;

        public WordPair(string sk, string en)
        {
            this.sk = sk;
            this.en = en;
        }
    }

    [SerializeField] private GameObject platformsRoot;
    [SerializeField] private GameObject cameraObject;
    [SerializeField] private float cameraRiseSpeed = 0.35f;
    [SerializeField] private float cameraFollowOffset = 2.5f;
    [SerializeField] private float cameraFollowSpeed = 6f;
    [SerializeField] private float jumpDuration = 0.35f;
    [SerializeField] private float jumpArcHeight = 0.9f;

    private static readonly WordPair[] Words =
    {
        new WordPair("ma\u010dka", "cat"),
        new WordPair("pes", "dog"),
        new WordPair("dom", "house"),
        new WordPair("voda", "water"),
        new WordPair("chlieb", "bread"),
        new WordPair("kniha", "book"),
        new WordPair("strom", "tree"),
        new WordPair("slnko", "sun"),
        new WordPair("mesto", "city"),
        new WordPair("priate\u013e", "friend"),
    };

    private readonly StringBuilder typed = new StringBuilder();
    private int wordIndex;
    private int score;
    private int stepIndex;
    private bool alive = true;
    private bool jumping;
    private float cameraHalfHeight;

    private TextMeshProUGUI wordLabel;
    private TextMeshProUGUI typedLabel;
    private TextMeshProUGUI scoreLabel;
    private TextMeshProUGUI infoLabel;

    private void Start()
    {
        if (platformsRoot == null)
        {
            platformsRoot = GameObject.Find("Platforms");
        }

        if (cameraObject == null && Camera.main != null)
        {
            cameraObject = Camera.main.gameObject;
        }

        var camera = cameraObject.GetComponent<Camera>();
        float cameraDistance = Mathf.Abs(cameraObject.transform.position.z - transform.position.z);
        cameraHalfHeight = Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * cameraDistance
            + Mathf.Tan(Mathf.Abs(cameraObject.transform.eulerAngles.x) * Mathf.Deg2Rad) * cameraDistance;

        BuildUi();
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
        if (Keyboard.current != null)
        {
            Keyboard.current.onTextInput -= OnTextInput;
        }
    }

    private void Update()
    {
        if (!alive)
        {
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

    private void OnTextInput(char c)
    {
        if (!alive || jumping || !char.IsLetter(c))
        {
            return;
        }

        typed.Append(char.ToLowerInvariant(c));
        string target = Words[wordIndex].en;

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
            }
        }
        else
        {
            RefreshLabels();
        }
    }

    private void CorrectWord()
    {
        typed.Clear();
        score++;
        wordIndex = (wordIndex + 1) % Words.Length;
        RefreshLabels();
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

    private void RefreshLabels()
    {
        WordPair word = Words[wordIndex];
        wordLabel.text = "Prelo\u017e: " + word.sk;

        var sb = new StringBuilder();
        for (int i = 0; i < word.en.Length; i++)
        {
            sb.Append(i < typed.Length ? typed[i] : '_');
            if (i < word.en.Length - 1)
            {
                sb.Append(' ');
            }
        }

        typedLabel.text = sb.ToString();
        scoreLabel.text = "SK\u00d3RE: " + score;
    }

    private void Die(string reason)
    {
        alive = false;

        var playerRenderer = GetComponent<Renderer>();
        if (playerRenderer != null)
        {
            playerRenderer.enabled = false;
        }

        infoLabel.text = "GAME OVER - SK\u00d3RE: " + score;
        infoLabel.color = new Color(1f, 0.35f, 0.35f);
        Debug.Log("Death: " + reason);
    }

    private void Win()
    {
        alive = false;
        infoLabel.text = "VYHRAL SI!";
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
