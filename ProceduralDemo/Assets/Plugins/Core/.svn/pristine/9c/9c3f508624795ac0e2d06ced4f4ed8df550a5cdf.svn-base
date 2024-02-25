using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace  Act2
{
	public static class ActTreeGraphUtil
	{
		public static void DrawLoop(Rect nodeRect, Color color)
		{
			float size = nodeRect.height;
			size *= 0.75f;
			float border = 0.5f * (nodeRect.height - size);
			Vector2 p0 = new Vector2(nodeRect.xMax, nodeRect.yMin + border);
			Vector2 p1 = p0 + size * Vector2.right;
			Vector2 p2 = p1 + size * Vector2.up;
			Vector2 p3 = p2 + size * Vector2.left;
			GraphEditor.Lines.DrawLine(p0, p1, color);
			GraphEditor.Lines.DrawLine(p1, p2, color);
			GraphEditor.Lines.DrawLine(p2, p3, color);
			DrawArrow(p2, p3, color);
		}

		public static void DrawArrow(Vector2 fromPos, Vector2 toPos, Color color)
		{
			Vector2 dir = toPos - fromPos;
			if (Core.Util.IsVector2Zero(dir))
			{
				return;
			}
			Core.Util.Normalize2(ref dir);
			GraphEditor.Lines.DrawLine(fromPos, toPos, color);
			float angle = Mathf.PI / 5;
			float arrowDist = 8.0f;
			Vector2 d1 = Core.Util.Rotate2D(-dir, angle);
			Vector2 d2 = Core.Util.Rotate2D(-dir, -angle);
			GraphEditor.Lines.DrawLine(toPos, toPos + arrowDist * d1, color);
			GraphEditor.Lines.DrawLine(toPos, toPos + arrowDist * d2, color);
		}
		public static void DrawArrowToRect(Vector2 fromPos, Rect toRect, Color color)
		{
			Vector2 toPos = toRect.center;
			Vector2 dir = toPos - fromPos;
			if (Core.Util.IsVector2Zero(dir))
			{
				return;
			}
			Core.Util.Normalize2(ref dir);
			if (IntersectsAABB(fromPos, dir, toRect, out Vector2 hit))
			{
				toPos = hit;
			}
			DrawArrow(fromPos, toPos, color);
		}

		public static void DrawArrowFromRectToRect(Rect fromRect, Rect toRect, Color color)
		{
			Vector2 toPos = toRect.center;
			Vector2 fromPos = fromRect.center;
			Vector2 dir = toPos - fromPos;
			if (Core.Util.IsVector2Zero(dir))
			{
				return;
			}
			Core.Util.Normalize2(ref dir);
			if (IntersectsAABB(toPos, -dir, fromRect, out Vector2 hit))
			{
				fromPos = hit;
			}
			DrawArrowToRect(fromPos, toRect, color);
		}
		
		public static bool IntersectsAABB(Vector3 origin, Vector3 direction, Rect aabb, out Vector2 hitPoint)
		{
			Vector3 InverseDirection = new Vector3(
				Core.Util.Approximately(direction.x, 0.0f) ? 999999999.0f : 1.0f / direction.x,
				Core.Util.Approximately(direction.y, 0.0f) ? 999999999.0f : 1.0f / direction.y,
				Core.Util.Approximately(direction.z, 0.0f) ? 999999999.0f : 1.0f / direction.z);
			// https://gamedev.stackexchange.com/questions/18436/most-efficient-aabb-vs-ray-collision-algorithms
			// https://tavianator.com/2011/ray_box.html
			float t1 = (aabb.xMin - origin.x) * InverseDirection.x;
			float t2 = (aabb.xMax - origin.x) * InverseDirection.x;
			float t3 = (aabb.yMin - origin.y) * InverseDirection.y;
			float t4 = (aabb.yMax - origin.y) * InverseDirection.y;

			float tmin = Mathf.Max(Mathf.Min(t1, t2), Mathf.Min(t3, t4));
			float tmax = Mathf.Min(Mathf.Max(t1, t2), Mathf.Max(t3, t4));

			// if tmax < 0, ray (line) is intersecting AABB, but the whole AABB is behind us
			if (tmax < 0)
			{
				hitPoint = Vector2.zero;
				return false;
			}
			// if tmin > tmax, ray doesn't intersect AABB
			if (tmin > tmax)
			{
				hitPoint = Vector2.zero;
				return false;
			}
			hitPoint = origin + tmin * direction;
			return true;
		}
		
		public static Vector2 GetDefaultPosition(int index, int nodeCount, bool isParent)
		{
			Vector2 parentPos = 20.0f * Vector2.one;
			if (isParent)
			{
				return parentPos;
			}

			Vector2 center = parentPos;
			float radius = 150.0f;

			center.x += 250.0f;
			center.y += radius;

			float delta = 2.0f * Mathf.PI / (nodeCount - 1);
			float angle = Mathf.PI - delta * index;
			Vector2 dir = Core.Util.Rotate2D(Vector2.up, angle);
			
			Vector2 pos = center + radius * dir;
			return pos;
		}
		
		public static bool TryGetPosition(Node node, bool isParent, out Vector2 pos)
		{
			pos = isParent ? node._GraphPositionParent : node._GraphPositionChild;
			// Kind of a big assumption here, we're assuming any node at 0,0 have not had their positions initialized
			bool isValid = !Core.Util.IsVector2Zero(pos);
			return isValid;
		}

		public static void SetPosition(ActTree2 tree, SerializedObject sTree, int nodeID, bool isParent, Vector2 pos)
		{
			int nodeIndex = tree.GetNodeIndex(nodeID);
			if (nodeIndex < 0)
			{
				return;
			}
			SerializedProperty sNodes = sTree.FindProperty("m_Nodes");
			SerializedProperty sNode = sNodes.GetArrayElementAtIndex(nodeIndex);
			SerializedProperty sPos = sNode.FindPropertyRelative(isParent ? "m_GraphPositionParent" : "m_GraphPositionChild");
			sPos.vector2Value = new Vector2(pos.x, pos.y);
		}

		public static Rect PositionToRect(Vector2 pos, string name)
		{
			GUIContent content = new GUIContent(name);
			Vector2 size = GUI.skin.label.CalcSize(content);
			size.x += 20.0f;
			size.x = Mathf.Max(size.x, 100.0f);
			return new Rect(pos, size);
		}
	}
}

