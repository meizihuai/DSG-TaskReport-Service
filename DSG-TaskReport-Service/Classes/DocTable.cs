using Novacode;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DSG_TaskReport_Service
{
    class DocTable
    {
        public Table tab;
        private int rowCount;
        private int columnCount;
        private int insertedCount;
        public DocTable(DocX doc,int rowCount, string[] columns)
        {
            this.rowCount = rowCount;
            insertedCount = 0;
            this.columnCount = columns.Length;
            tab = doc.AddTable(rowCount + 1, columns.Length);
            tab.Design = TableDesign.TableGrid;
            tab.Alignment = Alignment.center;
            for (int i = 0; i < columns.Length; i++)
            {
                tab.Rows[0].Cells[i].FillColor = Color.FromArgb(226, 226, 226);
                tab.Rows[0].Cells[i].Paragraphs[0].Append(columns[i]).Bold().FontSize(12);
            }        
        }
        public void InsertRow(object[] obj)
        {
            if (insertedCount > rowCount) return;
            insertedCount++;
            int j = columnCount;
            if (obj.Length < j) j = obj.Length;
            for(int i = 0; i < j; i++)
            {
                tab.Rows[insertedCount].Cells[i].Paragraphs[0].Append(obj[i].ToString()).FontSize(12);
            }         
        }
      
    }
}
