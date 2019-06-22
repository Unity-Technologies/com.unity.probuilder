using UnityEditor.EditorTools;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    [EditorTool("Edit Cube", typeof(Cube))]
    public class EditCubeTool : EditShapeTool<Cube>
    {
    }
}
