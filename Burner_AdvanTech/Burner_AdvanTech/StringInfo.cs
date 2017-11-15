using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Burner_AdvanTech
{
    class StringInfo
    {
        public List<string> StringCollection { get; set; }
        public StringInfo()
        {
            StringCollection = new List<string>();

            for(int i = 0;i < new Global().stringnumber;i++)
            {
                StringCollection.Add("string" + (i + 1).ToString());
            }
        }

    }

    public class VolTemp
    {
        public string CellNumber { set; get; }
        public string Vol { set; get; }
        public string Temp { set; get; }

        public bool BalState { set; get; }
    }

    class BPinfo
    {
        public List<string> BpCollection { get; set; }
        public BPinfo()
        {
            BpCollection = new List<string>();
            for(int i = 0;i < new Global().bpnumber;i++)
            {
                BpCollection.Add("Bp" + (i + 1).ToString());
            }
        }
    }

    public class StringFirmwareInfor
    {
        public string NodeAddress { get; set; }

        public string FirmwareType { get; set; }

        public string FirmwareVersion { get; set; }

        public bool OnlineStatus { get; set; }

        public int BurningProgress { get; set; }
    }


    public static class DataGridOp
    {
        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            int numVisuals = 0;
            T child = default(T);
       //     int numVisuals = VisualTreeHelper.GetChildrenCount(parent);

            parent.Dispatcher.Invoke(() => {
                  numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            });
            for (int i = 0; i < numVisuals; i++)
            {

                Visual v = null;
                parent.Dispatcher.Invoke(() => {
                    v = (Visual)VisualTreeHelper.GetChild(parent, i);
                });
                 
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }
                if (child != null)
                {
                    break;
                }
            }
            return child;
        }

        public static DataGridRow GetSelectedRow(this DataGrid grid)
        {
            return (DataGridRow)grid.ItemContainerGenerator.ContainerFromItem(grid.SelectedItem);
        }
        public static DataGridRow GetRow(this DataGrid grid, int index)
        {
            DataGridRow row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            if (row == null)
            {
                // May be virtualized, bring into view and try again.
                grid.UpdateLayout();
                grid.ScrollIntoView(grid.Items[index]);
                row = (DataGridRow)grid.ItemContainerGenerator.ContainerFromIndex(index);
            }
            return row;
        }

        public static DataGridCell GetCell(this DataGrid grid, DataGridRow row, int column)
        {
            if (row != null)
            {
                DataGridCellsPresenter presenter = GetVisualChild<DataGridCellsPresenter>(row);

                if (presenter == null)
                {
                    grid.ScrollIntoView(row, grid.Columns[column]);
                    presenter = GetVisualChild<DataGridCellsPresenter>(row);
                }

                DataGridCell cell = (DataGridCell)presenter.ItemContainerGenerator.ContainerFromIndex(column);
                return cell;
            }
            return null;
        }

        public static DataGridCell GetCell(this DataGrid grid, int row, int column)
        {
            DataGridRow rowContainer = grid.GetRow(row);
            return grid.GetCell(rowContainer, column);
        }

      
    }

    class Uptext
    {
         public string TextContent
        {
            get;set;
        }
        
    }
}
