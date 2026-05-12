using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace FSGaryityTool_Win11.Controls
{
    public sealed partial class PolyView : UserControl
    {
        


        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PolyView view)
            {
                view.LineDemonstratorCanvasControl.Invalidate();
                if (view.DataSource == null || !view.DataSource.SuppressInvalidate)
                    view.RebuildEditVisuals();
            }
        }

        private void PolyEditDemonstratorCanvas_RightTapped_AddPoint(object sender, RightTappedRoutedEventArgs e)
        {
            if (!IsEditPoint) { e.Handled = true; return; }
            var pos = e.GetPosition(PolyEditDemonstratorCanvas);
            _lastRightClickPosition = pos;
            _lastContextIndex = -1;
            var menu = BuildCanvasMenu();
            var dqAdd = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            if (dqAdd != null)
            {
                dqAdd.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                {
                    try { menu.ShowAt(PolyEditDemonstratorCanvas, pos); }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Menu.ShowAt failed: " + ex.Message); }
                });
            }
            else
            {
                try { menu.ShowAt(PolyEditDemonstratorCanvas, pos); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Menu.ShowAt failed: " + ex.Message); }
            }
            e.Handled = true;
        }

        // Returns a logical coordinate and an insertion index (or -1 for Append)
        private ((double x, double y) logical, int insertIndex) GetSnappedLogicalForCanvasPoint(float canvasX, float canvasY, GridBounds grid)
        {
            if (DataSource == null || DataSource.PointsData == null || DataSource.PointsData.Count == 0)
            {
                var l = DataSource?.CanvasToLogical(canvasX, canvasY, grid) ?? (0.0, 0.0);
                return (SnapLogical(l), -1);
            }

            var canvasPts = DataSource.GetCanvasPoints(grid);
            if (canvasPts.Count < 2)
            {
                // only one point -> insert after it
                var logical = DataSource.CanvasToLogical(canvasX, canvasY, grid);
                return (SnapLogical(logical), 1);
            }

            float bestDist2 = float.MaxValue;
            int bestSegment = -1;
            float bestPx = 0, bestPy = 0;

            for (int i = 0; i < canvasPts.Count - 1; i++)
            {
                var a = canvasPts[i];
                var b = canvasPts[i + 1];
                float ax = a.x, ay = a.y, bx = b.x, by = b.y;
                float vx = bx - ax, vy = by - ay;
                float wx = canvasX - ax, wy = canvasY - ay;
                float denom = vx * vx + vy * vy;
                float t = denom == 0 ? 0f : (vx * wx + vy * wy) / denom;
                t = MathF.Max(0f, MathF.Min(1f, t));
                float px = ax + t * vx;
                float py = ay + t * vy;
                float dx = px - canvasX;
                float dy = py - canvasY;
                float d2 = dx * dx + dy * dy;
                if (d2 < bestDist2)
                {
                    bestDist2 = d2;
                    bestSegment = i;
                    bestPx = px; bestPy = py;
                }
            }

            if (bestSegment >= 0)
            {
                var logical = DataSource.CanvasToLogical(bestPx, bestPy, grid);
                // insert after segment start -> index = bestSegment + 1
                return (SnapLogical(logical), bestSegment + 1);
            }

            var fallback = DataSource.CanvasToLogical(canvasX, canvasY, grid);
            return (SnapLogical(fallback), -1);
        }

        // Snap logical coordinate to integer grid when IsIntegerScaleMode is enabled
        private (double x, double y) SnapLogical((double x, double y) logical)
        {
            if (!IsIntegerScaleMode) return logical;

            double minX = MinHorizontalValue;
            double maxX = MaxHorizontalValue;
            double minY = MinVerticalValue;
            double maxY = MaxVerticalValue;

            double sx = Math.Round(logical.x);
            double sy = Math.Round(logical.y);

            sx = Math.Clamp(sx, minX, maxX);
            sy = Math.Clamp(sy, minY, maxY);

            return (sx, sy);
        }
        

        public PolyView()
        {
            InitializeComponent();
        }

        // Editing visuals
        private global::Microsoft.UI.Xaml.Shapes.Polyline? _editPolyline;
        private readonly List<global::Microsoft.UI.Xaml.Controls.Primitives.Thumb> _thumbs = new();
        private int _draggingIndex = -1;
        private bool _isInsertDragging = false;
        private bool _isThumbDragging = false;
        // last right-click position (canvas coords) and target index for context menu
        private Windows.Foundation.Point _lastRightClickPosition;
        private int _lastContextIndex = -1;
        private uint? _activePointerId = null;
        private int _selectedIndex = -1;
        // prevent reentrant rebuilds of visual tree
        private bool _isRebuilding = false;

        private void EnsureEditCanvasHandlers()
        {
            if (PolyEditDemonstratorCanvas == null) return;

            // Only need to react to size changes for rebuilds and insertion clicks
            SizeChanged -= PolyView_SizeChanged_ForEdit;
            SizeChanged += PolyView_SizeChanged_ForEdit;
            // Do not attach left-button pointer pressed insertion handler to avoid accidental inserts.
            // Insertion is done explicitly from the right-click menu.
            PolyEditDemonstratorCanvas.PointerPressed -= PolyEditDemonstratorCanvas_PointerPressed_InsertPoint;
            PolyEditDemonstratorCanvas.RightTapped -= PolyEditDemonstratorCanvas_RightTapped_AddPoint;
            PolyEditDemonstratorCanvas.RightTapped += PolyEditDemonstratorCanvas_RightTapped_AddPoint;
        }

        private void PolyView_SizeChanged_ForEdit(object? sender, SizeChangedEventArgs e)
        {
            RebuildEditVisuals();
        }

        private void PolyEditDemonstratorCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            // Pointer press is no longer needed for thumb since Thumb handles drag.
            // Leave handler for inserting points in future.
            return;
        }

        private void Thumb_DragStarted(object? sender, global::Microsoft.UI.Xaml.Controls.Primitives.DragStartedEventArgs e)
        {
            if (sender is global::Microsoft.UI.Xaml.Controls.Primitives.Thumb t && t.Tag is int idx)
            {
                _draggingIndex = idx;
                _isThumbDragging = true;
                // visual feedback - scale up using RenderTransform (safer than VisualState)
                t.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
                t.RenderTransform = new global::Microsoft.UI.Xaml.Media.ScaleTransform { ScaleX = 1.12, ScaleY = 1.12 };

                // temporarily suppress heavy invalidation
                if (DataSource != null) DataSource.SuppressInvalidate = true;
            }
        }

        private void Thumb_DragDelta(object? sender, global::Microsoft.UI.Xaml.Controls.Primitives.DragDeltaEventArgs e)
        {
            if (!(sender is global::Microsoft.UI.Xaml.Controls.Primitives.Thumb t)) return;
            if (DataSource == null) return;
            if (!(t.Tag is int idx)) return;

            double left = Canvas.GetLeft(t);
            double top = Canvas.GetTop(t);
            left += e.HorizontalChange;
            top += e.VerticalChange;
            Canvas.SetLeft(t, left);
            Canvas.SetTop(t, top);

            var centerX = (float)(left + t.Width / 2);
            var centerY = (float)(top + t.Height / 2);
            if (PolyEditDemonstratorCanvas == null) return;
            var grid = GetGridBounds(PolyEditDemonstratorCanvas.ActualWidth, PolyEditDemonstratorCanvas.ActualHeight);
            DataSource.MovePointToCanvasPosition(idx, centerX, centerY, grid);
            // update only thumb and polyline positions incrementally
            UpdateEditVisualPositions();
        }

        private void Thumb_DragCompleted(object? sender, global::Microsoft.UI.Xaml.Controls.Primitives.DragCompletedEventArgs e)
        {
            var thumb = sender as global::Microsoft.UI.Xaml.Controls.Primitives.Thumb;
            int? completedIdx = null;
            if (thumb != null && thumb.Tag is int tidx)
                completedIdx = tidx;

            _draggingIndex = -1;
            _isThumbDragging = false;
            if (thumb != null)
            {
                try
                {
                    // reset transform
                    thumb.RenderTransform = null;
                }
                catch { }
            }
            if (DataSource != null)
            {
                DataSource.SuppressInvalidate = false;

                // defer final full redraw to avoid reentrancy during input processing
                var owner = DataSource.Owner;
                if (owner != null)
                {
                    var dq = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
                    if (dq != null)
                    {
                        dq.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () => owner.Invalidate());
                    }
                    else
                    {
                        // fallback if no dispatcher queue available on this thread
                        owner.Invalidate();
                    }
                }
            }
            // ensure selection remains
            if (thumb != null && thumb.Tag is int sel)
                _selectedIndex = sel;

            // If integer snapping is enabled, snap the final moved point to integer grid now (after drag)
            try
            {
                if (completedIdx.HasValue && DataSource != null && (this.IsIntegerScaleMode))
                {
                    if (thumb != null && PolyEditDemonstratorCanvas != null)
                    {
                        double left = Canvas.GetLeft(thumb);
                        double top = Canvas.GetTop(thumb);
                        var centerX = (float)(left + thumb.Width / 2);
                        var centerY = (float)(top + thumb.Height / 2);
                        var grid = GetGridBounds(PolyEditDemonstratorCanvas.ActualWidth, PolyEditDemonstratorCanvas.ActualHeight);
                        var logical = DataSource.CanvasToLogical(centerX, centerY, grid);
                        double rx = Math.Round(logical.x);
                        double ry = Math.Round(logical.y);
                        DataSource.MovePointLogical(completedIdx.Value, rx, ry);
                        UpdateEditVisualPositions();
                    }
                }
            }
            catch { }
        }

        private void Thumb_Tapped(object? sender, global::Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (sender is global::Microsoft.UI.Xaml.Controls.Primitives.Thumb t && t.Tag is int idx)
            {
                _selectedIndex = idx;
                UpdateSelectionVisuals();
            }
        }

        private MenuFlyout BuildThumbMenu()
        {
            // Prefer to use the XAML-defined MenuFlyout as a template: clone its items so we don't reuse the visual instances.
            if (this.Resources != null && this.Resources.TryGetValue("PolyViewThumbMenu", out var res) && res is MenuFlyout template)
            {
                var menu = new MenuFlyout();
                foreach (var it in template.Items)
                {
                    if (it is MenuFlyoutItem srcItem)
                    {
                        var item = new MenuFlyoutItem { Text = srcItem.Text };
                        // copy style reference (styles are safe to reuse)
                        item.Style = srcItem.Style;

                        // clone common icon types to avoid reusing visual tree elements
                        if (srcItem.Icon is SymbolIcon sIcon)
                        {
                            item.Icon = new SymbolIcon { Symbol = sIcon.Symbol };
                        }
                        else if (srcItem.Icon is FontIcon fIcon)
                        {
                            item.Icon = new FontIcon { Glyph = fIcon.Glyph, FontFamily = fIcon.FontFamily };
                        }

                        // attach handler based on Text (matches XAML)
                        switch (srcItem.Text)
                        {
                            case "Insert Before":
                                item.Click += ThumbMenu_InsertBefore_Click;
                                item.IsEnabled = IsEditPoint;
                                break;
                            case "Insert After":
                                item.Click += ThumbMenu_InsertAfter_Click;
                                item.IsEnabled = IsEditPoint;
                                break;
                            case "Delete Point":
                                item.Click += ThumbMenu_Delete_Click;
                                item.IsEnabled = IsEditPoint;
                                break;
                            default:
                                // preserve no-op or other items by copying nothing
                                break;
                        }

                        menu.Items.Add(item);
                    }
                    else
                    {
                        // if there are separators or other types, try to reuse their type-safe constructors
                        if (it is MenuFlyoutSeparator)
                            menu.Items.Add(new MenuFlyoutSeparator());
                    }
                }

                return menu;
            }

            // Fallback: construct a simple menu when template is not available
            var fallback = new MenuFlyout();
            var insertBefore = new MenuFlyoutItem { Text = "Insert Before" };
            insertBefore.Click += ThumbMenu_InsertBefore_Click;
            insertBefore.IsEnabled = IsEditPoint;
            var insertAfter = new MenuFlyoutItem { Text = "Insert After" };
            insertAfter.Click += ThumbMenu_InsertAfter_Click;
            insertAfter.IsEnabled = IsEditPoint;
            var delete = new MenuFlyoutItem { Text = "Delete Point" };
            delete.Click += ThumbMenu_Delete_Click;
            delete.IsEnabled = IsEditPoint;
            // try to apply styles if present
            if (this.Resources != null && this.Resources.TryGetValue("PolyViewMenuFlyoutItemAddStyle", out var addSty) && addSty is Style addStyle)
            {
                insertBefore.Style = addStyle;
                insertAfter.Style = addStyle;
            }
            if (this.Resources != null && this.Resources.TryGetValue("PolyViewMenuFlyoutItemDeleteStyle", out var delSty) && delSty is Style delStyle)
            {
                delete.Style = delStyle;
            }
            fallback.Items.Add(insertBefore);
            fallback.Items.Add(insertAfter);
            fallback.Items.Add(delete);
            return fallback;
        }

        private MenuFlyout BuildCanvasMenu()
        {
            if (this.Resources != null && this.Resources.TryGetValue("PolyViewCanvasMenu", out var res) && res is MenuFlyout template)
            {
                var menu = new MenuFlyout();
                foreach (var it in template.Items)
                {
                    if (it is MenuFlyoutItem srcItem)
                    {
                        var item = new MenuFlyoutItem { Text = srcItem.Text };
                        item.Style = srcItem.Style;
                        if (srcItem.Icon is SymbolIcon sIcon)
                            item.Icon = new SymbolIcon { Symbol = sIcon.Symbol };
                        else if (srcItem.Icon is FontIcon fIcon)
                            item.Icon = new FontIcon { Glyph = fIcon.Glyph, FontFamily = fIcon.FontFamily };

                        // attach known handlers
                        if (srcItem.Text == "Add Point Here")
                            item.Click += CanvasMenu_AddPoint_Click;
                        item.IsEnabled = IsEditPoint;

                        menu.Items.Add(item);
                    }
                    else if (it is MenuFlyoutSeparator)
                    {
                        menu.Items.Add(new MenuFlyoutSeparator());
                    }
                }
                return menu;
            }

            // fallback
            var fallback = new MenuFlyout();
            var add = new MenuFlyoutItem { Text = "Add Point Here" };
            add.Click += CanvasMenu_AddPoint_Click;
            add.IsEnabled = IsEditPoint;
            if (this.Resources != null && this.Resources.TryGetValue("PolyViewMenuFlyoutItemAddStyle", out var sres) && sres is Style sAdd)
                add.Style = sAdd;
            fallback.Items.Add(add);
            return fallback;
        }

        private void Thumb_RightTapped(object? sender, RightTappedRoutedEventArgs e)
        {
            if (!IsEditPoint) { e.Handled = true; return; }

            if (sender is global::Microsoft.UI.Xaml.Controls.Primitives.Thumb t && t.Tag is int idx)
            {
                _lastContextIndex = idx;
                _lastRightClickPosition = e.GetPosition(PolyEditDemonstratorCanvas);
                var menu = BuildThumbMenu();

                var dq = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
                Action show = () =>
                {
                    try { menu.ShowAt(PolyEditDemonstratorCanvas, _lastRightClickPosition); }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Menu.ShowAt failed: " + ex.Message); }
                };

                if (dq != null) dq.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () => show());
                else show();

                e.Handled = true;
            }
        }

        private void UpdateSelectionVisuals()
        {
            // simple visual: rebuild thumbs so selected one can use different style
            for (int i = 0; i < _thumbs.Count; i++)
            {
                var thumb = _thumbs[i];
                if (i == _selectedIndex)
                {
                    thumb.Opacity = 1.0;
                }
                else
                {
                    thumb.Opacity = 0.9;
                }
            }
        }

        // Removed pointer-fallback handlers. Thumb.DragDelta is used as primary drag mechanism.

        private void PolyEditDemonstratorCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            // If a Thumb is being dragged, Thumb.DragDelta handles movement. Prevent the canvas pointer-moved
            // handler from also moving points which causes jitter/refresh.
            if (_isThumbDragging) return;
            if (_draggingIndex < 0) return;
            if (DataSource == null) return;
            var p = e.GetCurrentPoint(PolyEditDemonstratorCanvas);
            var pt = p.Position;
            var grid = GetGridBounds(PolyEditDemonstratorCanvas.ActualWidth, PolyEditDemonstratorCanvas.ActualHeight);
            DataSource.MovePointToCanvasPosition(_draggingIndex, (float)pt.X, (float)pt.Y, grid);
            UpdateEditVisualPositions();
            e.Handled = true;
        }

        // Insert-on-click logic: click empty area to insert a new point and begin dragging it
        private void PolyEditDemonstratorCanvas_PointerPressed_InsertPoint(object sender, PointerRoutedEventArgs e)
        {
            if (DataSource == null) return;
            var pt = e.GetCurrentPoint(PolyEditDemonstratorCanvas).Position;
            var grid = GetGridBounds(PolyEditDemonstratorCanvas.ActualWidth, PolyEditDemonstratorCanvas.ActualHeight);

            // if clicked near existing thumb, ignore (let thumb handle)
            int near = DataSource.GetNearestPointIndex((float)pt.X, (float)pt.Y, grid, 10f);
            if (near >= 0) return;

            // Insert new logical point at click position and start dragging
            var logical = DataSource.CanvasToLogical((float)pt.X, (float)pt.Y, grid);
            int idx = DataSource.InsertPointLogical(logical);
            if (idx >= 0)
            {
                // rebuild visuals so thumb exists
                RebuildEditVisuals();

                // capture pointer on canvas and set insert drag state
                _isInsertDragging = true;
                _activePointerId = e.Pointer.PointerId;
                try { PolyEditDemonstratorCanvas.CapturePointer(e.Pointer); } catch { }
                _draggingIndex = idx;
                e.Handled = true;
            }
        }

        private void ShowPointContextMenu(int idx, Windows.Foundation.Point position)
        {
            if (!IsEditPoint) return;

            var menu = new MenuFlyout();
            // Insert Before
            var insertBefore = new MenuFlyoutItem { Text = "Insert Before" };
            try
            {
                object? sres = null;
                if (PolyEditDemonstratorCanvas?.Resources != null && PolyEditDemonstratorCanvas.Resources.TryGetValue("PolyViewMenuFlyoutItemAddStyle", out sres)) { }
                else if (this.Resources != null && this.Resources.TryGetValue("PolyViewMenuFlyoutItemAddStyle", out sres)) { }
                else if (Application.Current?.Resources != null && Application.Current.Resources.TryGetValue("PolyViewMenuFlyoutItemAddStyle", out sres)) { }
                if (sres is Style ssty) insertBefore.Style = ssty;
            }
            catch { }
            insertBefore.Click += (_, __) =>
            {
                if (DataSource == null) return;
                var grid = GetGridBounds(PolyEditDemonstratorCanvas.ActualWidth, PolyEditDemonstratorCanvas.ActualHeight);
                var clicked = position;
                // determine target segment: prefer segment between idx-1 and idx when exists
                int segA = Math.Max(0, idx - 1);
                int segB = Math.Min(idx, (DataSource.PointsData?.Count ?? 1) - 1);
                var proj = ProjectToSegmentCanvas(segA, segB, (float)clicked.X, (float)clicked.Y, grid);
                var logical = DataSource.CanvasToLogical(proj.x, proj.y, grid);
                DataSource.InsertPointLogicalAt(idx, logical);
                RebuildEditVisuals();
            };
            menu.Items.Add(insertBefore);

            // Insert After
            var insertAfter = new MenuFlyoutItem { Text = "Insert After" };
            try
            {
                object? sres2 = null;
                if (PolyEditDemonstratorCanvas?.Resources != null && PolyEditDemonstratorCanvas.Resources.TryGetValue("PolyViewMenuFlyoutItemAddStyle", out sres2)) { }
                else if (this.Resources != null && this.Resources.TryGetValue("PolyViewMenuFlyoutItemAddStyle", out sres2)) { }
                else if (Application.Current?.Resources != null && Application.Current.Resources.TryGetValue("PolyViewMenuFlyoutItemAddStyle", out sres2)) { }
                if (sres2 is Style ssty2) insertAfter.Style = ssty2;
            }
            catch { }
            insertAfter.Click += (_, __) =>
            {
                if (DataSource == null) return;
                var grid = GetGridBounds(PolyEditDemonstratorCanvas.ActualWidth, PolyEditDemonstratorCanvas.ActualHeight);
                var clicked = position;
                int count = DataSource.PointsData?.Count ?? 0;
                int segA = Math.Min(idx, Math.Max(0, count - 2));
                int segB = Math.Min(idx + 1, Math.Max(0, count - 1));
                var proj = ProjectToSegmentCanvas(segA, segB, (float)clicked.X, (float)clicked.Y, grid);
                var logical = DataSource.CanvasToLogical(proj.x, proj.y, grid);
                DataSource.InsertPointLogicalAt(idx + 1, logical);
                RebuildEditVisuals();
            };
            menu.Items.Add(insertAfter);

            var delete = new MenuFlyoutItem { Text = "Delete Point" };
            // find delete style resource and apply
            object? dstyle = null;
            if (PolyEditDemonstratorCanvas?.Resources != null && PolyEditDemonstratorCanvas.Resources.TryGetValue("PolyViewMenuFlyoutItemDeleteStyle", out dstyle)) { }
            else if (this.Resources != null && this.Resources.TryGetValue("PolyViewMenuFlyoutItemDeleteStyle", out dstyle)) { }
            else if (Application.Current?.Resources != null && Application.Current.Resources.TryGetValue("PolyViewMenuFlyoutItemDeleteStyle", out dstyle)) { }

            if (dstyle is Style delStyle)
                delete.Style = delStyle;

            delete.Click += (_, __) =>
            {
                DataSource?.RemoveAt(idx);
                _selectedIndex = -1;
                RebuildEditVisuals();
            };
            menu.Items.Add(delete);
            var dqMenu = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
            if (dqMenu != null)
            {
                dqMenu.TryEnqueue(Microsoft.UI.Dispatching.DispatcherQueuePriority.Normal, () =>
                {
                    try { menu.ShowAt(PolyEditDemonstratorCanvas, position); }
                    catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Menu.ShowAt failed: " + ex.Message); }
                });
            }
            else
            {
                try { menu.ShowAt(PolyEditDemonstratorCanvas, position); }
                catch (Exception ex) { System.Diagnostics.Debug.WriteLine("Menu.ShowAt failed: " + ex.Message); }
            }
        }

        // Project a canvas point onto the segment between points at indices a and b; returns canvas coordinates of projection.
        private (float x, float y) ProjectToSegmentCanvas(int aIndex, int bIndex, float canvasX, float canvasY, GridBounds grid)
        {
            if (DataSource == null || DataSource.PointsData == null)
                return (canvasX, canvasY);

            var pts = DataSource.GetCanvasPoints(grid);
            if (pts.Count == 0) return (canvasX, canvasY);

            aIndex = Math.Clamp(aIndex, 0, pts.Count - 1);
            bIndex = Math.Clamp(bIndex, 0, pts.Count - 1);
            var a = pts[aIndex];
            var b = pts[bIndex];

            float ax = a.x, ay = a.y, bx = b.x, by = b.y;
            float vx = bx - ax, vy = by - ay;
            float wx = canvasX - ax, wy = canvasY - ay;
            float denom = vx * vx + vy * vy;
            float t = denom == 0f ? 0f : (vx * wx + vy * wy) / denom;
            t = MathF.Max(0f, MathF.Min(1f, t));
            float px = ax + t * vx;
            float py = ay + t * vy;
            return (px, py);
        }

        // Handlers wired from XAML MenuFlyout
        private void CanvasMenu_AddPoint_Click(object? sender, RoutedEventArgs e)
        {
            if (DataSource == null) return;
            var grid = GetGridBounds(PolyEditDemonstratorCanvas.ActualWidth, PolyEditDemonstratorCanvas.ActualHeight);
            var (logical, insertIndex) = GetSnappedLogicalForCanvasPoint((float)_lastRightClickPosition.X, (float)_lastRightClickPosition.Y, grid);
            if (insertIndex >= 0)
                DataSource.InsertPointLogicalAt(insertIndex, logical);
            else
                DataSource.InsertPointLogical(logical);
            RebuildEditVisuals();
        }

        private void ThumbMenu_InsertBefore_Click(object? sender, RoutedEventArgs e)
        {
            if (DataSource == null) return;
            int idx = _lastContextIndex;
            var grid = GetGridBounds(PolyEditDemonstratorCanvas.ActualWidth, PolyEditDemonstratorCanvas.ActualHeight);
            int segA = Math.Max(0, idx - 1);
            int segB = Math.Min(idx, (DataSource.PointsData?.Count ?? 1) - 1);
            var proj = ProjectToSegmentCanvas(segA, segB, (float)_lastRightClickPosition.X, (float)_lastRightClickPosition.Y, grid);
            var logical = DataSource.CanvasToLogical(proj.x, proj.y, grid);
            DataSource.InsertPointLogicalAt(idx, logical);
            RebuildEditVisuals();
        }

        private void ThumbMenu_InsertAfter_Click(object? sender, RoutedEventArgs e)
        {
            if (DataSource == null) return;
            int idx = _lastContextIndex;
            var grid = GetGridBounds(PolyEditDemonstratorCanvas.ActualWidth, PolyEditDemonstratorCanvas.ActualHeight);
            int count = DataSource.PointsData?.Count ?? 0;
            int segA = Math.Min(idx, Math.Max(0, count - 2));
            int segB = Math.Min(idx + 1, Math.Max(0, count - 1));
            var proj = ProjectToSegmentCanvas(segA, segB, (float)_lastRightClickPosition.X, (float)_lastRightClickPosition.Y, grid);
            var logical = DataSource.CanvasToLogical(proj.x, proj.y, grid);
            DataSource.InsertPointLogicalAt(idx + 1, logical);
            RebuildEditVisuals();
        }

        private void ThumbMenu_Delete_Click(object? sender, RoutedEventArgs e)
        {
            if (DataSource == null) return;
            int idx = _lastContextIndex;
            DataSource.RemoveAt(idx);
            _selectedIndex = -1;
            RebuildEditVisuals();
        }

        private void PolyEditDemonstratorCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_isInsertDragging && _activePointerId.HasValue && e.Pointer.PointerId == _activePointerId.Value)
            {
                _isInsertDragging = false;
                _activePointerId = null;
                _draggingIndex = -1;
                try { PolyEditDemonstratorCanvas.ReleasePointerCapture(e.Pointer); } catch { }
                e.Handled = true;
                return;
            }

            if (_draggingIndex < 0) return;
            _draggingIndex = -1;
            try { PolyEditDemonstratorCanvas.ReleasePointerCapture(e.Pointer); } catch { }
            e.Handled = true;
        }

        private void RebuildEditVisuals()
        {
            if (_isRebuilding) return;
            if (PolyEditDemonstratorCanvas == null) return;
            _isRebuilding = true;
            try
            {
                PolyEditDemonstratorCanvas.Children.Clear();
                _thumbs.Clear();

                if (DataSource == null || DataSource.PointsData == null || DataSource.PointsData.Count == 0)
                    return;

                var grid = GetGridBounds(PolyEditDemonstratorCanvas.ActualWidth, PolyEditDemonstratorCanvas.ActualHeight);
                var pts = DataSource.GetCanvasPoints(grid);

                // polyline
                _editPolyline = new global::Microsoft.UI.Xaml.Shapes.Polyline
                {
                    Stroke = new SolidColorBrush(Microsoft.UI.Colors.DeepSkyBlue),
                    StrokeThickness = 2,
                };

                var pc = new global::Microsoft.UI.Xaml.Media.PointCollection();
                foreach (var p in pts)
                    pc.Add(new Windows.Foundation.Point(p.x, p.y));
                _editPolyline.Points = pc;
                PolyEditDemonstratorCanvas.Children.Add(_editPolyline);

                // thumbs (use Thumb control for built-in drag events)
                for (int i = 0; i < pts.Count; i++)
                {
                    var p = pts[i];
                    var thumb = new global::Microsoft.UI.Xaml.Controls.Primitives.Thumb
                    {
                        Width = 14,
                        Height = 14,
                        Tag = i,
                    };

                    // Try to find a resource named "PolyViewThumbTemplate" in local -> control -> app resources.
                    object? res = null;
                    if (PolyEditDemonstratorCanvas?.Resources != null && PolyEditDemonstratorCanvas.Resources.TryGetValue("PolyViewThumbTemplate", out res)) { }
                    else if (this.Resources != null && this.Resources.TryGetValue("PolyViewThumbTemplate", out res)) { }
                    else if (Application.Current?.Resources != null && Application.Current.Resources.TryGetValue("PolyViewThumbTemplate", out res)) { }

                    if (res is Style s)
                    {
                        thumb.Style = s;
                    }
                    else if (res is ControlTemplate ct)
                    {
                        thumb.Template = ct;
                    }
                    else
                    {
                        // fallback simple styling
                        thumb.Background = new SolidColorBrush(Microsoft.UI.Colors.White);
                        thumb.BorderBrush = new SolidColorBrush(Microsoft.UI.Colors.DeepSkyBlue);
                        thumb.BorderThickness = new Thickness(2);
                    }

                    Canvas.SetLeft(thumb, p.x - thumb.Width / 2);
                    Canvas.SetTop(thumb, p.y - thumb.Height / 2);
                    bool isEndpoint = (i == 0 || i == pts.Count - 1);
                    bool allowDrag = !isEndpoint || IsStartEndPointMoved;

                    if (allowDrag)
                    {
                        thumb.DragStarted += Thumb_DragStarted;
                        thumb.DragDelta += Thumb_DragDelta;
                        thumb.DragCompleted += Thumb_DragCompleted;
                    }
                    else
                    {
                        // Prevent thumb from being dragged but keep it hittable for selection/context
                        thumb.IsHitTestVisible = true;
                    }

                    // selection handling
                    thumb.Tapped += Thumb_Tapped;
                    thumb.RightTapped += Thumb_RightTapped;
                    PolyEditDemonstratorCanvas.Children.Add(thumb);
                    _thumbs.Add(thumb);
                }

                EnsureEditCanvasHandlers();
                }
            finally
            {
                _isRebuilding = false;
            }
         }

        private void UpdateEditVisualPositions()
        {
            if (PolyEditDemonstratorCanvas == null) return;
            if (DataSource == null || DataSource.PointsData == null) return;
            if (_editPolyline == null) return;

            var grid = GetGridBounds(PolyEditDemonstratorCanvas.ActualWidth, PolyEditDemonstratorCanvas.ActualHeight);
            var pts = DataSource.GetCanvasPoints(grid);

            var pc = _editPolyline.Points;
            pc.Clear();
            for (int i = 0; i < pts.Count; i++)
            {
                var p = pts[i];
                pc.Add(new Windows.Foundation.Point(p.x, p.y));
                if (i < _thumbs.Count)
                {
                    var thumb = _thumbs[i];
                    Canvas.SetLeft(thumb, p.x - thumb.Width / 2);
                    Canvas.SetTop(thumb, p.y - thumb.Height / 2);
                }
            }
        }

        public static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register(
            nameof(DataSource), typeof(PolyViewDataSource), typeof(PolyView), new PropertyMetadata(null, OnDataSourceChanged));

        public PolyViewDataSource? DataSource
        {
            get => (PolyViewDataSource?)GetValue(DataSourceProperty);
            set => SetValue(DataSourceProperty, value);
        }

        private void PolyView_KeyDown(object sender, global::Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Delete && _selectedIndex >= 0)
            {
                DataSource?.RemoveAt(_selectedIndex);
                _selectedIndex = -1;
                RebuildEditVisuals();
            }
        }

        private static void OnDataSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PolyView view)
            {
                view.AttachDataSource(e.OldValue as PolyViewDataSource, e.NewValue as PolyViewDataSource);
            }
        }

        private void AttachDataSource(PolyViewDataSource? oldDs, PolyViewDataSource? newDs)
        {
            if (oldDs != null)
            {
                oldDs.Owner = null;
                oldDs.PropertyChanged -= DataSource_PropertyChanged;
                if (oldDs.PointsData != null)
                    oldDs.PointsData.CollectionChanged -= Points_CollectionChanged;
            }

            if (newDs != null)
            {
                newDs.Owner = this;
                newDs.PropertyChanged += DataSource_PropertyChanged;
                if (newDs.PointsData != null)
                    newDs.PointsData.CollectionChanged += Points_CollectionChanged;
            }

            Invalidate();
        }

        private void DataSource_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // rebuild visuals when datasource changes
            // but skip if datasource requests suppression (e.g. during active thumb drag)
            if (DataSource != null && DataSource.SuppressInvalidate)
                return;

            Invalidate();
        }

        private void Points_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // collection changes should not trigger rebuild while suppressing invalidation
            if (DataSource != null && DataSource.SuppressInvalidate)
                return;

            Invalidate();
        }

        private void LineDemonstratorCanvasControl_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            var ds = args.DrawingSession;
            var grid = GetGridBounds(sender.ActualWidth, sender.ActualHeight);

            int rowCount = GetActualRowTickCount(grid.Height);
            int colCount = GetActualColumnTickCount(grid.Width);

            var rowPositions = GetTickPositions(grid.Top, grid.Height, rowCount);
            var colPositions = GetTickPositions(grid.Left, grid.Width, colCount);

            int rowCenterIndex = GetCenterIndex(rowCount);
            int colCenterIndex = GetCenterIndex(colCount);

            var colors = GetTickColors();

            DrawGridBorder(ds, grid, colors.BorderColor);
            DrawTickLines(ds, grid, rowPositions, colPositions, rowCenterIndex, colCenterIndex, colors);
            DrawBorderTicks(args, grid, rowPositions, colPositions, rowCenterIndex, colCenterIndex, colors);
            DrawIntersections(ds, rowPositions, colPositions, rowCenterIndex, colCenterIndex, colors);
        }

        public void Invalidate()
        {
            LineDemonstratorCanvasControl.Invalidate();
            RebuildEditVisuals();
        }
        private int GetActualRowTickCount(double height)
        {
            return GetActualTickCount(RowTickCount, height, MinRowTickHeight, MaxRowTickHeight,
                MinRowTickCount, MaxRowTickCount, RowAutoGridMode, RowAutoGridMultiple);
        }

        private int GetActualColumnTickCount(double width)
        {
            return GetActualTickCount(ColumnTickCount, width, MinColumnTickWidth, MaxColumnTickWidth,
                MinColumnTickCount, MaxColumnTickCount, ColumnAutoGridMode, ColumnAutoGridMultiple);
        }
        private int GetActualTickCount(string tickCountSetting, double length, double minTickSize, double maxTickSize, int minTickCount, int maxTickCount, AutoGridMode mode, int multiple)
        {
            if (string.Equals(tickCountSetting, "Auto", StringComparison.OrdinalIgnoreCase))
            {
                double clampedTickSize = Math.Clamp(minTickSize, 1.0, maxTickSize);
                int rawCount = (int)(length / clampedTickSize);
                int clampedCount = Math.Clamp(rawCount, minTickCount, maxTickCount);

                return mode switch
                {
                    AutoGridMode.Even => (clampedCount % 2 == 0) ? clampedCount : clampedCount + 1,
                    AutoGridMode.Odd => (clampedCount % 2 == 1) ? clampedCount : clampedCount + 1,
                    AutoGridMode.MultipleOfN => Math.Max(multiple, (clampedCount / multiple) * multiple),
                    AutoGridMode.PowerOfN => GetNearestPowerOfN(clampedCount, multiple),
                    _ => clampedCount
                };
            }

            if (int.TryParse(tickCountSetting, out int manualCount))
                return manualCount;

            return 8;
        }
        private int GetNearestPowerOfN(int target, int baseN)
        {
            if (baseN < 2) baseN = 2; // 最小底数为2
            int power = 1;
            while (power < target)
                power *= baseN;
            return power;
        }



        private void DrawGridBorder(CanvasDrawingSession ds, GridBounds grid, Windows.UI.Color color)
        {
            float left = grid.Left;
            float top = grid.Top;
            float right = grid.Right;
            float bottom = grid.Bottom;

            float leftWidth = (float)WaveGridBorderThickness.Left;
            float topWidth = (float)WaveGridBorderThickness.Top;
            float rightWidth = (float)WaveGridBorderThickness.Right;
            float bottomWidth = (float)WaveGridBorderThickness.Bottom;

            DrawLineStyle(ds, left, top, right, top, color, topWidth, WaveGridBorderLineStyle);       // Top
            DrawLineStyle(ds, right, top, right, bottom, color, rightWidth, WaveGridBorderLineStyle); // Right
            DrawLineStyle(ds, right, bottom, left, bottom, color, bottomWidth, WaveGridBorderLineStyle); // Bottom
            DrawLineStyle(ds, left, bottom, left, top, color, leftWidth, WaveGridBorderLineStyle);    // Left
        }
        private void DrawIntersections(CanvasDrawingSession ds, List<float> rowPositions, List<float> colPositions, int rowCenterIndex, int colCenterIndex, TickColors colors)
        {
            if (WaveGridTickMode == TickModeStyle.None)
                return;

            float crossSize = (float)CrossSize;
            float lineWidth = 1.5f;
            float dotRadius = (float)DotRadius;

            foreach (var (y, rowIdx) in rowPositions.Select((v, i) => (v, i)))
            {
                foreach (var (x, colIdx) in colPositions.Select((v, i) => (v, i)))
                {
                    var isRowCenter = rowIdx == rowCenterIndex;
                    var isColCenter = colIdx == colCenterIndex;

                    Windows.UI.Color color = isRowCenter ? colors.RowCenterColor :
                                              isColCenter ? colors.ColCenterColor :
                                              colors.ColColor;

                    if (WaveGridTickMode == TickModeStyle.Intersection)
                    {
                        ds.FillCircle(x, y, dotRadius, color);
                    }
                    else if (WaveGridTickMode == TickModeStyle.Cross)
                    {
                        float half = crossSize / 2;
                        ds.DrawLine(x - half, y, x + half, y, color, lineWidth);
                        ds.DrawLine(x, y - half, x, y + half, color, lineWidth);
                    }
                }
            }
        }
        private void DrawTickLines(CanvasDrawingSession ds, GridBounds grid, List<float> rowPositions, List<float> colPositions, int rowCenterIndex, int colCenterIndex, TickColors colors)
        {
            foreach (var (y, i) in rowPositions.Select((v, idx) => (v, idx)))
            {
                var color = (i == rowCenterIndex) ? colors.RowCenterColor : colors.RowColor;
                DrawLineStyle(ds, grid.Left, y, grid.Right, y, color, (float)RowTickLineWidth, WaveGridTickLineStyle);
            }

            foreach (var (x, i) in colPositions.Select((v, idx) => (v, idx)))
            {
                var color = (i == colCenterIndex) ? colors.ColCenterColor : colors.ColColor;
                DrawLineStyle(ds, x, grid.Top, x, grid.Bottom, color, (float)ColumnTickLineWidth, WaveGridTickLineStyle);
            }
        }

        private void DrawBorderTicks(CanvasDrawEventArgs args, GridBounds grid, List<float> rowPositions, List<float> colPositions, int rowCenterIndex, int colCenterIndex, TickColors colors)
        {
            var ds = args.DrawingSession;

            // 主刻度线
            DrawTickSegments(ds, rowPositions, grid.Left, grid.Left + (float)WaveGridBorderTickThickness.Left, rowCenterIndex, colors.RowColor, colors.RowCenterColor, true);
            DrawTickSegments(ds, rowPositions, grid.Right, grid.Right - (float)WaveGridBorderTickThickness.Right, rowCenterIndex, colors.RowColor, colors.RowCenterColor, true);
            DrawTickSegments(ds, colPositions, grid.Top, grid.Top + (float)WaveGridBorderTickThickness.Top, colCenterIndex, colors.ColColor, colors.ColCenterColor, false);
            DrawTickSegments(ds, colPositions, grid.Bottom, grid.Bottom - (float)WaveGridBorderTickThickness.Bottom, colCenterIndex, colors.ColColor, colors.ColCenterColor, false);

            // 子刻度线
            DrawSubTicks(ds, grid, rowPositions, colPositions, colors);
        }
        private void DrawSubTicks(CanvasDrawingSession ds, GridBounds grid, List<float> rowPositions, List<float> colPositions, TickColors colors)
        {
            int rowSubCells = Math.Max(0, RowGridBorderSubTickCount);
            int colSubCells = Math.Max(0, ColumnGridBorderSubTickCount);
            int rowSubTicksPerSegment = Math.Max(0, rowSubCells - 1);
            int colSubTicksPerSegment = Math.Max(0, colSubCells - 1);

            float subTickLeft = (float)WaveGridBorderSubTickThickness.Left;
            float subTickTop = (float)WaveGridBorderSubTickThickness.Top;
            float subTickRight = (float)WaveGridBorderSubTickThickness.Right;
            float subTickBottom = (float)WaveGridBorderSubTickThickness.Bottom;

            // 行方向子刻度（垂直线）
            var rowSegments = new List<(float start, float end)>();
            rowSegments.Add((grid.Top, rowPositions.First())); // 边框到第一个刻度
            for (int i = 0; i < rowPositions.Count - 1; i++)
                rowSegments.Add((rowPositions[i], rowPositions[i + 1]));
            rowSegments.Add((rowPositions.Last(), grid.Bottom)); // 最后一个刻度到边框

            foreach (var (start, end) in rowSegments)
            {
                for (int j = 1; j <= rowSubTicksPerSegment; j++)
                {
                    float y = start + (end - start) * j / rowSubCells;
                    DrawLineStyle(ds, grid.Left, y, grid.Left + subTickLeft, y, colors.RowColor, (float)RowSubTickLineWidth, WaveGridBorderTickLineStyle);
                    DrawLineStyle(ds, grid.Right, y, grid.Right - subTickRight, y, colors.RowColor, (float)RowSubTickLineWidth, WaveGridBorderTickLineStyle);
                }
            }

            // 列方向子刻度（水平线）
            var colSegments = new List<(float start, float end)>();
            colSegments.Add((grid.Left, colPositions.First()));
            for (int i = 0; i < colPositions.Count - 1; i++)
                colSegments.Add((colPositions[i], colPositions[i + 1]));
            colSegments.Add((colPositions.Last(), grid.Right));

            foreach (var (start, end) in colSegments)
            {
                for (int j = 1; j <= colSubTicksPerSegment; j++)
                {
                    float x = start + (end - start) * j / colSubCells;
                    DrawLineStyle(ds, x, grid.Top, x, grid.Top + subTickTop, colors.ColColor, (float)ColumnSubTickLineWidth, WaveGridBorderTickLineStyle);
                    DrawLineStyle(ds, x, grid.Bottom, x, grid.Bottom - subTickBottom, colors.ColColor, (float)ColumnSubTickLineWidth, WaveGridBorderTickLineStyle);
                }
            }
        }


        private void DrawTickSegments(CanvasDrawingSession ds, List<float> positions, float start, float end, int centerIndex, Windows.UI.Color normalColor, Windows.UI.Color centerColor, bool horizontal)
        {
            foreach (var (pos, i) in positions.Select((p, idx) => (p, idx)))
            {
                var color = (i == centerIndex) ? centerColor : normalColor;
                if (horizontal)
                    DrawLineStyle(ds, start, pos, end, pos, color, 1.5f, WaveGridBorderTickLineStyle);
                else
                    DrawLineStyle(ds, pos, start, pos, end, color, 1.5f, WaveGridBorderTickLineStyle);
            }
        }
        private void DrawLineStyle(CanvasDrawingSession ds, float x1, float y1, float x2, float y2, Windows.UI.Color color, float width, LineStyle style)
        {
            if (style == LineStyle.None)
                return;

            if (ViewDrawMode == DrawMode.BranchPrediction)
            {
                switch (style)
                {
                    case LineStyle.Solid:
                        ds.DrawLine(x1, y1, x2, y2, color, width);
                        return;
                    case LineStyle.Dot:
                        DrawDottedLine(ds, x1, y1, x2, y2, color, width);
                        return;
                    case LineStyle.Dash:
                        DrawDashedLine(ds, x1, y1, x2, y2, color, width);
                        return;
                }
            }
            else
            {
                CanvasStrokeStyle? strokeStyle = style switch
                {
                    LineStyle.Solid => null,
                    LineStyle.Dash => new CanvasStrokeStyle { DashStyle = CanvasDashStyle.Dash },
                    LineStyle.Dot => new CanvasStrokeStyle
                    {
                        DashStyle = CanvasDashStyle.Solid,
                        CustomDashStyle = [0.1f, 3f],
                        DashCap = CanvasCapStyle.Round
                    },
                    _ => null
                };

                if (strokeStyle != null)
                    ds.DrawLine(x1, y1, x2, y2, color, width, strokeStyle);
                else
                    ds.DrawLine(x1, y1, x2, y2, color, width);
            }

        }

        private void DrawDottedLine(CanvasDrawingSession ds, float x1, float y1, float x2, float y2, Windows.UI.Color color, float width)
        {
            // 预计算方向向量和长度
            float dx = x2 - x1;
            float dy = y2 - y1;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            float dotSpacing = 4f;
            int dotCount = Math.Max(1, (int)(length / dotSpacing));
            float step = 1f / dotCount;
            float halfStep = step * 0.5f;

            // 批量计算所有点
            float dotSize = halfStep * length; // 每个点的实际长度

            for (int i = 0; i < dotCount; i++)
            {
                float t = i * step;
                float sx = x1 + dx * (t);
                float sy = y1 + dy * t;
                float ex = x1 + dx * (t + halfStep);
                float ey = y1 + dy * (t + halfStep);
                ds.DrawLine(sx, sy, ex, ey, color, width);
            }
        }

        private void DrawDashedLine(CanvasDrawingSession ds, float x1, float y1, float x2, float y2, Windows.UI.Color color, float width)
        {
            // 预计算方向向量和长度
            float dx = x2 - x1;
            float dy = y2 - y1;
            float length = (float)Math.Sqrt(dx * dx + dy * dy);
            float dashLength = 8f;
            float gapLength = 4f;
            float segmentLength = dashLength + gapLength;

            float drawn = 0f;
            while (drawn < length)
            {
                float t1 = drawn / length;
                float t2 = Math.Min((drawn + dashLength) / length, 1f);

                float sx = x1 + dx * t1;
                float sy = y1 + dy * t1;
                float ex = x1 + dx * t2;
                float ey = y1 + dy * t2;

                ds.DrawLine(sx, sy, ex, ey, color);
                drawn += segmentLength;
            }
        }
    }
}
