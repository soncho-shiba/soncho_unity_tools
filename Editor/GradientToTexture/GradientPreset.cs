using UnityEngine;

// Gradientを保持するためのScriptableObjectクラス
[CreateAssetMenu(fileName = "GradientPreset", menuName = "SonchoTools/Gradient Preset", order = 1)]
public class GradientPreset : ScriptableObject
{
    public Gradient gradient;
}
