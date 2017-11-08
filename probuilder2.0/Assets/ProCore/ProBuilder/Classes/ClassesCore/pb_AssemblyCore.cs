#if PROBUILDER_DLL
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ProBuilderEditor-Unity5")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ProBuilderMeshOps-Unity5")]
#else
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Assembly-CSharp-Editor")]
#endif