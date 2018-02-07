using System.Runtime.CompilerServices;
#if PROBUILDER_DLL
[assembly: InternalsVisibleTo("ProBuilderEditor")]
#else
[assembly: InternalsVisibleTo("Unity.ProBuilder.Editor")]
#endif
