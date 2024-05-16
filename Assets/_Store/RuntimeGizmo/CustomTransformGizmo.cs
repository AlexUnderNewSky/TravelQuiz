using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using CommandUndoRedo;

namespace RuntimeGizmos
{
	//To be safe, if you are changing any transforms hierarchy, such as parenting an object to something,
	//you should call ClearTargets before doing so just to be sure nothing unexpected happens... as well as call UndoRedoManager.Clear()
	//For example, if you select an object that has children, move the children elsewhere, deselect the original object, then try to add those old children to the selection, I think it wont work.

	[RequireComponent(typeof(Camera))]
	public class CustomTransformGizmo : MonoBehaviour
	{
		public TransformSpace space = TransformSpace.Global;
		public TransformPivot pivot = TransformPivot.Pivot;

		//These are the same as the unity editor hotkeys
		public KeyCode ActionKey = KeyCode.LeftShift; //Its set to shift instead of control so that while in the editor we dont accidentally undo editor changes =/
		public KeyCode UndoAction = KeyCode.Z;
		public KeyCode RedoAction = KeyCode.Y;

		public Color xColor = new Color(1, 0, 0, 0.8f);
		public Color yColor = new Color(0, 1, 0, 0.8f);
		public Color zColor = new Color(0, 0, 1, 0.8f);
		public Color allColor = new Color(.7f, .7f, .7f, 0.8f);
		public Color selectedColor = new Color(1, 1, 0, 0.8f);
		public Color hoverColor = new Color(1, .75f, 0, 0.8f);

		public float handleLength = .25f;
		public float circleRadius = .25f;
		public float handleWidth = .003f;
		public float triangleSize = .03f;
		public int circleDetail = 40;
		public float minSelectedDistanceCheck = .04f;
		public float moveSpeedMultiplier = 1f;
		public float rotateSpeedMultiplier = 200f;

		public int maxUndoStored = 100;

		AxisVectors handleLines = new AxisVectors();
		AxisVectors handleTriangles = new AxisVectors();
		AxisVectors circlesLines = new AxisVectors();
		AxisVectors drawCurrentCirclesLines = new AxisVectors();
		
		bool isTransforming;
		Quaternion totalRotationAmount;
		Axis nearAxis = Axis.None;
		AxisInfo axisInfo;

		Vector3 pivotPoint;

		public Transform mainTargetRoot;

		Camera myCamera;


		static Material lineMaterial;
		static Material outlineMaterial;

		public bool highPrecisionMode = false;
		public Transform transformTarget;

		private Vector3 cahcedPos = Vector3.zero;
		private Quaternion cachedRot = Quaternion.identity;


		[SerializeField] private Toggle precisionSwitch;
		[SerializeField] private float highPrecisionMult = 0.1f;


		public void switchPrecision (Toggle toggle) {
			highPrecisionMode = toggle.isOn;
		}

		public void resetTransform ()
		{
			transformTarget.SetPositionAndRotation(cahcedPos, cachedRot);
			SetPivotPoint();
		}

		public void cacheTransform ()
		{
			cahcedPos = transformTarget.position;
			cachedRot = transformTarget.rotation;
		}

		void Awake()
		{
			myCamera = GetComponent<Camera>();
			SetMaterial();
		}

		void OnEnable() {
			SetPivotPoint();
			cacheTransform ();

			if(precisionSwitch)
				precisionSwitch.onValueChanged.AddListener(delegate {
					switchPrecision(precisionSwitch);
            });
		}

		void OnDisable() {
			if(precisionSwitch)
				precisionSwitch.onValueChanged.RemoveAllListeners();
		}

		void Update()
		{
			HandleUndoRedo();

			SetNearAxis();

			if(mainTargetRoot == null) return;
			
			TransformSelected();
		}

		void LateUpdate()
		{
			if(mainTargetRoot == null) return;

			//We run this in lateupdate since coroutines run after update and we want our gizmos to have the updated target transform position after TransformSelected()
			SetAxisInfo();
			SetLines();
		}

		void OnPostRender()
		{
			if(mainTargetRoot == null) return;

			lineMaterial.SetPass(0);

			Color xColor = (nearAxis == Axis.X) ? (isTransforming) ? selectedColor : hoverColor : this.xColor;
			Color yColor = (nearAxis == Axis.Y) ? (isTransforming) ? selectedColor : hoverColor : this.yColor;
			Color zColor = (nearAxis == Axis.Z) ? (isTransforming) ? selectedColor : hoverColor : this.zColor;
			Color allColor = (nearAxis == Axis.Any) ? (isTransforming) ? selectedColor : hoverColor : this.allColor;

			//Note: The order of drawing the axis decides what gets drawn over what.

			DrawQuads(handleLines.z, zColor);
			DrawQuads(handleLines.x, xColor);
			DrawQuads(handleLines.y, yColor);

			DrawTriangles(handleTriangles.x, xColor);
			DrawTriangles(handleTriangles.y, yColor);
			DrawTriangles(handleTriangles.z, zColor);

			AxisVectors rotationAxisVector = circlesLines;
			if(isTransforming && space == TransformSpace.Global)
			{
				rotationAxisVector = drawCurrentCirclesLines;

				AxisInfo axisInfo = new AxisInfo();
				axisInfo.xDirection = totalRotationAmount * Vector3.right;
				axisInfo.yDirection = totalRotationAmount * Vector3.up;
				axisInfo.zDirection = totalRotationAmount * Vector3.forward;
				SetCircles(axisInfo, drawCurrentCirclesLines);
			}

			DrawQuads(rotationAxisVector.all, allColor);
			DrawQuads(rotationAxisVector.x, xColor);
			DrawQuads(rotationAxisVector.y, yColor);
			DrawQuads(rotationAxisVector.z, zColor);
		}

		void HandleUndoRedo()
		{
			if(maxUndoStored != UndoRedoManager.maxUndoStored) { UndoRedoManager.maxUndoStored = maxUndoStored; }

			if(Input.GetKey(ActionKey))
			{
				if(Input.GetKeyDown(UndoAction))
				{
					UndoRedoManager.Undo();
				}
				else if(Input.GetKeyDown(RedoAction))
				{
					UndoRedoManager.Redo();
				}
			}
		}

		void TransformSelected()
		{
			if(mainTargetRoot != null)
			{
				if(nearAxis != Axis.None && Input.GetMouseButtonDown(0))
				{
					StartCoroutine(TransformCoro());
				}
			}
		}
		
		IEnumerator TransformCoro()
		{
			isTransforming = true;
			totalRotationAmount = Quaternion.identity;

			Vector3 originalPivot = pivotPoint;

			Vector3 planeNormal = (transform.position - originalPivot).normalized;
			Vector3 axis = GetNearAxisDirection();
			Vector3 projectedAxis = Vector3.ProjectOnPlane(axis, planeNormal).normalized;
			Vector3 previousMousePosition = Vector3.zero;

			List<ICommand> transformCommands = new List<ICommand>();


			while(!Input.GetMouseButtonUp(0))
			{
				Ray mouseRay = myCamera.ScreenPointToRay(Input.mousePosition);
				Vector3 mousePosition = Geometry.LinePlaneIntersect(mouseRay.origin, mouseRay.direction, originalPivot, planeNormal);

				float mult = 1f;
				if(highPrecisionMode)
					mult = highPrecisionMult;


				if(previousMousePosition != Vector3.zero && mousePosition != Vector3.zero)
				{

					if(nearAxis == Axis.Y)
					{
						float moveAmount = ExtVector3.MagnitudeInDirection(mousePosition - previousMousePosition, projectedAxis) * moveSpeedMultiplier * mult;
						Vector3 movement = axis * moveAmount;


						transformTarget.Translate(movement, Space.World);


						SetPivotPointOffset(movement);
					}
					else // if(type == TransformType.Rotate)
					{
						float rotateAmount = 0;
						Vector3 rotationAxis = axis;

						Vector3 projected = (nearAxis == Axis.Any || ExtVector3.IsParallel(axis, planeNormal)) ? planeNormal : Vector3.Cross(axis, planeNormal);
						rotateAmount = (ExtVector3.MagnitudeInDirection(mousePosition - previousMousePosition, projected) * rotateSpeedMultiplier * mult) / GetDistanceMultiplier();


						if(pivot == TransformPivot.Pivot)
						{
							transformTarget.Rotate(rotationAxis, rotateAmount, Space.World);
						}
						else if(pivot == TransformPivot.Center)
						{
							transformTarget.RotateAround(originalPivot, rotationAxis, rotateAmount);
						}	

						totalRotationAmount *= Quaternion.Euler(rotationAxis * rotateAmount);
					}
					
				}

				previousMousePosition = mousePosition;



				yield return null;
			}

//			for(int i = 0; i < transformCommands.Count; i++)
//			{
//				((TransformCommand)transformCommands[i]).StoreNewTransformValues();
//			}

			CommandGroup commandGroup = new CommandGroup();
			commandGroup.Set(transformCommands);
			UndoRedoManager.Insert(commandGroup);

			totalRotationAmount = Quaternion.identity;
			isTransforming = false;

			SetPivotPoint();
		}

		Vector3 GetNearAxisDirection()
		{
			if(nearAxis != Axis.None)
			{
				if(nearAxis == Axis.X) return axisInfo.xDirection;
				if(nearAxis == Axis.Y) return axisInfo.yDirection;
				if(nearAxis == Axis.Z) return axisInfo.zDirection;
				if(nearAxis == Axis.Any) return Vector3.one;
			}
			return Vector3.zero;
		}
	
		public void SetPivotPoint()
		{
			if(mainTargetRoot != null)
			{
				if(pivot == TransformPivot.Pivot)
				{
					pivotPoint = mainTargetRoot.position;
				}
				else if(pivot == TransformPivot.Center)
				{
					pivotPoint = mainTargetRoot.position;
				}
			}
		}

		void SetPivotPointOffset(Vector3 offset)
		{
			pivotPoint += offset;
		}



		AxisVectors axisVectorsBuffer = new AxisVectors();
		void SetNearAxis()
		{
			if(isTransforming) return;

			nearAxis = Axis.None;

			if(mainTargetRoot == null) return;

			float distanceMultiplier = GetDistanceMultiplier();
			float handleMinSelectedDistanceCheck = (this.minSelectedDistanceCheck + handleWidth) * distanceMultiplier;


				float tipMinSelectedDistanceCheck = 0;
				axisVectorsBuffer.Clear();

				tipMinSelectedDistanceCheck = (this.minSelectedDistanceCheck + triangleSize) * distanceMultiplier;
				axisVectorsBuffer.Add(handleTriangles);

				HandleNearest(axisVectorsBuffer, tipMinSelectedDistanceCheck);

				if(nearAxis == Axis.None)
				{
					HandleNearest(handleLines, handleMinSelectedDistanceCheck);
				}

				HandleNearest(circlesLines, handleMinSelectedDistanceCheck);
		
		}

		void HandleNearest(AxisVectors axisVectors, float minSelectedDistanceCheck)
		{
			float xClosestDistance = ClosestDistanceFromMouseToLines(axisVectors.x);
			float yClosestDistance = ClosestDistanceFromMouseToLines(axisVectors.y);
			float zClosestDistance = ClosestDistanceFromMouseToLines(axisVectors.z);
			float allClosestDistance = ClosestDistanceFromMouseToLines(axisVectors.all);

			if(xClosestDistance <= minSelectedDistanceCheck && xClosestDistance <= yClosestDistance && xClosestDistance <= zClosestDistance) nearAxis = Axis.X;
			else if(yClosestDistance <= minSelectedDistanceCheck && yClosestDistance <= xClosestDistance && yClosestDistance <= zClosestDistance) nearAxis = Axis.Y;
			else if(zClosestDistance <= minSelectedDistanceCheck && zClosestDistance <= xClosestDistance && zClosestDistance <= yClosestDistance) nearAxis = Axis.Z;
		}

		float ClosestDistanceFromMouseToLines(List<Vector3> lines)
		{
			Ray mouseRay = myCamera.ScreenPointToRay(Input.mousePosition);

			float closestDistance = float.MaxValue;
			for(int i = 0; i < lines.Count; i += 2)
			{
				IntersectPoints points = Geometry.ClosestPointsOnSegmentToLine(lines[i], lines[i + 1], mouseRay.origin, mouseRay.direction);
				float distance = Vector3.Distance(points.first, points.second);
				if(distance < closestDistance)
				{
					closestDistance = distance;
				}
			}
			return closestDistance;
		}

		void SetAxisInfo()
		{
			if(mainTargetRoot != null)
			{
				float size = handleLength * GetDistanceMultiplier();
				axisInfo.Set(mainTargetRoot, pivotPoint, size, space);
			}
		}

		//This helps keep the size consistent no matter how far we are from it.
		float GetDistanceMultiplier()
		{
			if(mainTargetRoot == null) return 0f;

			if(myCamera.orthographic) return Mathf.Max(.01f, myCamera.orthographicSize * 2f);
			return Mathf.Max(.01f, Mathf.Abs(ExtVector3.MagnitudeInDirection(pivotPoint - transform.position, myCamera.transform.forward)));
		}

		void SetLines()
		{
			SetHandleLines();
			SetHandleTriangles();
			SetCircles(axisInfo, circlesLines);
		}

		void SetHandleLines()
		{
			handleLines.Clear();

			float distanceMultiplier = GetDistanceMultiplier();
			float lineWidth = handleWidth * distanceMultiplier;
			//When scaling, the axis will have different line lengths and direction.

			float yLineLength = Vector3.Distance(pivotPoint, axisInfo.yAxisEnd) * AxisDirectionMultiplier(axisInfo.yAxisEnd - pivotPoint, axisInfo.yDirection);
			AddQuads(pivotPoint, axisInfo.yDirection, axisInfo.xDirection, axisInfo.zDirection, yLineLength, lineWidth, handleLines.y);

		}
		int AxisDirectionMultiplier(Vector3 direction, Vector3 otherDirection)
		{
			return ExtVector3.IsInDirection(direction, otherDirection) ? 1 : -1;
		}

		void SetHandleTriangles()
		{
			handleTriangles.Clear();

			float triangleLength = triangleSize * GetDistanceMultiplier();
			AddTriangles(axisInfo.yAxisEnd, axisInfo.yDirection, axisInfo.xDirection, axisInfo.zDirection, triangleLength, handleTriangles.y);
		}

		void AddTriangles(Vector3 axisEnd, Vector3 axisDirection, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float size, List<Vector3> resultsBuffer)
		{
			Vector3 endPoint = axisEnd + (axisDirection * (size * 2f));
			Square baseSquare = GetBaseSquare(axisEnd, axisOtherDirection1, axisOtherDirection2, size / 2f);

			resultsBuffer.Add(baseSquare.bottomLeft);
			resultsBuffer.Add(baseSquare.topLeft);
			resultsBuffer.Add(baseSquare.topRight);
			resultsBuffer.Add(baseSquare.topLeft);
			resultsBuffer.Add(baseSquare.bottomRight);
			resultsBuffer.Add(baseSquare.topRight);

			for(int i = 0; i < 4; i++)
			{
				resultsBuffer.Add(baseSquare[i]);
				resultsBuffer.Add(baseSquare[i + 1]);
				resultsBuffer.Add(endPoint);
			}
		}


		void AddSquares(Vector3 axisStart, Vector3 axisDirection, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float size, List<Vector3> resultsBuffer)
		{
			AddQuads(axisStart, axisDirection, axisOtherDirection1, axisOtherDirection2, size, size * .5f, resultsBuffer);
		}
		void AddQuads(Vector3 axisStart, Vector3 axisDirection, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float length, float width, List<Vector3> resultsBuffer)
		{
			Vector3 axisEnd = axisStart + (axisDirection * length);
			AddQuads(axisStart, axisEnd, axisOtherDirection1, axisOtherDirection2, width, resultsBuffer);
		}
		void AddQuads(Vector3 axisStart, Vector3 axisEnd, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float width, List<Vector3> resultsBuffer)
		{
			Square baseRectangle = GetBaseSquare(axisStart, axisOtherDirection1, axisOtherDirection2, width);
			Square baseRectangleEnd = GetBaseSquare(axisEnd, axisOtherDirection1, axisOtherDirection2, width);

			resultsBuffer.Add(baseRectangle.bottomLeft);
			resultsBuffer.Add(baseRectangle.topLeft);
			resultsBuffer.Add(baseRectangle.topRight);
			resultsBuffer.Add(baseRectangle.bottomRight);

			resultsBuffer.Add(baseRectangleEnd.bottomLeft);
			resultsBuffer.Add(baseRectangleEnd.topLeft);
			resultsBuffer.Add(baseRectangleEnd.topRight);
			resultsBuffer.Add(baseRectangleEnd.bottomRight);

			for(int i = 0; i < 4; i++)
			{
				resultsBuffer.Add(baseRectangle[i]);
				resultsBuffer.Add(baseRectangleEnd[i]);
				resultsBuffer.Add(baseRectangleEnd[i + 1]);
				resultsBuffer.Add(baseRectangle[i + 1]);
			}
		}

		Square GetBaseSquare(Vector3 axisEnd, Vector3 axisOtherDirection1, Vector3 axisOtherDirection2, float size)
		{
			Square square;
			Vector3 offsetUp = ((axisOtherDirection1 * size) + (axisOtherDirection2 * size));
			Vector3 offsetDown = ((axisOtherDirection1 * size) - (axisOtherDirection2 * size));
			//These might not really be the proper directions, as in the bottomLeft might not really be at the bottom left...
			square.bottomLeft = axisEnd + offsetDown;
			square.topLeft = axisEnd + offsetUp;
			square.bottomRight = axisEnd - offsetUp;
			square.topRight = axisEnd - offsetDown;
			return square;
		}

		void SetCircles(AxisInfo axisInfo, AxisVectors axisVectors)
		{
			axisVectors.Clear();

			float circleLength = circleRadius * GetDistanceMultiplier();
			AddCircle(pivotPoint, axisInfo.xDirection, circleLength, axisVectors.x);
			AddCircle(pivotPoint, axisInfo.zDirection, circleLength, axisVectors.z);
			AddCircle(pivotPoint, (pivotPoint - transform.position).normalized, circleLength, axisVectors.all, false);	
		}

		void AddCircle(Vector3 origin, Vector3 axisDirection, float size, List<Vector3> resultsBuffer, bool depthTest = true)
		{
			Vector3 up = axisDirection.normalized * size;
			Vector3 forward = Vector3.Slerp(up, -up, .5f);
			Vector3 right = Vector3.Cross(up, forward).normalized * size;
		
			Matrix4x4 matrix = new Matrix4x4();
		
			matrix[0] = right.x;
			matrix[1] = right.y;
			matrix[2] = right.z;
		
			matrix[4] = up.x;
			matrix[5] = up.y;
			matrix[6] = up.z;
		
			matrix[8] = forward.x;
			matrix[9] = forward.y;
			matrix[10] = forward.z;
		
			Vector3 lastPoint = origin + matrix.MultiplyPoint3x4(new Vector3(Mathf.Cos(0), 0, Mathf.Sin(0)));
			Vector3 nextPoint = Vector3.zero;
			float multiplier = 360f / circleDetail;

			Plane plane = new Plane((transform.position - pivotPoint).normalized, pivotPoint);

			float circleHandleWidth = handleWidth * GetDistanceMultiplier();

			for(int i = 0; i < circleDetail + 1; i++)
			{
				nextPoint.x = Mathf.Cos((i * multiplier) * Mathf.Deg2Rad);
				nextPoint.z = Mathf.Sin((i * multiplier) * Mathf.Deg2Rad);
				nextPoint.y = 0;
			
				nextPoint = origin + matrix.MultiplyPoint3x4(nextPoint);
			
				if(!depthTest || plane.GetSide(lastPoint))
				{
					Vector3 centerPoint = (lastPoint + nextPoint) * .5f;
					Vector3 upDirection = (centerPoint - origin).normalized;
					AddQuads(lastPoint, nextPoint, upDirection, axisDirection, circleHandleWidth, resultsBuffer);
				}

				lastPoint = nextPoint;
			}
		}

		void DrawLines(List<Vector3> lines, Color color)
		{
			GL.Begin(GL.LINES);
			GL.Color(color);

			for(int i = 0; i < lines.Count; i += 2)
			{
				GL.Vertex(lines[i]);
				GL.Vertex(lines[i + 1]);
			}

			GL.End();
		}

		void DrawTriangles(List<Vector3> lines, Color color)
		{
			GL.Begin(GL.TRIANGLES);
			GL.Color(color);

			for(int i = 0; i < lines.Count; i += 3)
			{
				GL.Vertex(lines[i]);
				GL.Vertex(lines[i + 1]);
				GL.Vertex(lines[i + 2]);
			}

			GL.End();
		}

		void DrawQuads(List<Vector3> lines, Color color)
		{
			GL.Begin(GL.QUADS);
			GL.Color(color);

			for(int i = 0; i < lines.Count; i += 4)
			{
				GL.Vertex(lines[i]);
				GL.Vertex(lines[i + 1]);
				GL.Vertex(lines[i + 2]);
				GL.Vertex(lines[i + 3]);
			}

			GL.End();
		}

		void SetMaterial()
		{
			if(lineMaterial == null)
			{
				lineMaterial = new Material(Shader.Find("Custom/Lines"));
				outlineMaterial = new Material(Shader.Find("Custom/Outline"));
			}
		}
	}
}
