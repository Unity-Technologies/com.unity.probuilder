#if UNITY_2021_3_OR_NEWER

using UnityEngine.UIElements;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// A custom <see cref="VisualElement"/> implementation that houses an <see cref="SliderInt"/>.
    /// Used in <see cref="BezierMeshOverlay"/> to display parameters.
    /// </summary>
    sealed class IntSlider : VisualElement
    {
        SliderInt m_SliderInt;

        public SliderInt slider => m_SliderInt;

        const string k_ElementStyle = "slider-and-input-field";

        public IntSlider(string val, int min, int max)
        {
            AddToClassList(k_ElementStyle);

            m_SliderInt = new SliderInt(val, min, max);
            m_SliderInt.showInputField = true;
            m_SliderInt.value = min;

            Add(m_SliderInt);
        }
    }
}
#endif
