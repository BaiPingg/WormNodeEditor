using UnityEditor;
using UnityEngine;
namespace wormNode
{
    public static class EditorResources
    {
        // Textures
        public static Texture2D dot { get { return _dot != null ? _dot : _dot = Resources.Load<Texture2D>("port"); } }
        private static Texture2D _dot;
        public static Texture2D dotOuter { get { return _dotOuter != null ? _dotOuter : _dotOuter = Resources.Load<Texture2D>("wormnode_dot_outer"); } }
        private static Texture2D _dotOuter;
        public static Texture2D nodeBody { get { return _nodeBody != null ? _nodeBody : _nodeBody = Resources.Load<Texture2D>("wormnode_node"); } }
        private static Texture2D _nodeBody;
        public static Texture2D nodeHighlight { get { return _nodeHighlight != null ? _nodeHighlight : _nodeHighlight = Resources.Load<Texture2D>("wormnode_node_highlight"); } }
        private static Texture2D _nodeHighlight;

        // Styles
        public static Styles styles { get { return _styles != null ? _styles : _styles = new Styles(); } }
        public static Styles _styles = null;
        public static GUIStyle OutputPort { get { return new GUIStyle(EditorStyles.label) { alignment = TextAnchor.UpperRight }; } }
        public class Styles
        {
            public GUIStyle inPortStyle, inPortStyleCircle, nodeHeader, normalNodeStyle, highLightNodeStyle, tooltip, horizontalLine;

            public GUIStyle outPortStyle,contentStyle,fieldInfoStyle,titleStyle;
            public Styles()
            {
                GUIStyle baseStyle = new GUIStyle("Label");
                baseStyle.fontSize = WormNodeGlobalSettings.GetOrCreateSettings().contetntSize;

                //leftOffstyle = new GUIStyle();
                //leftOffstyle.clipping = TextClipping.Overflow;
                ////leftOffstyle.contentOffset = new Vector2(-20, 0);
                ////leftOffstyle.padding = new RectOffset(-10, 0, 0, 0);
                fieldInfoStyle = new GUIStyle("Label");
                fieldInfoStyle.normal.background = nodeBody;

                contentStyle = new GUIStyle("Label");
                contentStyle.alignment = TextAnchor.MiddleRight;
                //contentStyle.fontSize = WormNodeGlobalSettings.GetOrCreateSettings().contetntSize;
              

                titleStyle = new GUIStyle("Label");
                titleStyle.alignment = TextAnchor.MiddleCenter;
            
                //titleStyle.fontSize = WormNodeGlobalSettings.GetOrCreateSettings().titleSize;


                inPortStyle = new GUIStyle();
              
                inPortStyle.normal.background = dot;
                inPortStyle.fixedWidth = 16;
                inPortStyle.fixedHeight = 16;

                inPortStyleCircle = new GUIStyle();
                inPortStyleCircle.alignment = TextAnchor.MiddleCenter;
                inPortStyleCircle.normal.background = dotOuter;
                

                outPortStyle = new GUIStyle();
                outPortStyle.alignment = TextAnchor.MiddleRight;
                outPortStyle.normal.background = dot;
                //outPortStyle.fixedHeight = 10;
                //outPortStyle.fixedWidth = 10;
              

                nodeHeader = new GUIStyle();
                nodeHeader.alignment = TextAnchor.MiddleCenter;
                nodeHeader.fontStyle = FontStyle.Bold;
                nodeHeader.normal.textColor = Color.white;

                normalNodeStyle = new GUIStyle();
                normalNodeStyle.border = new RectOffset(32, 32, 32, 32);
                //normalNodeStyle.padding = new RectOffset(16, 16, 8, 16);
                normalNodeStyle.normal.background = nodeBody;
                //normalNodeStyle.clipping = TextClipping.Overflow;

                highLightNodeStyle = new GUIStyle();
                highLightNodeStyle.normal.background = nodeHighlight;
                highLightNodeStyle.border = new RectOffset(32, 32, 32, 32);
                //highLightNodeStyle.padding = new RectOffset(16, 16, 8, 16);

                tooltip = new GUIStyle("helpBox");
                tooltip.alignment = TextAnchor.MiddleCenter;

               
                horizontalLine = new GUIStyle();
                horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
                horizontalLine.margin = new RectOffset(0, 0, 4, 4);
                horizontalLine.fixedHeight = 1;
            }
        }
       public static void HorizontalLine(Color color)
        {
            var c = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, styles.horizontalLine);
            GUI.color = c;
        }

    }
}