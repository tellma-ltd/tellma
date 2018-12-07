using System.Collections.Generic;

namespace BSharp.Services.ImportExport
{
    public class AbstractDataFile
    {
        // Sheets of rows of cells
        private readonly List<AbstractDataSheet> _data = new List<AbstractDataSheet>();

        public string this[int sheet, int row, int col]
        {
            get
            {
                return "";
            }
        }

        public void AddSheet(int rowCapacity, int columnCapacity)
        {
            _data.Add(new AbstractDataSheet(rowCapacity, columnCapacity));
        }

        private class AbstractDataSheet
        {
            private readonly List<List<AbstractDataCell>> _data = new List<List<AbstractDataCell>>();
            private readonly int _rowCapacity = 1;
            private readonly int _columnCapacity = 1;

            public AbstractDataSheet()
            {

            }

            public AbstractDataSheet(int rowCapacity, int columnCapacity)
            {
                _rowCapacity = rowCapacity;
                _columnCapacity = columnCapacity;
            }

            public void AddRow()
            {

            }

            //public AbstractDataRow this[int index]
            //{
            //    get
            //    {
            //        return _data[index];
            //    }
            //}
        }

        private class AbstractDataRow
        {
            private readonly List<AbstractDataRow> _data = new List<AbstractDataRow>();
            //public AbstractDataCell this[int index]
            //{
            //    get
            //    {
            //        return _data[index];
            //    }
            //}
        }

        private class AbstractDataCell
        {
            public string Content { get; set; }


        }
    }
}
