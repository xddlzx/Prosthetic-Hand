using System;
using UnityEngine;

/// <summary>
/// Bone-driven prosthetic hand controller for a rigged robotic hand FBX.
/// Attach this script to the imported hand prefab root, then drag the finger bone
/// Transforms into the Inspector.
///
/// Keyboard controls:
/// 1 = toggle index curl
/// 2 = toggle middle curl
/// 3 = toggle ring curl
/// 4 = toggle little curl
/// 5 = toggle thumb curl
/// O = open hand
/// F = fist
/// P = pinch
/// T = thumb up
/// G = multi-finger grasp
///
/// Important: disable/remove Animator components while using direct localRotation control,
/// otherwise the Animator will overwrite these bone rotations every frame.
/// </summary>
public class ProstheticHandBoneController : MonoBehaviour
{
    [Serializable]
    public class FingerRig
    {
        public string fingerName;

        [Header("Bones in order: base/proximal -> distal")]
        public Transform bone1;
        public Transform bone2;
        public Transform bone3;
        public Transform bone4; // Optional. Useful for this model's thumb.
        public bool useBone4;

        [Header("Full-curl local Euler offsets")]
        public Vector3 bone1FullCurlEuler = new Vector3(-75f, 0f, 0f);
        public Vector3 bone2FullCurlEuler = new Vector3(-80f, 0f, 0f);
        public Vector3 bone3FullCurlEuler = new Vector3(-70f, 0f, 0f);
        public Vector3 bone4FullCurlEuler = new Vector3(-45f, 0f, 0f);

        [Header("Runtime curl values")]
        [Range(0f, 1f)] public float currentCurl;
        [Range(0f, 1f)] public float targetCurl;

        private Quaternion open1;
        private Quaternion open2;
        private Quaternion open3;
        private Quaternion open4;
        private bool capturedOpenPose;

        public void CaptureOpenPose()
        {
            if (bone1 != null) open1 = bone1.localRotation;
            if (bone2 != null) open2 = bone2.localRotation;
            if (bone3 != null) open3 = bone3.localRotation;
            if (bone4 != null) open4 = bone4.localRotation;

            currentCurl = Mathf.Clamp01(currentCurl);
            targetCurl = Mathf.Clamp01(targetCurl);
            capturedOpenPose = true;
        }

        public void SetTarget(float curl01)
        {
            targetCurl = Mathf.Clamp01(curl01);
        }

        public void ToggleTarget()
        {
            targetCurl = targetCurl < 0.5f ? 1f : 0f;
        }

        public void UpdatePose(float curlUnitsPerSecond, float rotationLerpSpeed)
        {
            if (!capturedOpenPose)
            {
                CaptureOpenPose();
            }

            currentCurl = Mathf.MoveTowards(
                currentCurl,
                targetCurl,
                Mathf.Max(0.01f, curlUnitsPerSecond) * Time.deltaTime
            );

            float t = 1f - Mathf.Exp(-Mathf.Max(0.01f, rotationLerpSpeed) * Time.deltaTime);

            ApplyBone(bone1, open1, bone1FullCurlEuler, currentCurl, t);
            ApplyBone(bone2, open2, bone2FullCurlEuler, currentCurl, t);
            ApplyBone(bone3, open3, bone3FullCurlEuler, currentCurl, t);

            if (useBone4)
            {
                ApplyBone(bone4, open4, bone4FullCurlEuler, currentCurl, t);
            }
        }

        private static void ApplyBone(Transform bone, Quaternion openLocalRotation, Vector3 fullCurlEuler, float curl01, float lerp01)
        {
            if (bone == null) return;

            Quaternion targetRotation = openLocalRotation * Quaternion.Euler(fullCurlEuler * Mathf.Clamp01(curl01));
            bone.localRotation = Quaternion.Slerp(bone.localRotation, targetRotation, lerp01);
        }
    }

    public enum GestureCommand
    {
        OpenHand,
        Fist,
        Pinch,
        ThumbUp,
        IndexFlexion,
        MultiFingerGrasp
    }

    [Header("Finger rigs")]
    public FingerRig thumb = new FingerRig();
    public FingerRig index = new FingerRig();
    public FingerRig middle = new FingerRig();
    public FingerRig ring = new FingerRig();
    public FingerRig little = new FingerRig();

    [Header("Motion")]
    [Tooltip("How quickly curl values move from currentCurl to targetCurl, in 0..1 units per second.")]
    public float curlUnitsPerSecond = 10;

    [Tooltip("How quickly bone rotations visually interpolate to the current curl pose.")]
    public float rotationLerpSpeed = 25;

    [Header("Keyboard test control")]
    public bool keyboardControl = true;

    private void Reset()
    {
        thumb.fingerName = "Thumb";
        thumb.useBone4 = true;
        thumb.bone1FullCurlEuler = new Vector3(0f, -10f, 20f);
        thumb.bone2FullCurlEuler = new Vector3(10f, 20f, 55f);
        thumb.bone3FullCurlEuler = new Vector3(-20f, 15f, 45f);
        thumb.bone4FullCurlEuler = new Vector3(-5f, 5f, 55f);

        index.fingerName = "Index";
        middle.fingerName = "Middle";
        ring.fingerName = "Ring";
        little.fingerName = "Little";

        index.bone1FullCurlEuler = new Vector3(-75f, 0f, 0f);
        index.bone2FullCurlEuler = new Vector3(-80f, 0f, 0f);
        index.bone3FullCurlEuler = new Vector3(-70f, 0f, 0f);

        middle.bone1FullCurlEuler = new Vector3(-80f, 0f, 0f);
        middle.bone2FullCurlEuler = new Vector3(-85f, 0f, 0f);
        middle.bone3FullCurlEuler = new Vector3(-75f, 0f, 0f);

        ring.bone1FullCurlEuler = new Vector3(-80f, 0f, 0f);
        ring.bone2FullCurlEuler = new Vector3(-85f, 0f, 0f);
        ring.bone3FullCurlEuler = new Vector3(-75f, 0f, 0f);

        little.bone1FullCurlEuler = new Vector3(-70f, 0f, 0f);
        little.bone2FullCurlEuler = new Vector3(-75f, 0f, 0f);
        little.bone3FullCurlEuler = new Vector3(-65f, 0f, 0f);
    }

    private void Awake()
    {
        CaptureOpenPose();
        OpenHand();
    }

    private void Update()
    {
        if (keyboardControl)
        {
            HandleKeyboard();
        }

        thumb.UpdatePose(curlUnitsPerSecond, rotationLerpSpeed);
        index.UpdatePose(curlUnitsPerSecond, rotationLerpSpeed);
        middle.UpdatePose(curlUnitsPerSecond, rotationLerpSpeed);
        ring.UpdatePose(curlUnitsPerSecond, rotationLerpSpeed);
        little.UpdatePose(curlUnitsPerSecond, rotationLerpSpeed);
    }

    [ContextMenu("Capture Open Pose")]
    public void CaptureOpenPose()
    {
        thumb.CaptureOpenPose();
        index.CaptureOpenPose();
        middle.CaptureOpenPose();
        ring.CaptureOpenPose();
        little.CaptureOpenPose();
    }

    private void HandleKeyboard()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) index.ToggleTarget();
        if (Input.GetKeyDown(KeyCode.Alpha2)) middle.ToggleTarget();
        if (Input.GetKeyDown(KeyCode.Alpha3)) ring.ToggleTarget();
        if (Input.GetKeyDown(KeyCode.Alpha4)) little.ToggleTarget();
        if (Input.GetKeyDown(KeyCode.Alpha5)) thumb.ToggleTarget();

        if (Input.GetKeyDown(KeyCode.O)) OpenHand();
        if (Input.GetKeyDown(KeyCode.F)) Fist();
        if (Input.GetKeyDown(KeyCode.P)) Pinch();
        if (Input.GetKeyDown(KeyCode.T)) ThumbUp();
        if (Input.GetKeyDown(KeyCode.G)) MultiFingerGrasp();
    }

    public void SetFingerCurls(float thumbCurl, float indexCurl, float middleCurl, float ringCurl, float littleCurl)
    {
        thumb.SetTarget(thumbCurl);
        index.SetTarget(indexCurl);
        middle.SetTarget(middleCurl);
        ring.SetTarget(ringCurl);
        little.SetTarget(littleCurl);
    }

    public void ApplyGesture(GestureCommand gesture)
    {
        switch (gesture)
        {
            case GestureCommand.OpenHand:
                OpenHand();
                break;
            case GestureCommand.Fist:
                Fist();
                break;
            case GestureCommand.Pinch:
                Pinch();
                break;
            case GestureCommand.ThumbUp:
                ThumbUp();
                break;
            case GestureCommand.IndexFlexion:
                IndexFlexion();
                break;
            case GestureCommand.MultiFingerGrasp:
                MultiFingerGrasp();
                break;
            default:
                OpenHand();
                break;
        }
    }

    /// <summary>
    /// Call this later from your AI/backend bridge, for example:
    /// handController.ApplyGestureClass("fist");
    /// </summary>
    public void ApplyGestureClass(string gestureClass)
    {
        if (string.IsNullOrWhiteSpace(gestureClass)) return;

        string g = gestureClass.Trim().ToLowerInvariant().Replace(" ", "_").Replace("-", "_");

        switch (g)
        {
            case "open":
            case "open_hand":
            case "rest":
                OpenHand();
                break;

            case "fist":
            case "power_grasp":
                Fist();
                break;

            case "pinch":
            case "precision_pinch":
                Pinch();
                break;

            case "thumb_up":
            case "thumbs_up":
                ThumbUp();
                break;

            case "index":
            case "index_flexion":
                IndexFlexion();
                break;

            case "grasp":
            case "multi_finger_grasp":
                MultiFingerGrasp();
                break;

            default:
                Debug.LogWarning($"Unknown gesture class: {gestureClass}", this);
                break;
        }
    }

    public void OpenHand()
    {
        SetFingerCurls(0f, 0f, 0f, 0f, 0f);
    }

    public void Fist()
    {
        SetFingerCurls(0.85f, 1f, 1f, 1f, 1f);
    }

    public void Pinch()
    {
        // Thumb and index move toward each other; other fingers stay mostly open.
        SetFingerCurls(0.72f, 0.55f, 0.08f, 0f, 0f);
    }

    public void ThumbUp()
    {
        // Fingers closed, thumb open/extended.
        SetFingerCurls(0f, 1f, 1f, 1f, 1f);
    }

    public void IndexFlexion()
    {
        SetFingerCurls(0f, 1f, 0f, 0f, 0f);
    }

    public void MultiFingerGrasp()
    {
        SetFingerCurls(0.75f, 0.85f, 0.9f, 0.85f, 0.65f);
    }
    public void SetFingerAngle(string fingerName, float angleDegrees)
    {
        float curl01 = Mathf.Clamp(angleDegrees, 0f, 90f) / 90f;

        fingerName = fingerName.ToLower();

        if (fingerName == "thumb")
        {
            thumb.SetTarget(curl01);
        }
        else if (fingerName == "index")
        {
            index.SetTarget(curl01);
        }
        else if (fingerName == "middle")
        {
            middle.SetTarget(curl01);
        }
        else if (fingerName == "ring")
        {
            ring.SetTarget(curl01);
        }
        else if (fingerName == "little")
        {
            little.SetTarget(curl01);
        }
        else
        {
            Debug.LogWarning("Unknown finger name: " + fingerName);
        }
    }

    public void SetAllFingerAngles(
        float thumbAngle,
        float indexAngle,
        float middleAngle,
        float ringAngle,
        float littleAngle
    )
    {
        SetFingerAngle("thumb", thumbAngle);
        SetFingerAngle("index", indexAngle);
        SetFingerAngle("middle", middleAngle);
        SetFingerAngle("ring", ringAngle);
        SetFingerAngle("little", littleAngle);
    }

    public void ApplyJsonCommand(string json)
    {
        WebHandCommand command = JsonUtility.FromJson<WebHandCommand>(json);

        if (command == null || command.fingers == null)
        {
            Debug.LogWarning("Invalid hand command JSON");
            return;
        }

        SetAllFingerAngles(
            command.fingers.thumb,
            command.fingers.index,
            command.fingers.middle,
            command.fingers.ring,
            command.fingers.little
        );

        Debug.Log("Applied web command: " + json);
    }
}

[Serializable]
public class WebHandCommand
{
    public int id;
    public string gesture;
    public string bendLevel;
    public WebFingerAngles fingers;
}

[Serializable]
public class WebFingerAngles
{
    public float thumb;
    public float index;
    public float middle;
    public float ring;
    public float little;
}