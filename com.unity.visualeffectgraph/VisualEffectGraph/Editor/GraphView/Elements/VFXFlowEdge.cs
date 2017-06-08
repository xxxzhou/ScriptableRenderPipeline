﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UIElements.GraphView;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.VFX.UI
{
    public class VFXFlowEdge : Edge
    {
        public override int layer
        {
            get
            {
                return 1;
            }
        }


        public VFXFlowEdge()
        {
        }

        protected override void DrawEdge()
        {
            var edgePresenter = GetPresenter<EdgePresenter>();

            NodeAnchorPresenter outputPresenter = edgePresenter.output;
            NodeAnchorPresenter inputPresenter = edgePresenter.input;

            if (outputPresenter == null && inputPresenter == null)
                return;

            Vector2 from = Vector2.zero;
            Vector2 to = Vector2.zero;
            GetFromToPoints(ref from, ref to);


            float arrowHeight = 12;
            to -= Vector2.up * arrowHeight;

            Color edgeColor = edgePresenter.selected ? new Color(240 / 255f, 240 / 255f, 240 / 255f) : new Color(146 / 255f, 146 / 255f, 146 / 255f);

            Orientation orientation = outputPresenter != null ? outputPresenter.orientation : inputPresenter.orientation;

            Vector3[] points, tangents;
            GetTangents(orientation, from, to, out points, out tangents);

            RenderBezier(points[0], points[1], tangents[0], tangents[1], edgeColor, edgeWidth);
            RenderDisc(points[1], 4, edgeColor);

            VCircleMat.SetPass(0);
            GL.Begin(GL.TRIANGLES);
            GL.Color(edgeColor);
            GL.Vertex3(to.x - arrowHeight * .5f, to.y, 0);
            GL.Vertex3(to.x + arrowHeight * .5f, to.y, 0);
            GL.Vertex3(to.x, to.y + arrowHeight, 0);
            GL.End();
        }

        static Material VLineMat;
        static Material VCircleMat;

        // Only flow anchors are of interest to flow edges
        public override IEnumerable<NodeAnchor> GetAllAnchors(bool input, bool output)
        {
            foreach (var anchor in this.GetFirstOfType<VFXView>().GetAllFlowAnchors(input, output))
                yield return anchor;
        }

        void RenderDisc(Vector2 center, float radius, Color color)
        {
            if (VCircleMat == null)
            {
                VCircleMat = new Material(Shader.Find("Unlit/VColor"));
            }
            VCircleMat.SetPass(0);
            GL.Begin(GL.TRIANGLE_STRIP);
            GL.Color(color);

            float prec = 5;


            GL.Vertex3(center.x - radius, center.y, 0);

            for (float f = -prec + 1; f < prec; f += 1)
            {
                float phi = f * Mathf.PI / (2.0f * prec);

                float x = center.x + Mathf.Sin(phi) * radius;
                float y = Mathf.Cos(phi) * radius;

                //GL.TexCoord3(f,)
                GL.Vertex3(x, center.y - y, 0);
                GL.Vertex3(x, center.y + y, 0);
            }

            GL.Vertex3(center.x + radius, center.y, 0);

            GL.End();
        }

        public static void RenderLine(Vector2 start, Vector2 end,Color color,float edgeWidth)
        {
            if (VLineMat == null)
            {
                VLineMat = new Material(Shader.Find("Unlit/AALine"));
            }
            VLineMat.SetPass(0);
            GL.Begin(GL.TRIANGLE_STRIP);
            GL.Color(color);

            Vector2 dir = (end - start).normalized;
            Vector2 norm = new Vector2(dir.y, -dir.x);

            float halfWidth = edgeWidth * 0.5f + 0.5f;

            float vertexHalfWidth = halfWidth + 2;
            Vector2 edge = norm * vertexHalfWidth;

            GL.TexCoord3(0, -vertexHalfWidth, halfWidth);
            GL.Vertex(start - edge);
            GL.TexCoord3(0, vertexHalfWidth, halfWidth);
            GL.Vertex(start + edge);

            GL.TexCoord3(1, -vertexHalfWidth, halfWidth);
            GL.Vertex(end - edge);
            GL.TexCoord3(1, vertexHalfWidth, halfWidth);
            GL.Vertex(end + edge);

            GL.End();
        }

        public static void RenderBezier(Vector2 start, Vector2 end, Vector2 tStart, Vector2 tEnd, Color color, float edgeWidth)
        {
            if (VLineMat == null)
            {
                VLineMat = new Material(Shader.Find("Unlit/AALine"));
            }
            VLineMat.SetPass(0);
            GL.Begin(GL.TRIANGLE_STRIP);
            GL.Color(color);

            Vector2 prevPos = start;
            Vector2 edge = Vector2.zero;
            Vector2 dir = (tStart - start).normalized;
            Vector2 norm = new Vector2(dir.y, -dir.x);
            //tStart = start + tStart;
            //tEnd +=  end + tEnd;

            //GL.Vertex(start);

            float cpt = (start - end).magnitude / 5;
            if (cpt < 3)
                cpt = 3;


            float halfWidth = edgeWidth * 0.5f + 0.5f;

            float vertexHalfWidth = halfWidth + 2;

            for (float t = 1 / cpt; t < 1; t += 1 / cpt)
            {
                float minT = 1 - t;

                Vector2 pos = t * t * t * end +
                    3 * minT * t * t * tEnd +
                    3 * minT * minT * t * tStart +
                    minT * minT * minT * start;

                edge = norm * vertexHalfWidth;

                GL.TexCoord3(t, -vertexHalfWidth, halfWidth);
                GL.Vertex(prevPos - edge);
                GL.TexCoord3(t, vertexHalfWidth, halfWidth);
                GL.Vertex(prevPos + edge);

                dir = (pos - prevPos).normalized;
                norm = new Vector2(dir.y, -dir.x);

                prevPos = pos;
            }

            dir = (end - prevPos).normalized;
            norm = new Vector2(dir.y, -dir.x);
            edge = norm * vertexHalfWidth;

            GL.TexCoord3(1, -vertexHalfWidth, halfWidth);
            GL.Vertex(end - edge);
            GL.TexCoord3(1, vertexHalfWidth, halfWidth);
            GL.Vertex(end + edge);

            GL.End();
        }

        /*
        protected override void DrawEdge(IStylePainter painter)
        {
            var edgePresenter = GetPresenter<EdgePresenter>();

            NodeAnchorPresenter outputPresenter = edgePresenter.output;
            NodeAnchorPresenter inputPresenter = edgePresenter.input;

            if (outputPresenter == null && inputPresenter == null)
                return;

            Vector2 from = Vector2.zero;
            Vector2 to = Vector2.zero;
            GetFromToPoints(ref from, ref to);

            Color edgeColor = edgePresenter.selected ? new Color(240/255f,240/255f,240/255f) : new Color(146/255f,146/255f,146/255f);

            Orientation orientation = Orientation.Vertical;
            Vector3[] points, tangents;
            GetTangents(orientation, from, to, out points, out tangents);
            Handles.DrawBezier(points[0], points[1], tangents[0], tangents[1], edgeColor, null, 15f);

        }
        */
    }
}
