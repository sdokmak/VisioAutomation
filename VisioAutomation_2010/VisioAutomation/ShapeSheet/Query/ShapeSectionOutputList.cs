﻿using System.Collections.Generic;
using System.Linq;
using IVisio = Microsoft.Office.Interop.Visio;

namespace VisioAutomation.ShapeSheet.Query
{
    public class ShapeSectionOutputList<T> : OutputBase,  IEnumerable<ShapeSectionOutput<T>>
    {
        // for a given shape, contains the outputs for every section

        private List<ShapeSectionOutput<T>> _items;

        internal ShapeSectionOutputList(int shape_id, int count, List<ShapeSectionOutput<T>> sections) : base(shape_id, count) 
        {
            this._items = sections;
        }

        public IEnumerator<ShapeSectionOutput<T>> GetEnumerator()
        {
            return this._items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this._items.Count;
            }
        }

        public ShapeSectionOutput<T> this[int index]
        {
            get
            {
                return this._items[index];
            }
        }

    }
}