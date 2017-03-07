namespace PaintDotNet.Tools
{
    using PaintDotNet;
    using PaintDotNet.Canvas;
    using PaintDotNet.Collections;
    using PaintDotNet.Controls;
    using PaintDotNet.Functional;
    using PaintDotNet.HistoryFunctions;
    using PaintDotNet.HistoryMementos;
    using PaintDotNet.Rendering;
    using PaintDotNet.Resources;
    using PaintDotNet.Threading;
    using System;
    using System.Windows.Forms;

    internal abstract class SelectionTool : PaintDotNet.Tools.Tool
    {
        private bool append;
        private SelectionCombineMode combineMode;
        private Cursor cursorMouseDown;
        private Cursor cursorMouseUp;
        private Cursor cursorMouseUpMinus;
        private Cursor cursorMouseUpPlus;
        private bool hasMoved;
        private PointDouble lastXY;
        private bool moveOriginMode;
        private Selection newSelection;
        private SelectionCanvasLayer newSelectionRenderer;
        private Result<GeometryList> oldSelectionGeometryLazy;
        private DateTime startTime;
        private SegmentedList<PointDouble> tracePoints;
        private bool tracking;
        private SelectionHistoryMemento undoAction;
        private bool wasNotEmpty;

        public SelectionTool(DocumentWorkspace documentWorkspace, ImageResource toolBarImage, string name, string helpText, char hotKey, ToolBarConfigItems toolBarConfigItems) : base(documentWorkspace, toolBarImage, name, helpText, hotKey, false, toolBarConfigItems | (ToolBarConfigItems.None | ToolBarConfigItems.SelectionCombineMode))
        {
            this.tracking = false;
        }

        private PointDouble[] CreateSelectionPoly()
        {
            SegmentedList<PointDouble> list3;
            SegmentedList<PointDouble> inputTracePoints = this.TrimShapePath(this.tracePoints);
            SegmentedList<PointDouble> v = this.CreateShape(inputTracePoints);
            switch (this.combineMode)
            {
                case SelectionCombineMode.Exclude:
                case SelectionCombineMode.Xor:
                    list3 = v;
                    break;

                default:
                    list3 = Clipping.SutherlandHodgman<SegmentedList<PointDouble>, SegmentedList<PointDouble>>(base.Document.Bounds(), v);
                    break;
            }
            return list3.ToArrayEx<PointDouble>();
        }

        protected virtual SegmentedList<PointDouble> CreateShape(SegmentedList<PointDouble> inputTracePoints) => 
            inputTracePoints;

        private void Done()
        {
            if (this.tracking)
            {
                WhatToDo reset;
                PointDouble[] newPolygon = this.CreateSelectionPoly();
                this.hasMoved &= newPolygon.Length > 1;
                TimeSpan span = (TimeSpan) (DateTime.Now - this.startTime);
                double totalMilliseconds = span.TotalMilliseconds;
                bool flag = this.WasClickTooQuick(totalMilliseconds);
                bool flag2 = newPolygon.Length == 0;
                bool flag3 = false;
                if (this.append)
                {
                    if (((!this.hasMoved && this.MustMoveForEmit) | flag2) | flag3)
                    {
                        reset = WhatToDo.Reset;
                    }
                    else
                    {
                        reset = WhatToDo.Emit;
                    }
                }
                else if ((this.hasMoved || !this.MustMoveForEmit) && ((!flag && !flag2) && !flag3))
                {
                    reset = WhatToDo.Emit;
                }
                else
                {
                    reset = WhatToDo.Clear;
                }
                switch (this.GetWhatToDo(reset, this.append, this.oldSelectionGeometryLazy.Value, newPolygon))
                {
                    case WhatToDo.Clear:
                        if (this.wasNotEmpty)
                        {
                            this.undoAction.Name = DeselectFunction.StaticName;
                            this.undoAction.Image = DeselectFunction.StaticImage;
                            base.HistoryStack.PushNewMemento(this.undoAction);
                        }
                        base.Selection.Reset();
                        break;

                    case WhatToDo.Emit:
                        this.undoAction.Name = base.Name;
                        base.HistoryStack.PushNewMemento(this.undoAction);
                        base.Selection.CommitContinuation();
                        break;

                    case WhatToDo.Reset:
                        base.Selection.ResetContinuation();
                        break;
                }
                this.newSelection.Reset();
                this.tracking = false;
                base.DocumentWorkspace.EnableSelectionOutline = true;
                this.oldSelectionGeometryLazy = null;
            }
        }

        private Cursor GetCursor()
        {
            if (!base.IsInPanMode)
            {
                return this.GetCursor(base.IsMouseDown, (base.ModifierKeys & Keys.Control) > Keys.None, (base.ModifierKeys & Keys.Alt) > Keys.None);
            }
            return base.CurrentPanModeCursor;
        }

        private Cursor GetCursor(bool mouseDown, bool ctrlDown, bool altDown)
        {
            if (mouseDown)
            {
                return this.cursorMouseDown;
            }
            if (ctrlDown)
            {
                return this.cursorMouseUpPlus;
            }
            if (altDown)
            {
                return this.cursorMouseUpMinus;
            }
            return this.cursorMouseUp;
        }

        protected virtual WhatToDo GetWhatToDo(WhatToDo plannedAction, bool appending, GeometryList oldSelectionGeometry, PointDouble[] newPolygon) => 
            plannedAction;

        protected override void OnActivate()
        {
            base.Cursor = this.GetCursor();
            base.DocumentWorkspace.EnableSelectionTinting = true;
            this.newSelection = new Selection();
            this.newSelectionRenderer = new SelectionCanvasLayer();
            base.DocumentCanvas.CanvasLayers.Add(this.newSelectionRenderer);
            this.newSelectionRenderer.Selection = this.newSelection;
            this.newSelectionRenderer.IsInteriorFilled = false;
            this.newSelectionRenderer.IsVisible = false;
            base.OnActivate();
        }

        protected override void OnClick()
        {
            base.OnClick();
            if (!this.moveOriginMode)
            {
                this.Done();
            }
        }

        protected override void OnDeactivate()
        {
            base.DocumentWorkspace.EnableSelectionTinting = false;
            if (this.tracking)
            {
                this.Done();
            }
            base.OnDeactivate();
            this.SetCursors(null, null, null, null);
            base.DocumentCanvas.CanvasLayers.Remove(this.newSelectionRenderer);
            DisposableUtil.Free<SelectionCanvasLayer>(ref this.newSelectionRenderer);
            this.newSelection = null;
            this.oldSelectionGeometryLazy = null;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (this.tracking)
            {
                this.Render();
            }
            base.Cursor = this.GetCursor();
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if (this.tracking)
            {
                this.Render();
            }
            base.Cursor = this.GetCursor();
        }

        protected override void OnMouseDown(MouseEventArgsF e)
        {
            base.OnMouseDown(e);
            base.Cursor = this.GetCursor();
            PointDouble point = e.Point;
            PointDouble item = this.RoundMouseCanvasPoint(point);
            if (this.tracking)
            {
                this.moveOriginMode = true;
                this.lastXY = item;
                this.OnMouseMove(e);
            }
            else if (((e.Button & MouseButtons.Left) == MouseButtons.Left) || ((e.Button & MouseButtons.Right) == MouseButtons.Right))
            {
                this.tracking = true;
                this.hasMoved = false;
                this.startTime = DateTime.Now;
                this.tracePoints = new SegmentedList<PointDouble>();
                this.tracePoints.Add(item);
                this.undoAction = new SelectionHistoryMemento("sentinel", base.Image, base.DocumentWorkspace);
                this.wasNotEmpty = !base.Selection.IsEmpty;
                if (((base.ModifierKeys & Keys.Control) != Keys.None) && (e.Button == MouseButtons.Left))
                {
                    this.combineMode = SelectionCombineMode.Union;
                }
                else if (((base.ModifierKeys & Keys.Alt) != Keys.None) && (e.Button == MouseButtons.Left))
                {
                    this.combineMode = SelectionCombineMode.Exclude;
                }
                else if (((base.ModifierKeys & Keys.Control) != Keys.None) && (e.Button == MouseButtons.Right))
                {
                    this.combineMode = SelectionCombineMode.Xor;
                }
                else if (((base.ModifierKeys & Keys.Alt) != Keys.None) && (e.Button == MouseButtons.Right))
                {
                    this.combineMode = SelectionCombineMode.Intersect;
                }
                else
                {
                    this.combineMode = base.ToolSettings.Selection.CombineMode.Value;
                }
                base.DocumentWorkspace.EnableSelectionOutline = false;
                Result<GeometryList> oldSelectionGeometryLazy0 = base.Selection.GetCachedLazyGeometryList();
                this.oldSelectionGeometryLazy = oldSelectionGeometryLazy0;
                Work.QueueWorkItem(delegate {
                    Result<GeometryList> oldSelectionGeometryLazy = this.oldSelectionGeometryLazy;
                    if (oldSelectionGeometryLazy0 == oldSelectionGeometryLazy)
                    {
                        oldSelectionGeometryLazy.EnsureEvaluated();
                    }
                });
                this.newSelection.Restore(base.Selection.Save());
                switch (this.combineMode)
                {
                    case SelectionCombineMode.Replace:
                        this.append = false;
                        base.Selection.Reset();
                        break;

                    case SelectionCombineMode.Union:
                    case SelectionCombineMode.Exclude:
                    case SelectionCombineMode.Intersect:
                    case SelectionCombineMode.Xor:
                        this.append = true;
                        base.Selection.ResetContinuation();
                        break;

                    default:
                        throw ExceptionUtil.InvalidEnumArgumentException<SelectionCombineMode>(this.combineMode, "this.combineMode");
                }
                this.newSelectionRenderer.IsVisible = true;
                if (this.tracePoints.Count >= this.MinPointsForRender)
                {
                    this.Render();
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgsF e)
        {
            base.OnMouseMove(e);
            PointDouble point = e.Point;
            PointDouble num2 = this.RoundMouseCanvasPoint(point);
            if (this.moveOriginMode)
            {
                PointDouble num3 = new PointDouble(num2.X - this.lastXY.X, num2.Y - this.lastXY.Y);
                for (int i = 0; i < this.tracePoints.Count; i++)
                {
                    PointDouble num5 = this.tracePoints[i];
                    num5.X += num3.X;
                    num5.Y += num3.Y;
                    this.tracePoints[i] = num5;
                }
                this.lastXY = num2;
                this.Render();
            }
            else if (this.tracking)
            {
                foreach (PointDouble num6 in e.IntermediatePoints)
                {
                    PointDouble item = this.RoundMouseCanvasPoint(num6);
                    if (item != this.tracePoints[this.tracePoints.Count - 1])
                    {
                        this.tracePoints.Add(item);
                    }
                }
                this.hasMoved = true;
                this.Render();
            }
        }

        protected override void OnMouseUp(MouseEventArgsF e)
        {
            this.OnMouseMove(e);
            if (this.moveOriginMode)
            {
                this.moveOriginMode = false;
            }
            else
            {
                this.Done();
            }
            base.OnMouseUp(e);
            base.Cursor = this.GetCursor();
        }

        private void Render()
        {
            if ((this.tracePoints != null) && (this.tracePoints.Count >= this.MinPointsForRender))
            {
                PointDouble[] polygon = this.CreateSelectionPoly();
                if (polygon.Length > 2)
                {
                    base.Selection.SetContinuation(polygon, this.combineMode);
                    if (this.SelectionMode == SelectionCombineMode.Replace)
                    {
                        this.newSelection.SetContinuation(polygon, SelectionCombineMode.Replace);
                    }
                    else
                    {
                        this.newSelection.SetContinuation(polygon, SelectionCombineMode.Xor);
                    }
                    base.Update();
                }
            }
        }

        private PointDouble RoundMouseCanvasPoint(PointDouble pt) => 
            PointDouble.Truncate(pt);

        protected void SetCursors(string cursorMouseUpResName, string cursorMouseUpMinusResName, string cursorMouseUpPlusResName, string cursorMouseDownResName)
        {
            DisposableUtil.Free<Cursor>(ref this.cursorMouseUp);
            if (cursorMouseUpResName != null)
            {
                this.cursorMouseUp = PdnResources.GetCursor(cursorMouseUpResName);
            }
            DisposableUtil.Free<Cursor>(ref this.cursorMouseUpMinus);
            if (cursorMouseUpMinusResName != null)
            {
                this.cursorMouseUpMinus = PdnResources.GetCursor(cursorMouseUpMinusResName);
            }
            DisposableUtil.Free<Cursor>(ref this.cursorMouseUpPlus);
            if (cursorMouseUpPlusResName != null)
            {
                this.cursorMouseUpPlus = PdnResources.GetCursor(cursorMouseUpPlusResName);
            }
            DisposableUtil.Free<Cursor>(ref this.cursorMouseDown);
            if (cursorMouseDownResName != null)
            {
                this.cursorMouseDown = PdnResources.GetCursor(cursorMouseDownResName);
            }
        }

        protected virtual SegmentedList<PointDouble> TrimShapePath(SegmentedList<PointDouble> trimTheseTracePoints) => 
            trimTheseTracePoints;

        protected virtual bool WasClickTooQuick(double milliseconds) => 
            (milliseconds <= 50.0);

        protected virtual int MinPointsForRender =>
            2;

        protected virtual bool MustMoveForEmit =>
            true;

        protected SelectionCombineMode SelectionMode =>
            this.combineMode;

        protected enum WhatToDo
        {
            Clear,
            Emit,
            Reset
        }
    }
}

