using UnityEngine.UIElements;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    ///  A custom <see cref="VisualElement"/> implementation that houses an <see cref="Slider"/> and
    /// <see cref="IntegerField"/>. Used in <see cref="BezierMeshTransientOverlay"/> to display parameters.
    /// </summary>
    sealed class SliderAndFloatField : VisualElement
    {
        public Slider m_Slider;
        public FloatField m_FloatField;

        const string k_ElementStyle = "slider-and-input-field";

        public SliderAndFloatField(string val, float min, float max)
        {
            AddToClassList(k_ElementStyle);

            m_Slider = new Slider(val, min, max);
            Add(m_Slider);

            m_FloatField = new FloatField();
            Add(m_FloatField);

            InitSliderAndFloatFields();
        }

        void InitSliderAndFloatFields()
        {
            var lowVal = m_Slider.lowValue;
            var highVal = m_Slider.highValue;

            m_Slider.value = m_FloatField.value = lowVal;

            m_FloatField.RegisterValueChangedCallback(evt =>
            {
                m_FloatField.value = Mathf.Clamp(m_FloatField.value, lowVal, highVal);
                m_Slider.SetValueWithoutNotify(m_FloatField.value);
            });

            m_Slider.RegisterValueChangedCallback(evt =>
            {
                m_FloatField.SetValueWithoutNotify(m_Slider.value);
            });
        }
    }
}
