using NUnit.Framework;
using UnityEngine;
using UnityEngine.ProBuilder;

class TransformUtilityTests
{
    const int k_ChildCount = 4;

    Transform m_Parent;
    Transform[] m_Children;

    [SetUp]
    public void Setup()
    {
        m_Parent = new GameObject("Parent").transform;
        m_Children = new Transform[k_ChildCount];

        for (int i = 0; i < k_ChildCount; ++i)
        {
            Transform child = new GameObject($"Child {i}").transform;
            child.parent = m_Parent;
            m_Children[i] = child;
        }
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(m_Parent.gameObject);

        //Ensure that children are destroyed if they are not reparented
        foreach (var child in m_Children)
        {
            if (child != null)
                Object.DestroyImmediate(child.gameObject);
        }
    }

    [Test]
    public void UnparentChildren_HasZeroChildren()
    {
        Assume.That(m_Parent.childCount, Is.EqualTo(k_ChildCount));

        TransformUtility.UnparentChildren(m_Parent);
        Assert.That(m_Parent.childCount, Is.EqualTo(0));

        TransformUtility
            .ReparentChildren(m_Parent); //This call is to ensure that we clear the child dictionary created by unparent
    }

    [Test]
    public void UnparentChildrenAndReparentChildren_HasSameAmountOfChildren()
    {
        Assume.That(m_Parent.childCount, Is.EqualTo(k_ChildCount));
        TransformUtility.UnparentChildren(m_Parent);
        Assume.That(m_Parent.childCount, Is.EqualTo(0));

        TransformUtility.ReparentChildren(m_Parent);
        Assert.That(m_Parent.childCount, Is.EqualTo(k_ChildCount));
    }

    [Test]
    public void UnparentChildrenAndReparentChildren_ChildrenAreInSameOrder()
    {
        Assume.That(m_Parent.childCount, Is.EqualTo(k_ChildCount));
        TransformUtility.UnparentChildren(m_Parent);
        Assume.That(m_Parent.childCount, Is.EqualTo(0));

        TransformUtility.ReparentChildren(m_Parent);
        Assume.That(m_Parent.childCount, Is.EqualTo(k_ChildCount));

        for (int i = 0; i < k_ChildCount; ++i)
        {
            Assert.That(m_Parent.GetChild(i), Is.EqualTo(m_Children[i]));
        }
    }
}
