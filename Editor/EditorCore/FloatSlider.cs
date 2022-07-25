using UnityEngine.UIElements;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// A custom <see cref="VisualElement"/> implementation that houses an <see cref="Slider"/>.
    /// Used in <see cref="BezierMeshOverlay"/> to display parameters.
    /// </summary>
    sealed class FloatSlider : VisualElement
    {
        Slider m_Slider;

        public Slider slider => m_Slider;

        const string k_ElementStyle = "slider-and-input-field";

        public FloatSlider(string val, float min, float max)
        {
            AddToClassList(k_ElementStyle);

            m_Slider = new Slider(val, min, max);
            m_Slider.showInputField = true;
            m_Slider.value = min;

            Add(m_Slider);
        }
    }
}
