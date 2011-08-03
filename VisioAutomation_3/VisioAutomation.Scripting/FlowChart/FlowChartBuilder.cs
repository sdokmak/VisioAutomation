using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using VA = VisioAutomation;
using IVisio= Microsoft.Office.Interop.Visio;
namespace VisioAutomation.Scripting.FlowChart
{
    public class FlowChartBuilder
    {
        public static IList<VA.Layout.MSAGL.Drawing> LoadFromXML(Session scriptingsession, string filename)
        {
            var xmldoc = XDocument.Load(filename);
            return LoadFromXML(scriptingsession, xmldoc);
        }

        public static IList<VA.Layout.MSAGL.Drawing> LoadFromXML(Session scriptingsession, XDocument xmldoc)
        {
            var drawings = new List<VA.Layout.MSAGL.Drawing>();
            bool major_error = false;
            var page_els = xmldoc.Root.Elements("page");
            foreach (var page_el in page_els)
            {
                var node_ids = new HashSet<string>();
                var con_ids = new HashSet<string>();

                var renderer = new Layout.MSAGL.DirectedGraphLayout();
                var renderoptions_el = page_el.Element("renderoptions");
                GetRenderOptionsFromXml(renderoptions_el, renderer);

                var drawing = new Layout.MSAGL.Drawing();
                var shape_els = page_el.Element("shapes").Elements("shape");
                var con_els = page_el.Element("connectors").Elements("connector");

                var shape_infos = shape_els.Select(e => ShapeInfo.FromXml(scriptingsession, e)).ToList();
                var con_infos = con_els.Select(e => ConnectorInfo.FromXml(scriptingsession, e)).ToList();

                // ANALYZE 1

                scriptingsession.Write(VA.Scripting.OutputStream.Verbose,"Analyzing shape data...");
                foreach (var shape_info in shape_infos)
                {
                    scriptingsession.Write(VA.Scripting.OutputStream.Verbose, "shape {0}", shape_info.ID);

                    if (node_ids.Contains(shape_info.ID))
                    {
                        scriptingsession.Write(VA.Scripting.OutputStream.Verbose, "ERROR: Node \"{0}\" is already defined", shape_info.ID);
                        major_error = true;
                    }
                    else
                    {
                        node_ids.Add(shape_info.ID);
                    }
                }

                scriptingsession.Write(VA.Scripting.OutputStream.Verbose, "Analyzing connector data...");
                foreach (var con_info in con_infos)
                {
                    scriptingsession.Write(VA.Scripting.OutputStream.Verbose, "connector {0}", con_info.ID);

                    if (con_ids.Contains(con_info.ID))
                    {
                        scriptingsession.Write(VA.Scripting.OutputStream.Verbose,"ERROR: Connector \"{0}\" is already defined", con_info.ID);
                        major_error = true;
                    }
                    else
                    {
                        con_ids.Add(con_info.ID);
                    }

                    if (!node_ids.Contains(con_info.From))
                    {
                        scriptingsession.Write(VA.Scripting.OutputStream.Verbose,
                            "ERROR: Connector \"{0}\" references a nonexistent FROM Node \"{1}\"",
                            con_info.ID, con_info.From);
                        major_error = true;
                    }

                    if (!node_ids.Contains(con_info.To))
                    {
                        scriptingsession.Write(VA.Scripting.OutputStream.Verbose,
                            "ERROR: Connector \"{0}\" references a nonexistent TO Node \"{1}\"",
                            con_info.ID, con_info.To);
                        major_error = true;
                    }
                }

                if (major_error)
                {
                    scriptingsession.Write(VA.Scripting.OutputStream.Verbose,"Errors encountered in shape data. Stopping.");
                    System.Environment.Exit(-1);
                }

                scriptingsession.Write(VA.Scripting.OutputStream.Verbose,"Creating shape AutoLayout nodes");
                foreach (var shape_info in shape_infos)
                {
                    var al_shape = drawing.AddShape(shape_info.ID, shape_info.Label, shape_info.Stencil,
                                                    shape_info.Master);
                    al_shape.URL = shape_info.URL;
                    al_shape.CustomProperties = new Dictionary<string, VA.CustomProperties.CustomPropertyCells>();
                    foreach (var kv in shape_info.custprops)
                    {
                        al_shape.CustomProperties[kv.Key] = kv.Value;
                    }
                }

                scriptingsession.Write(VA.Scripting.OutputStream.Verbose,"Creating connector AutoLayout nodes");
                foreach (var con_info in con_infos)
                {
                    var def_connector_type = VA.Connections.ConnectorType.Curved;
                    var connectory_type = def_connector_type;

                    var from_shape = drawing.FindShape(con_info.From);
                    var to_shape = drawing.FindShape(con_info.To);

                    var def_con_color = new VA.Drawing.ColorRGB(0x000000);
                    var def_con_weight = 1.0 / 72.0;
                    var def_end_arrow = 2;
                    var al_connector = drawing.Connect(con_info.ID, from_shape, to_shape, con_info.Label,
                                                       connectory_type);

                    al_connector.ShapeCells = new VA.DOM.ShapeCells();
                    al_connector.ShapeCells.LineColor = VA.Convert.ColorToFormulaRGB(con_info.Element.AttributeAsColor("color", def_con_color));
                    al_connector.ShapeCells.LineWeight = con_info.Element.AttributeAsInches("weight", def_con_weight);
                    al_connector.ShapeCells.EndArrow = def_end_arrow;
                }

                scriptingsession.Write(VA.Scripting.OutputStream.Verbose,"Rendering AutoLayout...");

                drawings.Add(drawing);
                scriptingsession.Write(VA.Scripting.OutputStream.Verbose,"Finished rendering AutoLayout");
            }

            return drawings;
        }

        public static void RenderDiagrams(
            VA.Scripting.Session scriptingsession,
            IList<VA.Layout.MSAGL.Drawing> drawings)
        {
            scriptingsession.Write(VA.Scripting.OutputStream.Verbose,"Start Rendering FlowChart");
            var app = scriptingsession.VisioApplication;


            if (drawings.Count < 1)
            {
                return;
            }

            var doc = scriptingsession.Document.New();

            int num_expected_pages = drawings.Count;


            foreach (int i in Enumerable.Range(0, drawings.Count))
            {
                var diagram = drawings[i];


                scriptingsession.Write(VA.Scripting.OutputStream.Verbose,"Rendering page: {0}", i+1);

                var options = new Layout.MSAGL.LayoutOptions();
                options.UseDynamicConnectors = false;

                IVisio.Page page = null;
                if (doc.Pages.Count == 1)
                {
                    page = app.ActivePage;
                }
                else
                {
                    page = doc.Pages.Add();
                }

                diagram.Render(page, options);
                scriptingsession.Page.ResizeToFitContents(new VA.Drawing.Size(1.0, 1.0), true);
                scriptingsession.View.Zoom(VA.Scripting.Zoom.ToPage);

                scriptingsession.Write(VA.Scripting.OutputStream.Verbose,"Finished rendering page");
            }

            scriptingsession.Write(VA.Scripting.OutputStream.Verbose,"Finished rendering pages");
            scriptingsession.Write(VA.Scripting.OutputStream.Verbose,"Finished rendering flowchart.");
        }

        private static void GetRenderOptionsFromXml(XElement el, Layout.MSAGL.DirectedGraphLayout directed_graph_layout)
        {
            System.Func<string, bool> bool_converter = s => bool.Parse(s);
            System.Func<string, int> int_converter = s => int.Parse(s);
            System.Func<string, double> double_converter = (s) => double.Parse(s);

            directed_graph_layout.LayoutOptions.UseDynamicConnectors = VA.Scripting.XmlUtil.GetAttributeValue(el,"usedynamicconnectors", bool_converter);
            directed_graph_layout.LayoutOptions.ScalingFactor = VA.Scripting.XmlUtil.GetAttributeValue(el,"scalingfactor", double_converter);
        }
    }
}