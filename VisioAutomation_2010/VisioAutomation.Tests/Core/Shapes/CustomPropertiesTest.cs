﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using VisioAutomation.Extensions;
using VisioAutomation.Shapes;
using VisioAutomation.ShapeSheet;

namespace VisioAutomation_Tests.Core.Shapes
{
    [TestClass]
    public class CustomPropertiesTest : VisioAutomationTest
    {
        [TestMethod]
        public void CustomProps_Names()
        {
            Assert.IsFalse(CustomPropertyHelper.IsValidName(null));
            Assert.IsFalse(CustomPropertyHelper.IsValidName(string.Empty));
            Assert.IsFalse(CustomPropertyHelper.IsValidName(" foo "));
            Assert.IsFalse(CustomPropertyHelper.IsValidName("foo "));
            Assert.IsFalse(CustomPropertyHelper.IsValidName("foo\t"));
            Assert.IsFalse(CustomPropertyHelper.IsValidName("fo bar"));
            Assert.IsTrue(CustomPropertyHelper.IsValidName("foobar"));
        }

        [TestMethod]
        public void CustomProps_CRUD()
        {
            var page1 = this.GetNewPage();

            var s1 = page1.DrawRectangle(0, 0, 1, 1);
            s1.Text = "Checking for Custom Properties";

            // A new rectangle should have zero props
            var c0 = CustomPropertyHelper.GetCells(s1, CellValueType.Formula);
            Assert.AreEqual(0, c0.Count);


            int cp_type = 0; // 0 for string

            // Set one property
            // Notice that the properties some back double-quoted
            CustomPropertyHelper.Set(s1, "PROP1", "\"VAL1\"", cp_type);
            var c1 = CustomPropertyHelper.GetCells(s1, CellValueType.Formula);
            Assert.AreEqual(1, c1.Count);
            Assert.IsTrue(c1.ContainsKey("PROP1"));
            Assert.AreEqual("\"VAL1\"", c1["PROP1"].Value.Value);

            // Add another property
            CustomPropertyHelper.Set(s1, "PROP2", "\"VAL 2\"", cp_type);
            var c2 = CustomPropertyHelper.GetCells(s1, CellValueType.Formula);
            Assert.AreEqual(2, c2.Count);
            Assert.IsTrue(c2.ContainsKey("PROP1"));
            Assert.AreEqual("\"VAL1\"", c2["PROP1"].Value.Value);
            Assert.IsTrue(c2.ContainsKey("PROP2"));
            Assert.AreEqual("\"VAL 2\"", c2["PROP2"].Value.Value);

            // Modify the value of the second property
            CustomPropertyHelper.Set(s1, "PROP2", "\"VAL 2 MOD\"", cp_type);
            var c3 = CustomPropertyHelper.GetCells(s1, CellValueType.Formula);
            Assert.AreEqual(2, c3.Count);
            Assert.IsTrue(c3.ContainsKey("PROP1"));
            Assert.AreEqual("\"VAL1\"", c3["PROP1"].Value.Value);
            Assert.IsTrue(c3.ContainsKey("PROP2"));
            Assert.AreEqual("\"VAL 2 MOD\"", c3["PROP2"].Value.Value);

            // Now delete all the custom properties
            foreach (string name in c3.Keys)
            {
                CustomPropertyHelper.Delete(s1, name);
            }
            var c4 = CustomPropertyHelper.GetCells(s1, CellValueType.Formula);
            Assert.AreEqual(0, c4.Count);

            var app = this.GetVisioApplication();
            var doc = app.ActiveDocument;
            if (doc != null)
            {
                doc.Close(true);
            }
        }

        [TestMethod]
        public void CustomProps_AllTypes()
        {
            var page1 = this.GetNewPage();
            var s1 = page1.DrawRectangle(0, 0, 1, 1);
            s1.Text = "Checking for Custom Properties";
            
            // String Custom Property
            var prop_string_in = new CustomPropertyCells();
            prop_string_in.Format = "\"Format\"";
            prop_string_in.Label = "\"Label\"";
            prop_string_in.Prompt = "\"Prompt\"";
            prop_string_in.Type = CustomPropertyCells.CustomPropertyTypeToInt(CustomPropertyType.String);
            prop_string_in.Value = "1";

            // Boolean
            var prop_bool_in = new CustomPropertyCells();
            prop_bool_in.Format = "\"Format\"";
            prop_bool_in.Label = "\"Label\"";
            prop_bool_in.Prompt = "\"Prompt\"";
            prop_bool_in.Type = CustomPropertyCells.CustomPropertyTypeToInt(CustomPropertyType.Boolean);
            prop_bool_in.Value = true;

            CustomPropertyHelper.Set(s1, "PROP_STRING", prop_string_in);
            CustomPropertyHelper.Set(s1, "PROP_BOOLEAN", prop_bool_in);

            var props_dic = CustomPropertyHelper.GetCells(s1, CellValueType.Formula);

            var prop_string_out = props_dic["PROP_STRING"];

            Assert.AreEqual("\"Format\"", prop_string_out.Format.Value);
            Assert.AreEqual("\"Label\"", prop_string_out.Label.Value);
            Assert.AreEqual("\"Prompt\"", prop_string_out.Prompt.Value);
            Assert.AreEqual("0", prop_string_out.Type.Value);
            Assert.AreEqual("1", prop_string_out.Value.Value);

            var prop_bool_out = props_dic["PROP_BOOLEAN"];
            Assert.AreEqual("\"Format\"", prop_bool_out.Format.Value);
            Assert.AreEqual("\"Label\"", prop_bool_out.Label.Value);
            Assert.AreEqual("\"Prompt\"", prop_bool_out.Prompt.Value);
            Assert.AreEqual("3", prop_bool_out.Type.Value);
            Assert.AreEqual("TRUE", prop_bool_out.Value.Value);

            var app = this.GetVisioApplication();
            var doc = app.ActiveDocument;
            if (doc != null)
            {
                doc.Close(true);
            }
        }
    }
}