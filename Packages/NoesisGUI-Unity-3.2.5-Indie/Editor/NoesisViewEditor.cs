// using System;
// using System.Text;
// using System.Linq;
// using UnityEditor;
// using UnityEngine;
//
// #if ENABLE_URP_PACKAGE
// using UnityEngine.Rendering.Universal;
// #endif
//
// [CustomEditor(typeof(NoesisMonoRender))]
// public class NoesisViewEditor : Editor
// {
//     enum RenderMode
//     {
//         None,
//         Wireframe,
//         Batches,
//         Overdraw
//     };
//
//     private RenderMode ToRenderMode(Noesis.RenderFlags renderFlags)
//     {
//         if ((renderFlags & Noesis.RenderFlags.Wireframe) > 0)
//         {
//             return RenderMode.Wireframe;
//         }
//         else if ((renderFlags & Noesis.RenderFlags.ColorBatches) > 0)
//         {
//             return RenderMode.Batches;
//         }
//         else if ((renderFlags & Noesis.RenderFlags.Overdraw) > 0)
//         {
//             return RenderMode.Overdraw;
//         }
//         else
//         {
//             return RenderMode.None;
//         }
//     }
//
//     private Noesis.RenderFlags ToRenderFlags(RenderMode renderMode)
//     {
//         if (renderMode == RenderMode.Wireframe)
//         {
//             return Noesis.RenderFlags.Wireframe;
//         }
//         else if (renderMode == RenderMode.Batches)
//         {
//             return Noesis.RenderFlags.ColorBatches;
//         }
//         else if (renderMode == RenderMode.Overdraw)
//         {
//             return Noesis.RenderFlags.Overdraw;
//         }
//
//         return 0;
//     }
//
//     private GUIStyle _matchedActionStyle;
//     private GUIStyle _unmatchedActionStyle;
//
//     public override void OnInspectorGUI()
//     {
//         NoesisMonoRender monoRender = target as NoesisMonoRender;
//         Noesis.RenderFlags renderFlags = monoRender.RenderFlags;
//
//         // Register changes in the component so scene can be saved, and Undo is also enabled
//         Undo.RecordObject(monoRender, "Noesis View");
//
//         EditorGUILayout.LabelField(new GUIContent("Render Mode", "Views attached to camera objects work in 'Camera Overlay' mode. 'Render Texture' mode is enabled in all other cases"),
//             new GUIContent(monoRender.IsRenderToTexture() ? "Render Texture" : "Camera Overlay"), EditorStyles.popup);
//
//         if (monoRender.IsRenderToTexture())
//         {
//             monoRender.ContinuousRendering = EditorGUILayout.Toggle(new GUIContent("Continuous Rendering", "When continuous rendering is disabled, rendering only happens when UI changes." +
//                 " For performance purposes and to save battery this is the default mode when rendering to texture.\n\nThis flag is not available in 'Camera Overlay' mode and instead the property " +
//                 "NoesisView.NeedsRendering must be used with a manually repainted camera."), monoRender.ContinuousRendering);
//         }
//         else
//         {
//           #if ENABLE_URP_PACKAGE
//             view.RenderPassEvent = (RenderPassEvent)EditorGUILayout.EnumPopup(new GUIContent("Injection Point", 
//                 "Controls when the UI render pass executes"), view.RenderPassEvent);
//           #endif
//
//             bool depthTesting = EditorGUILayout.Toggle(new GUIContent("World Space UI",
//                 "World Space UI is positioned in the world among other objects in the Scene"
//                 ), (renderFlags & Noesis.RenderFlags.DepthTesting) > 0);
//             renderFlags = (renderFlags & ~Noesis.RenderFlags.DepthTesting) | (depthTesting ? Noesis.RenderFlags.DepthTesting : 0);
//         }
//
//         EditorGUILayout.Space();
//
//         EditorGUILayout.BeginHorizontal();
//         GUIContent[] options = { new GUIContent("Low Quality"), new GUIContent("Medium Quality"), new GUIContent("High Quality"), new GUIContent("Custom Quality") };
//         int[] values = { 0, 1, 2, 3};
//         float inMaxError = monoRender.TessellationMaxPixelError;
//         int value = inMaxError == 0.7f ? 0 : inMaxError == 0.4f ? 1 : inMaxError == 0.2f ? 2 : 3;
//         value = EditorGUILayout.IntPopup(new GUIContent("Tessellation Pixel Error", "Tessellation curve tolerance in screen space. " + 
//             "'Medium Quality' is usually fine for PPAA (non-multisampled) while 'High Quality' is the recommended pixel error if you are rendering to a 8x multisampled surface"),
//             value, options, values);
//         float outMaxError = value == 0 ? 0.7f : value == 1 ? 0.4f : value == 2 ? 0.2f: inMaxError;
//         monoRender.TessellationMaxPixelError = Math.Max(0.01f, EditorGUILayout.FloatField(outMaxError, GUILayout.Width(64)));
//         EditorGUILayout.EndHorizontal();
//
//         RenderMode renderMode = (RenderMode)EditorGUILayout.EnumPopup(new GUIContent("Debug Render Flags",
//             "Enables debugging render flags." + 
//             "\n\n- Wireframe: toggles wireframe mode when rendering triangles" + 
//             "\n\n- ColorBatches: each batch submitted to the GPU is given a unique solid color" + 
//             "\n\n- Overdraw: displays pixel overdraw using blending layers. Different colors are used for each type of triangles." +
//                 "'Green' for normal ones, 'Red' for opacities and 'Blue' for clipping masks"
//             ), ToRenderMode(renderFlags));
//         renderFlags = (renderFlags & ~(Noesis.RenderFlags.Wireframe | Noesis.RenderFlags.ColorBatches | Noesis.RenderFlags.Overdraw))
//             | ToRenderFlags(renderMode);
//
//         bool ppaa = EditorGUILayout.Toggle(new GUIContent("Enable PPAA", 
//             "Per-Primitive Antialiasing extrudes the contours of the geometry and smooths them. " +
//             "It is a 'cheap' antialiasing algorithm useful when GPU MSAA is not enabled.\n\n" +
//             "Not recommended when using World Space UI or Transform3D."),
//             (renderFlags & Noesis.RenderFlags.PPAA) > 0);
//         renderFlags = (renderFlags & ~Noesis.RenderFlags.PPAA) | (ppaa ? Noesis.RenderFlags.PPAA : 0);
//
//         bool lcd = EditorGUILayout.Toggle(new GUIContent("Subpixel Rendering",
//             "Enables subpixel rendering compatible with LCD displays"),
//             (renderFlags & Noesis.RenderFlags.LCD) > 0);
//         renderFlags = (renderFlags & ~Noesis.RenderFlags.LCD) | (lcd ? Noesis.RenderFlags.LCD : 0);
//
//         monoRender.RenderFlags = renderFlags;
//
//         monoRender.DPIScale = EditorGUILayout.Toggle(new GUIContent("DPI Scale",
//             "When enabled, the view is scaled by the actual DPI of the screen or physical device running the application"),
//             monoRender.DPIScale);
//
//         monoRender.ClearStencil = EditorGUILayout.Toggle(new GUIContent("Clear Stencil Buffer",
//             "(Experimental)\nNoesis requires the stencil buffer to be cleared to zero at the start of each frame. " +
//             "If Unity is using the stencil buffer somewhere else, you must enable this option to force a clear. " +
//             "Note that enabling this can have a minor performance impact, so it should be left disabled if not necessary."),
//             monoRender.ClearStencil);
//
//         EditorGUILayout.Space();
//         
//         monoRender.UseRealTimeClock = EditorGUILayout.Toggle(new GUIContent("Real Time Clock",
//             "Indicates if 'Time.realtimeSinceStartup' is used instead of 'Time.time' for animations"), monoRender.UseRealTimeClock);
//
//         EditorGUILayout.Space();
//
//         monoRender.EnableKeyboard = EditorGUILayout.Toggle(new GUIContent("Enable Keyboard",
//             "Indicates if keyboard input events are processed by this view"), monoRender.EnableKeyboard);
//
//         monoRender.EnableTouch = EditorGUILayout.Toggle(new GUIContent("Enable Touch",
//             "Indicates if touch input events are processed by this view"), monoRender.EnableTouch);
//
//         EditorGUILayout.Space();
//
//         EditorGUILayout.Space();
//     }
//
//     public override bool HasPreviewGUI()
//     {
//         return Application.isPlaying;
//     }
//
//     public override bool RequiresConstantRepaint()
//     {
//         return Application.isPlaying;
//     }
//
//     private GUIStyle _previewHeaderStyle;
//     private GUIStyle _previewTextStyle;
//
//     public override void OnPreviewGUI(Rect rect_, GUIStyle background)
//     {
//         NoesisMonoRender monoRender = target as NoesisMonoRender;
//         Noesis.ViewStats stats = monoRender.GetStats();
//
//         if (_previewHeaderStyle == null)
//         {
//             _previewHeaderStyle = new GUIStyle(EditorStyles.largeLabel)
//             {
//                 alignment = TextAnchor.UpperLeft,
//                 fontStyle = FontStyle.Bold
//             };
//         }
//         
//         if (_previewTextStyle == null)
//         {
//             _previewTextStyle = new GUIStyle(EditorStyles.label)
//             {
//                 richText = true,
//                 fontStyle = FontStyle.Normal
//             };
//         }
//
//         StringBuilder left = new StringBuilder();
//         left.AppendLine("\n\nFrame Time (ms)");
//         left.AppendLine("Update Time (ms)");
//         left.AppendLine("Render Time (ms)");
//         left.AppendLine();
//         left.AppendLine("Triangles");
//         left.AppendLine("Draws");
//         left.AppendLine("Batches");
//         left.AppendLine("Tessellations");
//         left.AppendLine("Geometry Size (kB)");
//         left.AppendLine("Flushes");
//         left.AppendLine();
//         left.AppendLine("Stencil Masks");
//         left.AppendLine("Opacity Groups");
//         left.AppendLine("RT Switches");
//         left.AppendLine();
//         left.AppendLine("Ramps Uploaded");
//         left.AppendLine("Rasterized Glyphs");
//         left.AppendLine("Discarded Glyph Tiles");
//
//         _previewTextStyle.alignment = TextAnchor.UpperLeft;
//         GUI.Label(new Rect(rect_.x + 15, rect_.y + 5, 220, 500), left.ToString(), _previewTextStyle);
//
//         var format = new System.Globalization.NumberFormatInfo { NumberDecimalSeparator = "." };
//
//         StringBuilder right = new StringBuilder();
//         right.AppendLine("\n\n<b>" + stats.FrameTime.ToString("#,##0.00", format) + "</b>");
//         right.AppendLine("<b>" + stats.UpdateTime.ToString("#,##0.00", format) + "</b>");
//         right.AppendLine("<b>" + stats.RenderTime.ToString("#,##0.00", format) + "</b>");
//         right.AppendLine();
//         right.AppendLine("<b>" + stats.Triangles + "</b>");
//         right.AppendLine("<b>" + stats.Draws + "</b>");
//         right.AppendLine("<b>" + stats.Batches + "</b>");
//         right.AppendLine("<b>" + stats.Tessellations + "</b>");
//         right.AppendLine("<b>" + String.Format("{0:F0}", stats.GeometrySize / 1024) + "</b>");
//         right.AppendLine("<b>" + stats.Flushes + "</b>");
//         right.AppendLine();
//         right.AppendLine("<b>" + stats.Masks + "</b>");
//         right.AppendLine("<b>" + stats.Opacities + "</b>");
//         right.AppendLine("<b>" + stats.RenderTargetSwitches + "</b>");
//         right.AppendLine();
//         right.AppendLine("<b>" + stats.UploadedRamps + "</b>");
//         right.AppendLine("<b>" + stats.RasterizedGlyphs + "</b>");
//         right.AppendLine("<b>" + stats.DiscardedGlyphTiles + "</b>");
//
//         _previewTextStyle.alignment = TextAnchor.UpperRight;
//         GUI.Label(new Rect(rect_.x + 15, rect_.y + 5, 220, 500), right.ToString(), _previewTextStyle);
//     }
// }
