using UnityEngine.UIElements;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    ///  A custom <see cref="VisualElement"/> implementation that houses an <see cref="SliderInt"/> and
    /// <see cref="FloatField"/>. Used in <see cref="BezierMeshTransientOverlay"/> to display parameters.
    /// </summary>
    sealed class SliderAndIntegerField : VisualElement
    {
        public SliderInt m_SliderInt;
        public IntegerField m_IntField;

        const string k_ElementStyle = "slider-and-input-field";

        public SliderAndIntegerField(string val, float min, float max)
        {
            AddToClassList(k_ElementStyle);

            m_SliderInt = new SliderInt(val, (int)min, (int)max);
            Add(m_SliderInt);

            m_IntField = new IntegerField();
            Add(m_IntField);

            InitSliderAndIntegerFields();
        }

        void InitSliderAndIntegerFields()
        {
            var lowVal = m_SliderInt.lowValue;
            var highVal = m_SliderInt.highValue;

            m_SliderInt.value = m_IntField.value = lowVal;

            m_IntField.RegisterValueChangedCallback(evt =>
            {
                m_IntField.value = Mathf.Clamp(m_IntField.value, lowVal, highVal);
                m_SliderInt.SetValueWithoutNotify(m_IntField.value);
            });

            m_SliderInt.RegisterValueChangedCallback(evt =>
            {
                m_IntField.SetValueWithoutNotify(m_SliderInt.value);
            });
        }
    }
}
