using IVisio = Microsoft.Office.Interop.Visio;

namespace VisioScripting.Commands
{
    public class ArrangeCommands : CommandSet
    {
        internal ArrangeCommands(Client client) :
            base(client)
        {

        }

        public void Nudge(TargetSelection targetselection, double dx, double dy)
        {
            if (dx == 0.0 && dy == 0.0)
            {
                return;
            }

            var cmdtarget = this._client.GetCommandTargetDocument();

            var activeapp = new VisioScripting.TargetActiveApplication();
            using (var undoscope = this._client.Undo.NewUndoScope(activeapp, nameof(Nudge)))
            {
                var window = cmdtarget.Application.ActiveWindow;
                var selection = window.Selection;
                var unitcode = IVisio.VisUnitCodes.visInches;

                // Move method: http://msdn.microsoft.com/en-us/library/ms367549.aspx   
                selection.Move(dx, dy, unitcode);
            }
        }

        private static void _send_selection(IVisio.Selection selection, Models.ShapeSendDirection dir)
        {

            if (dir == Models.ShapeSendDirection.ToBack)
            {
                selection.SendToBack();
            }
            else if (dir == Models.ShapeSendDirection.Backward)
            {
                selection.SendBackward();
            }
            else if (dir == Models.ShapeSendDirection.Forward)
            {
                selection.BringForward();
            }
            else if (dir == Models.ShapeSendDirection.ToFront)
            {
                selection.BringToFront();
            }
        }


        public void Send(Models.ShapeSendDirection dir)
        {
            var cmdtarget = this._client.GetCommandTargetDocument();
            var window = cmdtarget.Application.ActiveWindow;
            var selection = window.Selection;
            ArrangeCommands._send_selection(selection, dir);
        }

        public void AlignHorizontal(TargetSelection targetselection, Models.AlignmentHorizontal align)
        {
            var cmdtarget = this._client.GetCommandTargetDocument();

            IVisio.VisHorizontalAlignTypes halign;
            var valign = IVisio.VisVerticalAlignTypes.visVertAlignNone;

            switch (align)
            {
                case VisioScripting.Models.AlignmentHorizontal.Left:
                    halign = IVisio.VisHorizontalAlignTypes.visHorzAlignLeft;
                    break;
                case VisioScripting.Models.AlignmentHorizontal.Center:
                    halign = IVisio.VisHorizontalAlignTypes.visHorzAlignCenter;
                    break;
                case VisioScripting.Models.AlignmentHorizontal.Right:
                    halign = IVisio.VisHorizontalAlignTypes.visHorzAlignRight;
                    break;
                default: throw new System.ArgumentOutOfRangeException();
            }

            const bool glue_to_guide = false;

            var activeapp = new VisioScripting.TargetActiveApplication();
            using (var undoscope = this._client.Undo.NewUndoScope(activeapp, nameof(AlignHorizontal)))
            {
                var window = cmdtarget.Application.ActiveWindow;
                var selection = window.Selection;
                selection.Align(halign, valign, glue_to_guide);
            }
        }

        public void AlignVertical(TargetSelection targetselection, Models.AlignmentVertical align)
        {
            var cmdtarget = this._client.GetCommandTargetDocument();

            // Set the align enums
            var halign = IVisio.VisHorizontalAlignTypes.visHorzAlignNone;
            IVisio.VisVerticalAlignTypes valign;
            switch (align)
            {
                case VisioScripting.Models.AlignmentVertical.Top:
                    valign = IVisio.VisVerticalAlignTypes.visVertAlignTop;
                    break;
                case VisioScripting.Models.AlignmentVertical.Center:
                    valign = IVisio.VisVerticalAlignTypes.visVertAlignMiddle;
                    break;
                case VisioScripting.Models.AlignmentVertical.Bottom:
                    valign = IVisio.VisVerticalAlignTypes.visVertAlignBottom;
                    break;
                default: throw new System.ArgumentOutOfRangeException();
            }

            const bool glue_to_guide = false;

            // Perform the alignment
            var activeapp = new VisioScripting.TargetActiveApplication();
            using (var undoscope = this._client.Undo.NewUndoScope(activeapp, nameof(AlignVertical)))
            {
                var window = cmdtarget.Application.ActiveWindow;
                var selection = window.Selection;
                selection.Align(halign, valign, glue_to_guide);
            }
        }

        public void DistributenOnAxis(TargetShapes targetshapes, Models.Axis axis, double spacing)
        {
            var cmdtarget = this._client.GetCommandTargetPage();

            var page = cmdtarget.ActivePage;
            targetshapes = targetshapes.Resolve(this._client);
            var targetshapeids = targetshapes.ToShapeIDs();
            var activeapp = new VisioScripting.TargetActiveApplication();
            using (var undoscope = this._client.Undo.NewUndoScope(activeapp, nameof(DistributeOnAxis)))
            {
                VisioScripting.Helpers.ArrangeHelper._distribute_with_spacing(page, targetshapeids, axis, spacing);
            }
        }

        public void DistributeOnAxis(VisioScripting.TargetSelection targetselection, Models.Axis axis)
        {
            var cmdtarget = this._client.GetCommandTargetPage();

            IVisio.VisUICmds cmd;

            switch (axis)
            {
                case VisioScripting.Models.Axis.XAxis:
                    cmd = IVisio.VisUICmds.visCmdDistributeHSpace;
                    break;
                case VisioScripting.Models.Axis.YAxis:
                    cmd = IVisio.VisUICmds.visCmdDistributeVSpace;
                    break;
                default:
                    throw new System.ArgumentOutOfRangeException();
            }

            var activeapp = new VisioScripting.TargetActiveApplication();
            using (var undoscope = this._client.Undo.NewUndoScope(activeapp, nameof(DistributeOnAxis)))
            {
                cmdtarget.Application.DoCmd((short) cmd);
            }
        }

        public void DistributeHorizontal(TargetShapes targetshapes, Models.AlignmentHorizontal halign)
        {
            var cmdtarget = this._client.GetCommandTargetDocument();

            int shape_count = targetshapes.SelectShapesAndCount(this._client);
            if (shape_count < 1)
            {
                return;
            }

            IVisio.VisUICmds cmd;

            switch (halign)
            {
                case VisioScripting.Models.AlignmentHorizontal.Left:
                    cmd = IVisio.VisUICmds.visCmdDistributeLeft;
                    break;
                case VisioScripting.Models.AlignmentHorizontal.Center:
                    cmd = IVisio.VisUICmds.visCmdDistributeCenter;
                    break;
                case VisioScripting.Models.AlignmentHorizontal.Right:
                    cmd = IVisio.VisUICmds.visCmdDistributeRight;
                    break;
                default: throw new System.ArgumentOutOfRangeException();
            }

            cmdtarget.Application.DoCmd((short) cmd);
        }

        public void DistributeVertical(TargetShapes targetshapes, Models.AlignmentVertical valign)
        {
            var cmdtarget = this._client.GetCommandTargetDocument();

            int shape_count = targetshapes.SelectShapesAndCount(this._client);
            if (shape_count < 1)
            {
                return;
            }

            IVisio.VisUICmds cmd;
            switch (valign)
            {
                case VisioScripting.Models.AlignmentVertical.Top:
                    cmd = IVisio.VisUICmds.visCmdDistributeTop;
                    break;
                case VisioScripting.Models.AlignmentVertical.Center:
                    cmd = IVisio.VisUICmds.visCmdDistributeMiddle;
                    break;
                case VisioScripting.Models.AlignmentVertical.Bottom:
                    cmd = IVisio.VisUICmds.visCmdDistributeBottom;
                    break;
                default: throw new System.ArgumentOutOfRangeException();
            }

            cmdtarget.Application.DoCmd((short) cmd);

        }
    }
}