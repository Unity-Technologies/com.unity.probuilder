using System;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    // The menu bar entry for Extrude delegates to Face/Edge as appropriate.
    sealed class Extrude : MenuAction
    {
        public override ToolbarGroup @group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override string iconPath => String.Empty;
        public override Texture2D icon => null;

        static readonly TooltipContent s_Tooltip = new TooltipContent
        (
            "Extrude", "",
            keyCommandSuper, 'E'
        );

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Face | SelectMode.Edge; }
        }

        public override bool enabled
        {
            get
            {
                switch (ProBuilderEditor.selectMode)
                {
                    case SelectMode.Edge:
                        return EditorToolbarLoader.GetInstance<ExtrudeEdges>().enabled;
                    case SelectMode.Face:
                        return EditorToolbarLoader.GetInstance<ExtrudeFaces>().enabled;
                    default:
                        return false;
                }
            }
        }

        public override bool hidden
        {
            get { return true; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            switch (ProBuilderEditor.selectMode)
            {
                case SelectMode.Edge:
                    return EditorToolbarLoader.GetInstance<ExtrudeEdges>().PerformAction();
                case SelectMode.Face:
                    return EditorToolbarLoader.GetInstance<ExtrudeFaces>().PerformAction();

                default:
                    return ActionResult.NoSelection;
            }
        }

        internal override string GetMenuItemOverride()
        {
            return @"                switch (ProBuilderEditor.selectMode)
                {
                    case SelectMode.Edge:
                        EditorAction.Start(new MenuActionSettings(EditorToolbarLoader.GetInstance<ExtrudeEdges>(), true));
                        break;
                    case SelectMode.Face:
                        EditorAction.Start(new MenuActionSettings(EditorToolbarLoader.GetInstance<ExtrudeFaces>(), true));
                        break;
                }";
        }
    }
}
