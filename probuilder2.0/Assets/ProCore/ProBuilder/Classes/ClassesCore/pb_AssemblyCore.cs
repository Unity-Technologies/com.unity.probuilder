
#if PROBUILDER_DLL
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ProBuilderEditor")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("ProBuilderMeshOps")]
#else
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Assembly-CSharp-Editor")]
#endif