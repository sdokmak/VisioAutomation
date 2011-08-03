using System;
using System.Collections.Generic;
using VisioAutomation.Extensions;
using IVisio = Microsoft.Office.Interop.Visio;
using VA = VisioAutomation;
using System.Linq;

namespace VisioAutomation.Scripting.Commands
{
    public class SelectionCommands : CommandSet
    {
        public SelectionCommands(Session session) :
            base(session)
        {

        }
        
        public IEnumerable<IVisio.Shape> EnumShapes()
        {
            var app = this.Session.VisioApplication;
            var activewin = app.ActiveWindow;
            var sel = activewin.Selection;

            var shapes = sel.AsEnumerable();
            return shapes;
        }

        public IEnumerable<IVisio.Shape> EnumShapes2D()
        {
            var shapes = this.EnumShapes().Where(s => s.OneD == 0);
            return shapes;
        }

        public IVisio.Selection Get()
        {
            var application = this.Session.VisioApplication;
            var active_window = application.ActiveWindow;
            var selection = active_window.Selection;
            return selection;
        }

        public void SelectAll()
        {
            if (!this.Session.HasActiveDrawing)
            {
                return;
            }

            var active_window = this.Session.View.GetActiveWindow();
            active_window.SelectAll();
        }

        public void SelectInvert()
        {
            if (!this.Session.HasActiveDrawing)
            {
                return;
            }

            var application = this.Session.VisioApplication;
            var active_page = application.ActivePage;
            var shapes = active_page.Shapes;
            if (shapes.Count < 1)
            {
                return;
            }

            Invert(application.ActiveWindow);
        }

        public static void Invert(IVisio.Window window)
        {
            if (window == null)
            {
                throw new System.ArgumentNullException("window");
            }

            if (window.Page == null)
            {
                throw new System.ArgumentException("Window has null page", "window");
            }

            var page = (IVisio.Page) window.Page;
            var shapes = page.Shapes;
            var all_shapes = shapes.AsEnumerable();
            var selection = window.Selection;
            var selected_set = new System.Collections.Generic.HashSet<IVisio.Shape>(selection.AsEnumerable());
            var shapes_to_select = all_shapes.Where(shape => !selected_set.Contains(shape)).ToList();

            window.DeselectAll();
            window.Select(shapes_to_select, IVisio.VisSelectArgs.visSelect);
        }

        public void SelectNone()
        {
            if (!this.Session.HasActiveDrawing)
            {
                return;
            }

            var application = this.Session.VisioApplication;
            var active_window = application.ActiveWindow;
            active_window.DeselectAll();
            active_window.DeselectAll();
        }

        public void Select(IVisio.Shape shape)
        {
            if (shape == null)
            {
                throw new ArgumentNullException("shape");
            }

            if (!this.Session.HasActiveDrawing)
            {
                return;
            }

            var application = this.Session.VisioApplication;
            var active_window = application.ActiveWindow;
            active_window.Select(shape, (short) IVisio.VisSelectArgs.visSelect);
        }

        public void Select(IEnumerable<IVisio.Shape> shapes)
        {
            if (shapes == null)
            {
                throw new ArgumentNullException("shapes");
            }

            if (!this.Session.HasActiveDrawing)
            {
                return;
            }

            var application = this.Session.VisioApplication;
            var active_window = application.ActiveWindow;
            active_window.Select(shapes, IVisio.VisSelectArgs.visSelect);
        }

        public void Select(IEnumerable<int> shapeids)
        {
            if (shapeids == null)
            {
                throw new ArgumentNullException("shapeids");
            }

            if (!this.Session.HasActiveDrawing)
            {
                return;
            }

            var application = this.Session.VisioApplication;
            var active_window = application.ActiveWindow;
            var page = application.ActivePage;
            var page_shapes = page.Shapes;
            var shapes = shapeids.Select(id => page_shapes.ItemFromID[id]).ToList();
            active_window.Select(shapes, IVisio.VisSelectArgs.visSelect);
        }
        
        public void SubSelect(IList<IVisio.Shape> shapes)
        {
            if (shapes == null)
            {
                throw new ArgumentNullException("shapes");
            }

            if (!this.Session.HasActiveDrawing)
            {
                return;
            }

            this.Session.VisioApplication.ActiveWindow.Select(shapes, IVisio.VisSelectArgs.visSubSelect);
        }

        public void SelectByMaster(IVisio.Master master)
        {
            if (!this.Session.HasActiveDrawing)
            {
                return;
            }

            var application = this.Session.VisioApplication;
            var page = application.ActivePage;
            // Get a selection of connectors, by master: 
            var selection = page.CreateSelection(
                IVisio.VisSelectionTypes.visSelTypeByMaster,
                IVisio.VisSelectMode.visSelModeSkipSub, 
                master);
        }

        public void SelectByLayer(string layername)
        {
            if (!this.Session.HasActiveDrawing)
            {
                return;
            }

            if (layername == null)
            {
                throw new ArgumentNullException("layername");
            }

            if (layername.Length < 1)
            {
                throw new ArgumentException("layername");
            }

            var layer = this.Session.Layer.GetLayer(layername);
            var application = this.Session.VisioApplication;
            var page = application.ActivePage;

            // Get a selection of connectors, by layer: 
            var selection = page.CreateSelection(
                IVisio.VisSelectionTypes.visSelTypeByLayer,
                IVisio.VisSelectMode.visSelModeSkipSub, 
                layer);
        }

        public IList<IVisio.Shape> GetShapes(ShapesEnumeration enumerationtype)
        {
            if (!this.Session.HasSelectedShapes())
            {
                return new List<IVisio.Shape>(0);
            }

            var selection = this.Session.Selection.Get();
            return VA.SelectionHelper.GetSelectedShapes(selection, enumerationtype);
        }

        public IList<IVisio.Shape> GetSubSelectedShapes()
        {
            //http://www.visguy.com/2008/05/17/detect-sub-selected-shapes-programmatically/
            var shapes = new List<IVisio.Shape>(0);
            var sel = this.Session.Selection.Get();
            var original_itermode = sel.IterationMode;

            // normal selection
            sel.IterationMode = ((short)IVisio.VisSelectMode.visSelModeSkipSub) + ((short)IVisio.VisSelectMode.visSelModeSkipSuper);

            if (sel.Count > 0)
            {
                shapes.AddRange(sel.AsEnumerable());
            }

            // sub selection
            sel.IterationMode = ((short)IVisio.VisSelectMode.visSelModeOnlySub) + ((short)IVisio.VisSelectMode.visSelModeSkipSuper);
            if (sel.Count > 0)
            {
                shapes.AddRange(sel.AsEnumerable());
            }

            sel.IterationMode = original_itermode;
            return shapes;
        }

        public void Delete()
        {
            if (!this.Session.HasSelectedShapes())
            {
                return;
            }

            var selection = this.Get();
            selection.Delete();
        }

        public void Copy()
        {
            if (!this.Session.HasSelectedShapes())
            {
                return;
            }

            var flags = IVisio.VisCutCopyPasteCodes.visCopyPasteNormal;

            var selection = this.Get();
            selection.Copy(flags);
        }

        public void Duplicate()
        {
            if (!this.Session.HasSelectedShapes())
            {
                return;
            }
            var active_window = this.Session.View.GetActiveWindow();
            var selection = active_window.Selection;
            selection.Duplicate();
        }

        public bool HasShapes()
        {
            return HasShapes(1);
        }

        public bool HasShapes(int min_items)
        {
            this.Session.Write(OutputStream.Verbose, "Checking for at least {0} selected shapes", min_items);
            if (min_items <= 0)
            {
                throw new System.ArgumentOutOfRangeException("min_items");
            }

            if (!this.Session.HasActiveDrawing)
            {
                return false;
            }

            var application = this.Session.VisioApplication;
            var active_window = application.ActiveWindow;
            var selection = active_window.Selection;
            bool v = selection.Count >= min_items;
            return v;
        }
    }
}