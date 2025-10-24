using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ThiNghiem : MonoBehaviour
{
    public GameObject testTube;
    public GameObject cuBottle;
    public GameObject hno3Bottle;
    public GameObject dropper;
    public GameObject spoon;
    public GameObject no2Gas;
    public GameObject cuParticle;
    public Image solutionImage;
    public TextMeshProUGUI infoText;
    public float spoonTiltAngle = 45f;
    public float spoonTiltDuration = 0.5f;

    private bool reactionStarted = false;
    private float reactionTime = 0f;
    private int dropCount = 0;
    private bool cuAdded = false;
    private bool hasCu = false;
    private bool hasHNO3 = false;
    private bool isDraggingSpoon = false;
    private bool isDraggingDropper = false;
    private Vector3 offset;
    private Vector3 originalTubePos; private Quaternion originalSpoonRot;
    private Color startColor = new Color(0.8f, 0.9f, 1f);
    private Color targetColor = new Color(0.2f, 0.6f, 0.8f);
    private float colorTransitionTime = 0f;

    void Start()
    {
        originalTubePos = testTube.transform.position;
        originalSpoonRot = spoon.transform.rotation;
        no2Gas.GetComponent<ParticleSystem>()?.Stop();
    }

    void Update()
    {
        if (!hasCu) infoText.text = "Take Cu";
        else if (hasCu && !cuAdded) infoText.text = "Pour Cu into the test tube.";
        else if (cuAdded && !hasHNO3) infoText.text = "Take HNO3";
        else if (hasHNO3 && cuAdded) infoText.text = "Drop HNO3.";
        if (reactionStarted) infoText.text = "Reaction...";

        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                if (hit.collider.gameObject == spoon)
                {
                    if (Vector2.Distance(spoon.transform.position, cuBottle.transform.position) < 0.5f) TakeCu();
                    isDraggingSpoon = true;
                    offset = spoon.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
                else if (hit.collider.gameObject == dropper)
                {
                    if (Vector2.Distance(dropper.transform.position, hno3Bottle.transform.position) < 0.5f) TakeHNO3();
                    isDraggingDropper = true;
                    offset = dropper.transform.position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
                }
                else if (hit.collider.gameObject == testTube && hasCu && !cuAdded)
                {
                    StartCoroutine(DropCu());
                }
                else if (hit.collider.gameObject == testTube && hasHNO3 && cuAdded)
                {
                    DropHNO3();
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isDraggingSpoon = false;
            isDraggingDropper = false;
        }

        if (isDraggingSpoon)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            spoon.transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, spoon.transform.position.z);
        }
        else if (isDraggingDropper)
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            dropper.transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, dropper.transform.position.z);
        }

        if (reactionStarted)
        {
            SimulateReaction();
        }
    }

    void TakeCu()
    {
        Debug.Log("Lấy Cu");
        hasCu = true;
    }

    void TakeHNO3()
    {
        Debug.Log("Lấy HNO₃");
        hasHNO3 = true;
    }

    IEnumerator DropCu()
    {
        if (hasCu && !cuAdded)
        {
            Vector3 targetPos = testTube.transform.position + new Vector3(2.56f / 2.0f, 3.85f * 0.55f, 0);
            float moveDuration = 0.3f;
            float elapsed = 0f;
            Vector3 startPos = spoon.transform.position;

            while (elapsed < moveDuration)
            {
                spoon.transform.position = Vector3.Lerp(startPos, targetPos, elapsed / moveDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            spoon.transform.position = targetPos;

            elapsed = 0f;
            Quaternion startRot = spoon.transform.rotation;
            Quaternion targetRot = Quaternion.Euler(0, 0, spoonTiltAngle);

            ParticleSystem ps = cuParticle.GetComponent<ParticleSystem>();
            ps?.Play();

            while (elapsed < spoonTiltDuration)
            {
                spoon.transform.rotation = Quaternion.Lerp(startRot, targetRot, elapsed / spoonTiltDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < spoonTiltDuration)
            {
                spoon.transform.rotation = Quaternion.Lerp(targetRot, originalSpoonRot, elapsed / spoonTiltDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            spoon.transform.rotation = originalSpoonRot;

            cuAdded = true;
            Debug.Log("Đổ Cu vào ống nghiệm.");
            if (ps != null) yield return new WaitForSeconds(ps.main.duration);
            ps?.Stop();
        }
    }

    void DropCuObject()
    {

    }

    void DropHNO3()
    {
        if (dropCount < 4)
        {
            dropCount++;
            StartCoroutine(MoveObjectAnimation(dropper.transform.position, testTube.transform.position));
            if (dropCount == 1)
            {
                colorTransitionTime = 0f;
            }
            Debug.Log($"Nhỏ giọt HNO₃ ({dropCount}/4).");
        }
        if (dropCount == 4)
        {
            //dropper.SetActive(false);

        }

        reactionStarted = true;
        StartCoroutine(PlayNO2GasEffect());
    }

    void DropHNO3Water()
    {

    }

    IEnumerator MoveObjectAnimation(Vector3 start, Vector3 end, bool isMoveBack = true)
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 originalPos = dropper.transform.position;
        while (elapsed < duration)
        {
            dropper.transform.position = Vector3.Lerp(start, end, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (isMoveBack) dropper.transform.position = originalPos;
    }

    IEnumerator PlayNO2GasEffect()
    {
        ParticleSystem ps = no2Gas.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            ps.Play();
            yield return new WaitForSeconds(ps.main.duration);
            ps.Stop();
        }
    }

    void SimulateReaction()
    {
        reactionTime += Time.deltaTime;
        colorTransitionTime += Time.deltaTime;

        if (dropCount >= 1)
        {
            float t = Mathf.Clamp01(colorTransitionTime / 2f);
            solutionImage.color = Color.Lerp(startColor, targetColor, t);
        }

        if (reactionTime > 1f && dropCount >= 4)
        {
            if (reactionTime > 5f)
            {
                reactionStarted = false;
                Debug.Log("Phản ứng hoàn tất.");
            }
        }
    }
}